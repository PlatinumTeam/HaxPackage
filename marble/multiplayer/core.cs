//-----------------------------------------------------------------------------
// core.cs
// Multiplayer Hax Package
// Copyright (C) 2012 The Multiplayer Team
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//------------------------------------------------------------------------------

// Jeff: used for port forwarding, ensure we have right port
package q {
   function setNetPort(%port) {
      $port = %port;
      return Parent::setNetPort(%port);
   }
};
activatePackage(q);


function portCheck() {
   cancel($portSchedule);
   if ($port !$= "28000")
      portInit(28000);
   $portSchedule = schedule(10000,0,"portCheck");
}

//-----------------------------------------------------------------------------

// Jeff: send chat over multiplayer server
function sendChatToServer() {
   if (!$MP::Chat::EnableChat)
      return;
   // Jeff: need chat message code
   %message = "";
   commandToServer('sendChat',%message);
}

// Jeff: grab the client sided marble, very useful for clients to have
//       some control of the marble object
function getMyMarble() {
   //No errors!
   if (!isObject(ServerConnection)) {
      return;
   }

   for (%i = 0; %i < ServerConnection.getCount(); %i ++) {
      %obj = ServerConnection.getObject(%i);
      if (%obj.getClassName() $= "Marble") {
         //HiGuy: If you are not the host, your marble is always the second one
         $Game::MyMarble = %obj;
         return %obj;
      }
   }
   return -1;
}

// Jeff & HiGuy: main loop for multiplayer stuff
function mpLoop() {
   cancel($Game::MPLoopSchedule);

   // Jeff & HiGuy: delta calculation
   if ($Game::LastLoop $= "")
      $Game::LastLoop = getRealTime();
   %delta = getRealTime() - $Game::LastLoop;
   $Game::LastLoop = getRealTime();
   $Game::MPLoopCount ++;

   // Jeff & HiGuy: update ghost rotation
   if (!!($Game::MPLoopCount % 3) && $MP::Core::RotateMarbles)
      rotLoop();

   // Jeff: update things that only hosts should update

   $Game::MPLoopSchedule = schedule($MP::Core::MPLoopDelta, 0, "mpLoop");
}

//HiGuy: This is the main ghost rotation loop.
//       Marble rotation data is sent to the server here, because the server
//       does not ghost this information to the clients, and thus does not know
//       a particular client's rotation
function rotLoop() {
   //No errors!
   if (!$MP::Core::RotateMarbles)
      return;
   if (!isObject(ServerConnection))
      return;
   if (!isObject($Game::MyMarble) && getMyMarble() == -1)
      return;
   //HiGuy: This saves bandwidth
   commandToServer('GR', getWords($Game::MyMarble.getTransform(), 3, 6));
}

//HiGuy: This hides the user's ghost, shows the user's marble, hides any "freddies.", starts the rotation loop, and eats your soul
function hideGhost() {
   resetGhosts();
   getMyMarble();
   buildGhostList();
   hideMyGhost();
   hideFreddies();
}

//HiGuy: All your hopes and loopy dreams will come true with this method. Do NOT fuck with it, it will bite you.
function startLoops() {
   mpLoop();
   rotLoop();
   updateGhostCollision();
   clientTweening();
   if ($HostingMP) {
      serverTweening();
      updateGhosts();
      oobLoop();
      portCheck();
   }
}

//HiGuy: Shows ALL the things
function resetGhosts() {
   if (!isObject(ServerConnection)) {
      return;
   }
   for (%i = 0; %i < ServerConnection.getCount(); %i ++) {
      %obj = ServerConnection.getObject(%i);
      if (%obj.getClassName() $= "StaticShape" && strStr(%obj.getDataBlock().shapeFile, "/balls/") != -1) {
         %obj.hide(false);
      }
      if (%obj.getClassName() $= "Marble") {
         %obj.setScale("1 1 1");
      }
   }
}

//HiGuy: Actually hides the ghost
function hideMyGhost() {
    //No errors!
   if (!isObject(ServerConnection)) {
      return;
   }

  //We need our marble to compare scales
   if (!isObject($Game::MyMarble) && getMyMarble() == -1)
      return;

   //Iterate!
   for (%i = 0; %i < ServerConnection.getCount(); %i ++) {
      %obj = ServerConnection.getObject(%i);

      //If it's a ghost...
      if (%obj.getClassName() $= "StaticShape" && strStr(%obj.getDataBlock().shapeFile, "/balls/") != -1) {
         //If it has the same scale as our marble
         if (getWord(%obj.getScale(), 0) $= $Game::MyScale) {
            //BAD GHOST
            %obj.hide(true);
            break;
         }
      }
   }
}

//HiGuy: Hides other peoples' Marbles (caps because they are of the Marble class)
function hideFreddies() {
   //No errors!
   if (!isObject(ServerConnection)) {
      return;
   }

   //We need our marble to compare marbles
   if (!isObject($Game::MyMarble) && getMyMarble() == -1)
      return;

   cancel($Game::HideFreddieSchedule);

   //Iterate!
   for (%i = 0; %i < ServerConnection.getCount(); %i ++) {
      %obj = ServerConnection.getObject(%i);

      //If it's a marble, and it's not us
      if (%obj.getClassName() $= "Marble" && %obj.getId() != $Game::MyMarble.getId()) {
         //Easy hiding, as we can't use .hide or the game will crash
         %obj.setScale("0 0 0");
         //Get them out of the way, we don't want shadows
         %obj.setTransform("0 0 -1000000 1 0 0 0");
      }
   }

   $Game::HideFreddieSchedule = schedule($MP::Core::HideFreddieDelta, 0, hideFreddies);
}

//HiGuy: Hacking finding of whose ghost is whose
function buildGhostList() {
   cancel($GhostListSchedule);

   //No errors!
   if (!isObject(ServerConnection)) {
      return;
   }

   //Reset!
   $Game::GhostCount = 0;

   //Iterate!
   for (%i = 0; %i < ServerConnection.getCount(); %i ++) {
      %obj = ServerConnection.getObject(%i);

      //If it's a ghost...
      if (%obj.getClassName() $= "StaticShape" && strStr(%obj.getDataBlock().shapeFile, "/balls/") != -1) {
         %ghostScale = getWord(%obj.getScale(), 0);
         if ($Game::GhostSize[%ghostScale] !$= "") {
            $Game::GhostMarble[$Game::GhostSize[%ghostScale]] = %obj;
            $Game::Ghosts[$Game::GhostCount] = %obj;
            $Game::GhostCount ++;
         }
      }
   }

   $GhostListSchedule = schedule($MP::Core::BuildGhostListDelta, 0, "buildGhostList");
}

function oobLoop() {
   cancel($OOBLoop);

   if (!$HostingMP)
      return;

   for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
      %client = ClientGroup.getObject(%i);
      if (%client.mouseFire && %client.player.getOOB()) {
         %client.player.onOOBClick();
         %client.schedule(250, "cancelRespawn");
      }
   }

   $OOBLoop = schedule($MP::Core::OOBLoopDelta, 0, "oobLoop");
}

//-----------------------------------------------------------------------------
// JEFF: TEMPORARY FUNCTIONS, COMMENT OR YOUR BALLZ GO WITH IT
//-----------------------------------------------------------------------------

// Jeff: test function to connect to me
function joinServer(%ip) {
   // Jeff: my ip, do not give out please

   // Matt: Requires check to see if ip is valid
   if ((%status = IPCheck(%ip)) !$= "Good")
   {
      MessageBoxOK("Invalid Address!", %status);
      return;
   }

   $HostingMP = false;

   %ip = %ip @ ":" @ $MP::Core::Port;
   $server::ServerType = "MultiPlayer";
   portInit($MP::Core::Port);
   if (isObject(ServerConnection))
      disconnect();
   if ($MP::Core::EnableNameDialog)
      EnterPlayerNameDlg.show("continueJoin(\"" @ %ip @ "\");");
   else
      continueJoin(%ip);
}

function continueJoin(%ip) {
   if ((%status = IPCheck(%ip)) !$= "Good")
   {
      MessageBoxOK("Invalid Address!", %status);
      return;
   }

   new GameConnection(ServerConnection);
   RootGroup.add(ServerConnection);
   ServerConnection.setConnectArgs($pref::Multiplayer::Username, $MP_VERSION, "bologna");
   ServerConnection.connect(%ip);
}

// Jeff: test function for me to host //Modified by Matt
function host() {
   %id = PM_missionList.getSelectedId();
   %mission = getField(PM_missionList.getRowTextById(%id), 1).file;

   if ($MP::Core::DisableMPs && checkMPs(%mission)) {
      //Ohes noes!
      MessageBoxOk("Moving Platforms", "ERROR: You cannot play a mission with moving platforms, as they will crash MultiPlayer or act glitchy!");
      return;
   }

   $pref::Server::Port = $MP::Core::Port;
   portInit($MP::Core::Port);
   $pref::HostMultiPlayer = true;
   //$pref::Multiplayer::Username = "Jeff";

   if ($MP::Core::EnableNameDialog)
      EnterPlayerNameDlg.show("continueHost();", true);
   else
      continueHost();
}

//Matt
function continueHost()
{
   $HostingMP = true;
   allowConnections(true);

   %id = PM_missionList.getSelectedId();
   %mission = getField(PM_missionList.getRowTextById(%id), 1);
   $LastMissionType = %mission.type;

   %serverType = "MultiPlayer";

   // We need to start a server if one isn't already running
   if ($Server::ServerType $= "") {
      if($doRecordDemo)
         recordDemo("~/client/demos/demo.rec", %mission.file);
      createServer(%serverType, %mission.file);
      %conn = new GameConnection(ServerConnection);
      RootGroup.add(ServerConnection);
      %conn.setConnectArgs($pref::Multiplayer::Username, $MP_VERSION, "bologna");
      %conn.setJoinPassword($Client::Password);
      %conn.connectLocal();
   }
   else
      loadMission(%mission.file);
   if(isObject(MissionInfo))
      MissionInfo.level = %mission.level;
}

// Matt: does this function belong here?
// Matt: wait's for all players to finish the level
function waitForPlayers()
{
   cancel($waitPlayers);

   if ($waitingPlayers == ClientGroup.getCount()) {
      $waitingPlayers = 0;
      setGameState("End");
   } else {
      $waitPlayers = schedule($MP::Core::WaitDelta, 0, waitForPlayers);
   }
}

function t(%message) {
   if (!$MP::Chat::EnableChat)
      return;
   %message = $pref::Multiplayer::Username @ ":" SPC %message;
   messageAll('',%message);
   commandToServer('Talk',%message);
}

function serverCmdTalk(%client,%message) {
   if (!$MP::Chat::EnableChat)
      return;
   commandToAll('GetTalk',%message);
}

function clientCmdGetTalk(%message) {
   if (!$MP::Chat::EnableChat)
      return;
   //echo("Talk:" SPC %message);
}

function incrementVersion() {
   %obj = new FileObject();
   %file = "";
   if (%obj.openForRead(expandFilename("./version.cs"))) {
      while (!%obj.isEOF()) {
         %line = trim(%obj.readLine());
         if (getSubStr(%line, 0, 11) $= "$MP_Version") {
            %version = getSubStr(%line, 14, strlen(%line));
            %version = getSubStr(%version, 0, strlen(%version) - 1);
            %version += 0.01;
            %line = "$MP_Version = " @ %version @ ";";
         }
         %file = %file @ %line @ "\n";
      }
   }
   %obj.close();
   if (%file !$= "" && %obj.openForWrite(expandFilename("./version.cs"))) {
      %obj.writeLine(trim(%file));
   }
   %obj.close();
   %obj.delete();
}

function toggleMultiplayer(%bool)
{
   if (%bool !$= "") {
      $pref::HostMultiPlayer = %bool;
   } else {
      $pref::HostMultiPlayer = !$pref::HostMultiPlayer;
   }
   if (!$pref::HostMultiPlayer)
   {
      PM_Play.command = "$HostingMP = false;PM_StartMission();";
      PM_Play.setBitmap("marble/client/ui/play/play");
      PM_Join.setVisible(false);
      PM_Join.setActive(false);
   } else {
      PM_Play.command = "host();";
      PM_Play.setBitmap("marble/multiplayer/images/host");
      PM_Join.setVisible(true);
      PM_Join.setActive(true);
   }
}

// Jeff: re-exec all mp files
function loadMP(){
   for (%f = findFirstFile("marble/multiplayer/*.cs"); %f !$= ""; %f = findNextFile("marble/multiplayer/*.cs"))
      exec(%f);
}

function getPing() {
   if (!isObject(ServerConnection))
      return -1;
   return ServerConnection.getPing();
}

function trackMP(%id) {
   cancel($track);
   //echo(%id.getTransform());
   $track = schedule(10,0,trackMP,%id);
}

function checkMPs(%file) {
   %obj = new FileObject();
   if (!%obj.openForRead(%file)) {
      %obj.close();
      %obj.delete();
      return false;
   }

   while (!%obj.isEOF()) {
      %line = trim(%obj.readLine());
      if (strStr(%line, "PathedDefault") != -1) {
         %obj.close();
         %obj.delete();
         return true;
      }
   }

   %obj.close();
   %obj.delete();
   return false;
}