//-----------------------------------------------------------------------------
// main.cs
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

$Game::MPNMultiple = 1.0043;

function getMod() {
   if (isFile("marble/server/scripts/speedmeter.cs.dso"))
      return  "Advanced";
   if (isFile("marble/server/scripts/tricks.cs"))
      return  "Project Dragon";
   if (isFile("marble/server/scripts/tricks.cs.dso"))
      return  "Project Dragon";
   if (isFile("marble/server/scripts/crates.cs"))
      return  "Elite Demo";
   if (isFile("marble/server/scripts/crates.cs.dso"))
      return  "Elite Demo";
   if (isFile("elite/server/scripts/crates.cs"))
      return  "Elite";
   if (isFile("elite/server/scripts/crates.cs.dso"))
      return  "Elite";
   if (isFile("marble/client/ui/Awards/AllSapphires.png"))
      return  "Emerald";
   if (isFile("emerald/client/ui/Awards/AllSapphires.png"))
      return  "Emerald 1.01";
   if (isFile("marble/data/missions/advanced/Bladestorm.mis"))
      return  "Fubar";
   if (isFile("marble/quickFix.cs"))
      return  "Future";
   if (isFile("marble/quickFix.cs.dso"))
      return  "Future";
   if (isFile("marble/data/missions/beginner/Gravity-Modifying.mis"))
      return  "Gold (George\'s Mini-Mod)";
   if (isFile("marble/client/ui/fullVersion.png"))
      return  "Gold Demo";
   if (isFile("marble/client/ui/STBox.gui.dso"))
      return  "Opal";
   if (isFile("marble/client/ui/play_rc_d.png"))
      return  "Platinum";
   if (isFile("platinum/client/ui/play_rc_d.png"))
      return  "Platinum";
   if (isFile("pqport/server/scripts/cannon.cs.dso"))
      return  "PlatinumQuest";
   if (isFile("pqport/server/scripts/cannon.cs")) //For PQ Team :3
      return  "PlatinumQuest";
   if (isFile($usermods @ "/server/scripts/cannon.cs.dso"))
      return  "PlatinumQuest";
   if (isFile($usermods @ "/server/scripts/cannon.cs"))
      return  "PlatinumQuest";
   if (isFile("marble/client/ui/BBLoadingGui.gui.dso"))
      return  "Reloaded";
   if (isFile("marble/data/particles/debris.png"))
      return  "Revived";
   if (isFile("marble/client/ui/loading/loadinggui_origin.png"))
      return  "Space";
   if (isFile("marble/multiplayer/master.cs"))
      return  "Gold Multiplayer";
   if (isFile("marble/multiplayer/master.cs.dso"))
      return  "Gold Multiplayer";
   return     "Gold";
}

// Jeff: main function for loading multiplayer into the game
function initMultiplayer() {
   // Jeff: Scripts
   exec("./version.cs");

   exec("./defaults.cs");
   exec("./variables.cs");

   exec("./support.cs");
   exec("./core.cs");

   //HiGuy: Ya might want to keep this executed ;)
   exec("./crc.cs");
   if ($Server::Dedicated) {
      exec("./dcon.cs");
   } else {
      exec("./gui.cs");
      exec("./clientCommands.cs");
   }

   exec("./tcp.cs");
   exec("./fireworks.cs");
   exec("./tween.cs");
   exec("./collision.cs");
   if ($MP::Master::EnableMasterServer)
      exec("./master.cs");
   exec("./serverCommands.cs");
   exec("./gameConnection.cs");
   if ($MP::Chat::EnableChat)
      exec("./chat.cs");

   // Jeff: Guis
   // None for now, give me some pie!!!
   // and no higuy, i do NOT want 3.14...

   // 3.141592653589793238462643383279502884
   //HiGuy: FTFY

   // Jeff: dialogs
   if (!$Server::Dedicated) {
      exec("./PauseDlg.gui");
      if ($MP::Master::EnableMasterServer)
         exec("./masterServerGui.gui");
      exec("./preGameDlg.gui");
      if ($MP::Admin::EnableAdminPanel)
         exec("./adminPanelDlg.gui");
      if ($MP::Core::EnableNameDialog) {
         exec("./EnterPlayerNameDlg.gui");
         exec("./EnterServerNameDlg.gui");
      }
      exec("./FinishGameDlg.gui");
      if ($MP::Chat::EnableChat) {
         exec("./ChatDlg.gui");
         exec("./ReceiveChatDlg.gui");
      }
      if ($MP::Direct::EnableDirectDialog)
         exec("./DirectConnectDlg.gui");
      exec("./JoinServerDlg.gui");
   }

   //HiGuy: Move around MBG stuff, you can totally remove this if you want
   if (getMod() $= "Gold Multiplayer") {
      exec("./PlayMissionGuiGui.gui");
      exec("./LoadingGui.gui");
   }

   if ($pref::Multiplayer::Username $= "")
      $pref::Multiplayer::Username = $MP::Core::DefaultName;

   toggleMultiplayer($pref::HostMultiPlayer);
}

//HiGuy: This should be called in onServerCreated()
function initMultiplayerServer() {
   //HiGuy: Variables
   $Game::FirstSpawn = false;
   $Game::MarbleCount = 0;

   //HiGuy: Scripts
   exec("./particles.cs");
   exec("./ghost.cs");

   activatePackage(oob);
   startLoops();
}

// Jeff: ensures that disconnect is fixed
package fixDisconnect {
   function PM_StartMission() {
      disconnect();
      Parent::Pm_StartMission();
   }
   function onExit() {
      //Sorry
      for (%i = 0; %i < TCPGroup.getCount(); %i ++) {
         TCPGroup.getObject(%i).disconnect();
      }
   }
};
activatePackage(fixDisconnect);