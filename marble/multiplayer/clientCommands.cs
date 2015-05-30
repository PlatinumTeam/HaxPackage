//-----------------------------------------------------------------------------
// clientCommands.cs
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

// Jeff: receive chat from server
function clientCmdReceiveChatFromServer(%message) {
   if (!$MP::Chat::EnableChat)
      return;
   receiveChat(%message);
   PreGameDlg.updateChat(%message);
}

// Jeff: pregame userlist updater, also updates the ready status
function clientCmdUpdatePreGameUserList(%list) {
   echo("List:" SPC %list);
   PreGameDlg.updateUserList(%list);
}

// Jeff: HOST ONLY, preGame play buttona activation, ensure
// that all clients are ready!
function clientCmdPreGameHostPlayToggle(%enable) {
   PreGamePlay.setActive(%enable);
}

// Jeff: clientcmd to show server info
function clientCmdUpdatePreGameServerInfo(%dedicated, %info, %playerCount, %readyCount, %maxPlayers, %running, %number) {
   if (%running)
      clientCmdPopPregame();
   else if ($PushPregame)
      Canvas.pushDialog(PreGameDlg);
   $Server::Dedicated = %dedicated;
   $Server::StandingHost = %number == 0 && %dedicated;
   PreGameDlg.changeServerInfo(%dedicated, %info, %playerCount, %readyCount, %maxPlayers);
   PreGameDlg.updateButtons();
}

// Jeff: push dialogs from command
function clientCmdPushDialog(%dialog) {
   if (isObject(%dialog)) {
      Canvas.pushDialog(%dialog);
   }
}

// Jeff: pop dialogs from command
function clientCmdPopDialog(%dialog) {
   if (isObject(%dialog)) {
      Canvas.popDialog(%dialog);
   }
}

// HiGuy: When pregame starts
function clientCmdPopPregame() {
   Canvas.popDialog(PregameDlg);
   hideGhost();
   //Hide it, damnit
   schedule(1000, 0, hideGhost);
   schedule(2000, 0, hideGhost);
   schedule(4000, 0, hideGhost);
   schedule(6000, 0, hideGhost);
   startLoops();
}

//HiGuy: Redirect to PlayGui
function clientCmdSetTime(%time) {
   PlayGui.setTime(%time);
}

//HiGuy: Redirect to PlayGui
function clientCmdSetGemCount(%gemCount) {
   PlayGui.setGemCount(%gemCount);
}

//HiGuy: Migrated from endGame() so each client get separate scores
function clientCmdEndGameTimes(%penaltyTime) {
   $Game::ScoreTime = PlayGui.elapsedTime;
   $Game::ElapsedTime = PlayGui.elapsedTime - %penaltyTime + PlayGui.totalBonus;
   $Game::PenaltyTime = %penaltyTime;
   $Game::BonusTime = PlayGui.totalBonus;

   // Not all missions have time qualifiers
   $Game::Qualified = MissionInfo.time? $Game::ScoreTime < MissionInfo.time: true;

   // Bump up the max level
   if (!$DemoBuild && !$playingDemo && $Game::Qualified && (MissionInfo.level + 1) > $Pref::QualifiedLevel[MissionInfo.type])
         $Pref::QualifiedLevel[MissionInfo.type] = MissionInfo.level + 1;
}

//HiGuy: Redirect to PlayGui
function clientCmdStartTimer() {
   PlayGui.startTimer();
}

//HiGuy: Redirect to PlayGui
function clientCmdStopTimer() {
   PlayGui.stopTimer();
}

//HiGuy: Redirect to PlayGui
function clientCmdResetTimer() {
   PlayGui.resetTimer();
}

//HiGuy: Redirect to PlayGui
function clientCmdSetMessage(%message, %timeout) {
   PlayGui.setMessage(%message, %timeout);
}

//HiGuy: Redirect to PlayGui
function clientCmdSetMaxGems(%maxGems) {
   PlayGui.setMaxGems(%maxGems);
}

//HiGuy: Redirect to PlayGui
function clientCmdSetPowerUp(%powerup) {
   //If the file doesn't exist, we'll crash the game
   //Gracefully handle the issue by simply clearing the display instead
   if (isFile(%powerup))
      PlayGui.setPowerUp(%powerup);
   else
      PlayGui.setPowerUp("");
}

//HiGuy: Redirect to PlayGui
function clientCmdAddHelpLine(%message,%beep) {
   addHelpLine(%message,%beep);
}

//HiGuy: Redirect to PlayGui
function clientCmdAddBonusTime(%time) {
   PlayGui.addBonusTime(%time);
}

//HiGuy: Guess :P
function clientCmdAdjustTimer(%time) {
   PlayGui.adjustTimer(%time);
}

function clientCmdStartRotLoop() {
   rotLoop();
}

function clientCmdFixGhosts() {
   hideGhost();
}

function clientCmdNewMarble(%namebase, %size) {
   $Game::GhostSize[%size] = %namebase;
   buildGhostList();
}

function clientCmdBuildGhostList() {
   buildGhostList();
}

function clientCmdYourScale(%size) {
   $Game::MyScale = %size;
   buildGhostList();
}

function clientCmdNudge(%pos) {
   if (!isObject($Game::MyMarble))
      return;

   $Game::MyMarble.setTransform(VectorAdd($Game::MyMarble.getTransform(), %pos));
}

// Jeff: apply impulse to a client without messing up
//       the camera and velocity
function clientCmdApplyImpulse(%position,%impulse) {
   // Jeff: if we can't find the object, then do nothing
   if (!isObject($Game::MyMarble)) {
      if (getMyMarble() == -1)
         return;
   }
   $Game::MyMarble.applyImpulse(%position,%impulse);
}

// Matt: Receive Playerlist
function clientCmdUserList(%list) {
   AdminPanelUserList.clear();
   for (%i = 0; %i < getFieldCount(%list); %i+=2) {
      AdminPanelUserList.addRow(AdminPanelUserList.rowCount(),getField(%list,%i) TAB (getField(%list,%i+1) ? "[HOST]" : ""));
   }
}

function clientCmdSetGravityDir(%dir, %bool) {
   setGravityDir(%dir, %bool);
}

function clientCmdFollowParticle(%nameBase, %partSize) {
   %player = $Game::GhostMarble[%nameBase];
   if (%player $= "") //oh well
      return;
   $ParticleArrayCount ++;
   for (%i = 0; %i < ServerConnection.getCount(); %i ++) {
      %obj = ServerConnection.getObject(%i);
      if (%obj.getClassName() $= "ParticleEmitterNode") {
         if (%obj.getDataBlock().timeMultiple == $Game::MPNMultiple) {
            // :o
            if (getWord(%obj.getScale(), 0) == %partSize) {
               // :O
               %id = $ParticleArrayCount;
               cLockCPart(%obj, %player, %id);
               return;
            }
         }
      }
   }
}

function clientCmdRemoveParticle(%partSize) {
   for (%i = 0; %i < ServerConnection.getCount(); %i ++) {
      %obj = ServerConnection.getObject(%i);
      if (%obj.getClassName() $= "ParticleEmitterNode") {
         if (%obj.getDataBlock().timeMultiple == $Game::MPNMultiple) {
            // :o
            if (getWord(%obj.getScale(), 0) == %partSize) {
               // >:O
               %obj.setTransform("0 0 -10000000 0 0 0 0");
            }
         }
      }
   }
}

function cLockCPart(%particle, %marble, %id) {
   cancel($CLockSch[%id]);
   if (!isObject(%particle) || !isObject(%marble))
      return;
   %particle.setTransform(%marble.getWorldBoxCenter() SPC "1 0 0 0");
   $CLockSch[%id] = schedule(10, 0, "cLockCpart", %particle, %marble, %id);
}

function clientCmdMissionMPs() {
   MessageBoxOk("Moving Platforms", "ERROR: You cannot play a mission with moving platforms, as they will crash MultiPlayer or act glitchy!");
}

function clientCmdStartFireworks(%padPos, %classname) {
   if (!isObject(ServerConnection))
      return;

   for (%i = 0; %i < ServerConnection.getCount(); %i ++) {
      %obj = ServerConnection.getObject(%i);
      if (%obj.getClassName() $= %classname) {
         if (%obj.getPosition() $= %padPos) {
            if (!isObject(MissionCleanup))
               ServerConnection.add(new SimGroup(MissionCleanup) {});
            findFireworks();
            startFireworks2(%obj);
         }
      }
   }
}