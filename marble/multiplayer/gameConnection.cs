//-----------------------------------------------------------------------------
// gameConnection.cs
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

//HiGuy: All of these should be obvious
function GameConnection::stopTimer(%this) {
   commandToClient(%this, 'stopTimer');
}

function GameConnection::setTime(%this, %time) {
   commandToClient(%this, 'setTime', %time);
}

function GameConnection::setGemCount(%this, %count) {
   commandToClient(%this, 'setGemCount', %count);
}

function GameConnection::resetTimer(%this) {
   commandToClient(%this, 'resetTimer');
}

function GameConnection::startTimer(%this) {
   commandToClient(%this, 'startTimer');
}

function GameConnection::setMessage(%this, %message, %timeout) {
   commandToClient(%this, 'setMessage', %message, %timeout);
}

function GameConnection::setMaxGems(%this, %maxGems) {
   commandToClient(%this, 'setMaxGems', %maxGems);
}

function GameConnection::setPowerUp(%this, %powerup) {
   commandToClient(%this, 'setPowerUp', %powerup);
}

function GameConnection::addHelpLine(%this, %line, %beep) {
   commandToClient(%this, 'addHelpLine', %line, %beep);
}

function GameConnection::adjustTimer(%this, %time) {
   commandToClient(%this, 'adjustTimer', %time);
}

function GameConnection::addBonusTime(%this, %time) {
   commandToClient(%this, 'addBonusTime', %time);
}

function GameConnection::nudge(%this, %pos) {
   commandToClient(%this, 'nudge', %pos);
}

function GameConnection::setGravityDir(%this, %dir, %bool) {
   commandToClient(%this, 'setGravityDir', %dir, %bool);
}

function GameConnection::isHost(%this) {
   if (%this.getName() $= "LocalClientConnection")
      return true;
   if (%this.getIP() $= "127.0.0.1")
      return true;
   return false;
}

function setGameState(%state) {
   for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
      ClientGroup.getObject(%i).setGameState(%state);
   }
}

function GameConnection::setGameState(%this, %state) {
   //HiGuy: No hacking!
   %state = stripNot(strlwr(%state), "abcdefghijklmnopqrstuvwxyz:1234567890");

   %this.gameState = %state;
   //HiGuy: Set state
   cancel(%this.stateSchedule);
   eval(%this @ ".state" @ %state @ "();");
}

function GameConnection::stateStart(%this)
{
   %this.resetTimer();
   %this.setMessage("");
   %this.setGemCount(0);
   %this.setMaxGems($Game::GemCount);
   %this.stateSchedule = %this.schedule( 500, "setGameState", "Ready");
   if(MissionInfo.startHelpText !$= "")
   {
      %this.addHelpLine(MissionInfo.startHelpText, false);
   }
}

function GameConnection::stateReady(%this)
{
   %this.play2d(ReadyVoiceSfx);
   %this.setMessage("ready");
   %this.stateSchedule = %this.schedule( 1500, "setGameState", "set");
}

function GameConnection::stateSet(%this)
{
   %this.play2d(SetVoiceSfx);
   %this.setMessage("set");
   %this.stateSchedule = %this.schedule( 1500, "setGameState", "Go");
}

function GameConnection::stateGo(%this)
{
   %this.play2d(GetRollingVoiceSfx);
   %this.setMessage("go");
   %this.startTimer();
   %this.stateSchedule = %this.schedule( 2000, "setGameState", "Play");

   // Target the players to the end pad and let them lose
   %player = %this.player;
   %player.setPad($Game::EndPad);
   %player.setMode(Normal);
}

function GameConnection::statePlay(%this)
{
   // Normaly play mode
   %this.setMessage("");
}

function GameConnection::stateEnd(%this)
{
   // Do score calculations, messages to winner, losers, etc.
   %this.stopTimer();
   %this.play2d(WonRaceSfx);
}

function GameConnection::stateWaiting(%this) {
   //Reset, don't start though
   %this.resetTimer();
   %this.setMessage("");
   %this.setGemCount(0);
   %this.setMaxGems($Game::GemCount);
   %this.player.setMode(Start);
   if(MissionInfo.startHelpText !$= "")
   {
      %this.addHelpLine(MissionInfo.startHelpText, false);
   }
}

function GameConnection::resetGems(%this) {
   for (%i = 0; %i < MissionGroup.getCount(); %i ++) {
      %obj = MissionGroup.getObject(%i);
      if (%obj.getClassName() $= "Item" && %obj.getDataBlock().classname $= "Gem") {
         if (%obj.pickup == %this.getId()) {
            if (%this.endPad)
               $Game::EndPadGems --;
            if ($Game::EndPadGems < 0)
               $Game::EndPadGems = 0;
            %obj.onMissionReset();
            %obj.pickup = "";
         }
      }
   }
}

function GameConnection::getCheckpointPos(%this,%num)
{
   // Return the point a little above the object's center
   if (!isObject(%this.checkPoint.pad))
      return "0 0 300 1 0 0 0";
   if (!$Game::FirstSpawn && $Server::ServerType $= "Multiplayer")
      return vectorAdd(%this.checkPoint.pad.getTransform(),"0 0 6") SPC
                    getWords(%this.checkPoint.pad.getTransform(), 3);
   return vectorAdd(%this.checkPoint.pad.getTransform(),"0 0 3") SPC
                    getWords(%this.checkPoint.pad.getTransform(), 3);
}

function respawnAll() {
   for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
      ClientGroup.getObject(%i).respawnPlayer();
   }
}

package oob {
   function Marble::setOOB(%this, %oob) {
      Parent::setOOB(%this, %oob);
      %this.oob = !!%oob;
      oobLoop();
   }
   function Marble::getOOB(%this) {
      return %this.oob $= "" ? false : !!%this.oob;
   }
};

function GameConnection::getIP(%this) {
   %address = %this.getAddress();
   if (%address $= "local")
      return "127.0.0.1"; //Localhost
   //IP:127.0.0.1:28001
   %address = getSubStr(%address, strPos(%address, ":") + 1, strLen(%address));
   //127.0.0.1:28001
   %address = getSubStr(%address, 0, strPos(%address, ":"));
   //127.0.0.1
   return %address;
}

function GameConnection::cancelRespawn(%this) {
   cancel(%this.respawnSchedule);
}

function GameConnection::getNumber(%this) {
   for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
      %client = ClientGroup.getObject(%i);
      if (%client.getId() == %this.getId())
         return %i;
   }
   return -1;
}
//-----------------------------------------------------------------------------
// Get client by name

function getClientByName(%name) {
   for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
      %cli = ClientGroup.getObject(%i);
      if (%cli.nameBase $= %name)
         return %cli;
   }
   return -1;
}

//-----------------------------------------------------------------------------
// Get client by ip

function getClientByIP(%ip) {
   if (%ip $= "local" || %ip $= "127.0.0.1")
      return LocalClientConnection; //Host
   for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
      %cli = ClientGroup.getObject(%i);
      if (%cli.getIP() $= %ip)
         return %cli;
   }
   return -1;
}

//-----------------------------------------------------------------------------
// Does you = me ?

function clientIsClient(%cl1, %cl2) {
   return %cl1.getId() == %cl2.getId();
}

function GameConnection::equals(%this, %cl2) {
   return %this.getId() == %cl2.getId();
}

//------------------------------------------------------------------------------

function gameOver() {
   if (!$Game::Running) {
      error("gameOver: No game running!");
      return;
   }

   startFireWorks(EndPoint);
   setGameState("end");
   $Game::StateSchedule = schedule(2000, 0, "endGame");
}
