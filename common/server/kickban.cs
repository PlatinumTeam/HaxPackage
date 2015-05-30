//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
// Portions Copyright (c) 2001 by Sierra Online, Inc.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------

function kick(%name, %ban) {
   if (!$MP::Ban::EnableBan)
      return;
   %client = getClientByName(%name);
   if (%client.isHost())
      return;
   if (%ban) //The listing has already been done... muahaha
      %client.delete("You have been banned from this server");
   else {
      addToBan.client($MP::Ban::KickTime * 1000);
      addToBanIP.client($MP::Ban::KickTime * 1000);
      %client.delete("You have been kicked from this server");
   }
}

function addToBan(%name, %time) {
   if (!$MP::Ban::EnableBan)
      return;
   for (%i = 0; $pref::Server::BanList[%i] !$= ""; %i ++) {
      if ($pref::Server::BanList[%i] $= %name) {
         //Increment their time
         $pref::Server::BanList[%i] = getField($pref::Server::BanList[%i], 0) TAB (%time == 0 ? 2147483647 : getField($pref::Server::BanList[%i], 1) + %time);
         return;
      }
   }
   $pref::Server::BanList[%i] = %name TAB (%time == 0 ? 2147483647 : getRealTime() + %time);
}

function addToBanIP(%ip, %time) {
   if (!$MP::Ban::EnableBan)
      return;
   if (%ip $= "127.0.0.1")
      return; //NO BANNING LOCALHOST!
   for (%i = 0; $pref::Server::BanListIP[%i] !$= ""; %i ++) {
      if ($pref::Server::BanListIP[%i] $= %ip) {
         //Increment their time
         $pref::Server::BanList[%i] = getField($pref::Server::BanListIP[%i], 0) TAB getField($pref::Server::BanListIP[%i], 1) + %time;
         return;
      }
   }
   $pref::Server::BanListIP[%i] = %ip TAB getRealTime() + %time;
}

function ban(%name) {
   if (!$MP::Ban::EnableBan)
      return;
   %client = getClientByName(%name);
   if (%client.isHost())
      return;
   %i = 0;
   addToBan(%name, $MP::Ban::BanTime * 1000);
   if (%client != -1)
      kick(%client.namebase);
}

function unban(%name) {
   %goDown = false;
   for (%i = 0; (%banOn = $pref::Server::BanList[%i]) !$= ""; %i ++) {
      if (%banOn $= %name) {
         $pref::Server::BanList[%i] = "";
         %goDown = true;
      }
      if (%goDown)
         $pref::Server::BanList[%i] = $pref::Server::BanList[%i + 1];
   }
   return %goDown;
}

function banip(%ip) {
   if (!$MP::Ban::EnableBan)
      return;
   %client = getClientByIP(%name);
   if (%client.isHost())
      return;
   %i = 0;
   addToBanIP(%ip, $MP::Ban::BanTime * 1000);
   if (%client != -1)
      kick(%client.namebase);
}

function banipname(%name) {
   if (!$MP::Ban::EnableBan)
      return;
   %client = getClientByName(%name);
   if (%client.isHost())
      return;
   banip(%client.getIP());
}

function unbanip(%ip) {
   %goDown = false;
   for (%i = 0; (%banOn = $pref::Server::BanListIP[%i]) !$= ""; %i ++) {
      if (%banOn $= %ip) {
         $pref::Server::BanListIP[%i] = "";
         %goDown = true;
      }
      if (%goDown)
         $pref::Server::BanListIP[%i] = $pref::Server::BanListIP[%i + 1];
   }
   return %goDown;
}

function checkBan(%ip, %name) {
   if (!$MP::Ban::EnableBan)
      return false;
   if (%ip $= "local") //No banning localhost
      return false;
   //Check name bans
   for (%i = 0; (%banOn = $pref::Server::BanList[%i]) !$= ""; %i ++) {
      if (%banOn $= %name) {
         %time = getField(%banOn, 1);
         if (getRealTime() > %time)
            unban(%name);
         else
            return true;
      }
   }

   //Check IP bans
   for (%i = 0; (%banOn = $pref::Server::BanListIP[%i]) !$= ""; %i ++) {
      if (%banOn $= %ip) {
         %time = getField(%banOn, 1);
         if (getRealTime() > %time)
            unbanip(%ip);
         else
            return true;
      }
   }
   return false;
}