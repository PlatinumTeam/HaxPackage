//-----------------------------------------------------------------------------
// serverCommands.cs
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

// Jeff: chat sent to the server goes here
function serverCmdSendChat(%client,%message) {
   if (!$MP::Chat::EnableChat)
      return;
   commandToAll('ReceiveChatFromServer', %client.namebase @ ":" SPC %message);
}

// Jeff: chat sent from preGameDlg
function serverCmdSendChatFromPreGame(%client,%message) {
   if (!$MP::Chat::EnableChat)
      return;
   commandToAll('ReceivePreGameChatFromServer', %client.namebase @ ":" SPC %message);
}

// Jeff: update userlist
function serverCmdUpdatePreGameUserList(%cl,%isHost) {
   %list = "";
   $Server::ReadyCount = 0;
   for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
      %client = ClientGroup.getObject(%i);
      %username = %client.username;
      // Jeff: assume host is index 0, should be 99.99% accurate
      if ((%i == 0 && %isHost) || (%client.isHost)) {
         %client.isHost = true;
         %username = %username TAB true;
      } else
         %username = %username TAB false;
      // Jeff: have we pressed the ready button?
      if ($Server::Ready[%client]) {
         %username = %username TAB true;
         $Server::ReadyCount ++;
      } else
         %username = %username TAB false;
      %list = (%list $= "") ? %username : %list TAB %username;
   }
   // Jeff: send cmd to host for play button
   %ready = ($Server::ReadyCount == $Server::PlayerCount) ? true : false;
   // Jeff: if we are dedicated and all are ready, start the match
   // else give host control of when to start, but everybody must be
   // ready regardless of the host
   if (($Server::Dedicated && !$pref::Server::StandingHosts) && %ready) {
      // Jeff: should be fixed to make it more stable in the future
      if ($MP::Chat::EnableChat)
         commandToAll('ReceivePreGameChatFromServer',"[SERVER]: GAME STARTING...");
      $Game::StartSchedule1 = schedule(2000,0,"setGameState","Start");
      $Game::StartSchedule2 = schedule(2000,0,"commandToAll",'PopPregame');
   } else {
      commandToHost('PreGameHostPlayToggle',%ready);
      cancel($Game::StartSchedule1);
      cancel($Game::StartSchedule2);
   }
   commandToAll('UpdatePreGameUserList',%list);
   serverCmdGetServerInfo(%client);
}

function serverCmdGetUserList(%client)
{
   %list = "";
   for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
      %client = ClientGroup.getObject(%i);
      %username = %client.username;
      // Jeff: assume host is index 0, should be 99.99% accurate
      if (%client.isHost)
         %username = %username TAB true;
      else
         %username = %username TAB false;
      %list = (%list $= "") ? %username : %list TAB %username;
   }
   commandToAll('UserList',%list);
}

// Jeff: signal ready status
function serverCmdSignalReadyStatus(%client,%status,%isHost) {
   $Server::Ready[%client] = %status;
   // Jeff: update the list so we can't visually see if a client is ready
   serverCmdUpdatePreGameUserList(%client,%isHost);
}

// Jeff: sets username
function serverCmdSetUsername(%client,%username) {
   %client.username = trim(%username);
}

// Jeff: get server information method
function serverCmdGetServerInfo(%client) {
   %dedicated = $Server::Dedicated;
   %playerCount = $Server::PlayerCount;
   %maxPlayers = $Pref::Server::MaxPlayers;
   %readyCount = $Server::ReadyCount;
   %info = $Pref::Server::Info;
   %running = $Game::FirstSpawn;
   //HiGuy: This decides (on a dedicated server) whether or not a client can access the admin panel, etc
   %clientNum = %client.getNumber();
   if (!$pref::Server::StandingHosts)
      %clientNum = 1;
   commandToClient(%client, 'UpdatePreGameServerInfo', %dedicated, %info, %playerCount, %readyCount, %maxPlayers, %running, %clientNum);
}

// Jeff: preGame PLAY BUTTON
function serverCmdPreGamePlay(%client) {
   $Game::FirstSpawn = true;
   for (%i = 0; %i < 0; %i ++) {
      %client = ClientGroup.getObject(%i);
      $Server::Ready[%client] = false;
   }
   $Server::ReadyCount = 0;
   serverCmdGetServerInfo();
   commandToAll('popPregame');
   respawnAll();
   setGameState("start");
   startLoops();
}

function serverCmdRestart(%client) {
   %client.respawnPlayer();
}

function serverCmdMousefire(%client, %mouseFire) {
   %client.mouseFire = %mouseFire;
}

function serverCmdStartMission(%client, %mission) {
   %clientNum = %client.getNumber();

   //Heeeeeeell no!
   if (%clientNum != 0 || !$pref::Server::StandingHosts || !$Server::Dedicated)
      return;

   if ($MP::Core::DisableMPs && checkMPs(%mission)) {
      //No!
      commandToClient(%client, 'MissionMPs');
      return;
   }

   //Otherwise, comply
   loadMission(%mission);
}

function serverCmdKickUser(%client, %user) {
   %clientNum = %client.getNumber();

   if (%clientNum != 0 || !$pref::Server::StandingHosts || !$Server::Dedicated)
      return;

   kick(%user);
}

function serverCmdBanUser(%client, %user) {
   %clientNum = %client.getNumber();

   if (%clientNum != 0 || !$pref::Server::StandingHosts || !$Server::Dedicated)
      return;

   ban(%user);
}

function serverCmdBanIPUser(%client, %user) {
   %clientNum = %client.getNumber();

   if (%clientNum != 0 || !$pref::Server::StandingHosts || !$Server::Dedicated)
      return;

   banIPName(%user);
}

// Jeff: show help for dedicated servers
function help() {
   if (!$Server::Dedicated)
      return;
   echo("-------------------------------------");
   echo("             Server help             ");
   echo("-------------------------------------");
   echo("");
   echo("ban(\"name\");  - Bans a player by username | Enter name to ban.");
   echo("banIP(\"ip\");  - Bans a player by IP       | Enter ip to ban.");
   echo("kick(\"name\"); - Kicks a player            | Enter name to be kicked.");
   echo("respawnAll(); - Respawns all the players");
   echo("quit();       - Quits the server.");
}