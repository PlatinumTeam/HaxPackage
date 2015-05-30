//-----------------------------------------------------------------------------
// master.cs
// Copyright (C) 2012 Jeffrey Hutchinson
// Copyright (C) 2012 HiGuy Smith
// Portions copyright (c) 2012 TGE
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

//-----------------------------------------------------------------------------
// Master Server - sending
//-----------------------------------------------------------------------------

//HiGuy: Update (2015): Server is 100% completely dead.
$Master::Server = "http://www.example.com:80";
$Master::Path = "/";

function masterStartGame() {
   if ($Master::Started) {
      $Master::Restart = true;
      masterEndGame();
      return;
   }
   $Master::Restart = false;
   if (isObject(MasterSender))
      MasterSender.destroy();
   new TCPObject(MasterSender);
   MasterSender.isEnding = false;
   %query = "Name=" @ $pref::Server::Multiplayer::serverName @
           "&Level=" @ MissionInfo.name @
           "&Mode=" @ $MP::Master::Mode @
           "&Players=" @ $Server::PlayerCount @
           "&Password=" @ ($serverPassword !$= "") @
           "&Version=" @ $MP::Master::Version @
           "&Dedicated=" @ $Server::Dedicated @
           "&Mod=" @ $Master::Mod @
           "&Port=" @ $pref::Server::Port;
   %values = URLEncode(%query);
   $Master::Started = true;
   MasterSender.post($Master::Server, $Master::Path @ "startgame.php", %values);
}

function masterEndGame() {
   if (isObject(MasterSender))
      MasterSender.destroy();
   if (!$Master::Started)
      return;
   new TCPObject(MasterSender);
   MasterSender.isEnding = true;
   %query = "Key=" @ $Server::Key;
   %values = URLEncode(%query);
   MasterSender.post($Master::Server, $Master::Path @ "endgame.php", %values);
}

function MasterSender::onLine(%this, %line) {
   //%this.echo(%line);
   Parent::onLine(%this, %line);
   if (strPos(%line, "PARAMS REQUIRED:") == 0) {
      //oops
      %params = getSubStr(%line, strPos(%line, ":") + 1, strLen(%line));
      %this.echo("Master Sender Failed. Required Params:" SPC %params);
      %this.failed = true;
   }
   if (strPos(%line, "GOOD") == 0) {
      //yay
      if (%this.isEnding) {
         //%this.echo("Master Sender Finished, Game Stopped.");
         cancel($heartbeat);
         if (isObject(MasterBeater)) { //No, don't even go there, andrew and "nobody"
            MasterBeater.disconnect();
            $heartbeat = schedule(1000, 0, masterHeartbeat);
         }
      } else {
         %key = getSubStr(%line, 5, strLen(%line));
         //%this.echo("Master Sender Finished! Server Key:" SPC %key);
         $Server::Key = %key;
         %this.failed = false;
      }
   }
}

function MasterSender::onFinish(%this) {
   if (%this.isEnding) {
      $Master::Started = false;
   } else {
      if (%this.failed) //Now, why would we start
         return;
      if ($MP::Master::UpdateTime < 10000) //We don't trust them that much
         $MP::Master::UpdateTime = 10000;
      schedule($MP::Master::UpdateTime, 0, masterHeartbeat);
   }
   if ($Master::Restart && %this.isEnding) {
      schedule(0, 0, masterStartGame);
   }
}

function masterHeartbeat() {
   if (!$Master::Started || $Server::Key $= "")
      return;
   cancel($heartbeat);
   if (isObject(MasterBeater)) { //No, don't even go there, andrew and "nobody"
      MasterBeater.destroy();
      $heartbeat = schedule(1000, 0, masterHeartbeat);
   }
   new TCPObject(MasterBeater);
   %query = "Name=" @ $pref::Server::Multiplayer::serverName @
           "&Level=" @ MissionInfo.name @
           "&Mode=" @ $MP::Master::Mode @
           "&Players=" @ $Server::PlayerCount @
           "&Password=" @ ($serverPassword !$= "") @
           "&Key=" @ $Server::Key @
           "&Version=" @ $MP::Master::Version @
           "&Dedicated=" @ $Server::Dedicated @
           "&Port=" @ $pref::Server::Port;
  %values = URLEncode(%query);
   MasterBeater.post($Master::Server, $Master::Path @ "heartbeat.php", %values);
}

function MasterBeater::onLine(%this, %line) {
   Parent::onLine(%this, %line);
   //%this.echo(%line);
   if (strPos(%line, "PARAMS REQUIRED:") == 0) {
      //oops
      %params = getSubStr(%line, strPos(%line, ":") + 1, strLen(%line));
      %this.echo("Master Beater Failed. Required Params:" SPC %params);
   }
   if (strPos(%line, "RESTART") == 0) {
      //balls
      cancel($heartbeat);
      %this.dont = true;
   }
   if (strPos(%line, "GOOD") == 0) {
      //yay
      %goodStatus = getSubStr(%line, 5, strLen(%line));
      //%this.echo("Master Beater Finished With Status:" SPC %goodStatus);
   }
}

function MasterBeater::onFinish(%this) {
   if (%this.dont)
      masterStartGame();
   else {
      cancel($heartbeat);
      if ($MP::Master::UpdateTime < 10000) //We don't trust them that much
         $MP::Master::UpdateTime = 10000;
      $heartbeat = schedule($MP::Master::UpdateTime, 0, masterHeartbeat);
   }
}

function MasterBeater::onDisconnected(%this) {
   %this.schedule(1000, "delete");
}