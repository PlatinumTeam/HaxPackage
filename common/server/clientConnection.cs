//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
// Portions Copyright (c) 2001 by Sierra Online, Inc.
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// This script function is called before a client connection
// is accepted.  Returning "" will accept the connection,
// anything else will be sent back as an error to the client.
// All the connect args are passed also to onConnectRequest
//
function GameConnection::onConnectRequest( %client, %netAddress, %name, %version, %a )
{
   echo("Connect request from: " @ %netAddress);
   if($Server::PlayerCount >= $pref::Server::MaxPlayers)
      return "CR_SERVERFULL";
   if (checkBan(%netAddress, %name))
      return "CR_YOUAREBANNED";
   if ($MP_VERSION && %version != $MP_VERSION && !%client.isHost()) {
      echo("Version mismatch! Client" SPC %name SPC "tried to join with version" SPC %version @ ", current version is" SPC $MP_VERSION);
      return "CHR_PROTOCOL";
   }
   return "";
}

//-----------------------------------------------------------------------------
// This script function is the first called on a client accept
//
function GameConnection::onConnect( %client, %name, %version )
{
   // Send down the connection error info, the client is
   // responsible for displaying this message if a connection
   // error occures.
   messageClient(%client,'MsgConnectionError',"",$Pref::Server::ConnectionError);

   // Send mission information to the client
   sendLoadInfoToClient( %client );

   // Simulated client lag for testing...
   // %client.setSimulatedNetParams(0.1, 30);

   // if hosting this server, set this client to superAdmin
   if (%client.getAddress() $= "local") {
      %client.isAdmin = true;
      %client.isSuperAdmin = true;
   }

   // Get the client's unique id:
   // %authInfo = %client.getAuthInfo();
   // %client.guid = getField( %authInfo, 3 );
   %client.guid = 0;
   addToServerGuidList( %client.guid );

   // Set admin status
   %client.isAdmin = false;
   %client.isSuperAdmin = false;

   // Save client preferences on the connection object for later use.
   %client.gender = "Male";
   %client.armor = "Light";
   %client.race = "Human";
   %client.skin = addTaggedString( "base" );
   %client.setPlayerName(%name);
   %client.score = 0;

   //
   $instantGroup = ServerGroup;
   $instantGroup = MissionCleanup;
   echo("CADD: " @ %client @ " " @ %client.getAddress());

   // Inform the client of all the other clients
   %count = ClientGroup.getCount();
   for (%cl = 0; %cl < %count; %cl++) {
      %other = ClientGroup.getObject(%cl);
      if ((%other != %client)) {
         // These should be "silent" versions of these messages...
         messageClient(%client, 'MsgClientJoin', "",
               "",
               %other,
               %other.sendGuid,
               %other.score,
               %other.isAIControlled(),
               %other.isAdmin,
               %other.isSuperAdmin);
      }
   }

   // Inform the client we've joined up
   messageClient(%client,
      'MsgClientJoin', "",
      "",
      %client,
      %client.sendGuid,
      %client.score,
      %client.isAiControlled(),
      %client.isAdmin,
      %client.isSuperAdmin);

   // Inform all the other clients of the new guy
   messageAllExcept(%client, -1, 'MsgClientJoin', "",
      "",
      %client,
      %client.sendGuid,
      %client.score,
      %client.isAiControlled(),
      %client.isAdmin,
      %client.isSuperAdmin);

   // If the mission is running, go ahead download it to the client
   if ($missionRunning)
      %client.loadMission();
   $Server::PlayerCount++;

   masterHeartbeat();

   for (%i = 0; %i < ClientGroup.getCount(); %i ++)
      serverCmdGetServerInfo(ClientGroup.getObject(%i));
}

//-----------------------------------------------------------------------------
// A player's name could be obtained from the auth server, but for
// now we use the one passed from the client.
// %realName = getField( %authInfo, 0 );
//
function GameConnection::setPlayerName(%client, %name) {
   // Minimum length requirements
   %name = trim(%name);
   if (strlen(%name) < 3)
      %name = %client.getIP();

   // Make sure the alias is unique, we'll hit something eventually
   if (!isNameUnique(%name)) {
      %isUnique = false;
      for (%suffix = 1; !%isUnique; %suffix ++)  {
         %nameTry = %name @ %suffix;
         %isUnique = isNameUnique(%nameTry);
      }
      %name = %nameTry;
   }

   // Tag the name with the "smurf" color:
   %client.nameBase = %name;
}

function isNameUnique(%name) {
   %count = ClientGroup.getCount();
   for (%i = 0; %i < %count; %i ++) {
      %test = ClientGroup.getObject( %i );
      if (%test.nameBase $= %name)
         return false;
   }
   return true;
}

//-----------------------------------------------------------------------------
// This function is called when a client drops for any reason
//
function GameConnection::onDrop(%client, %reason)
{
   %client.onClientLeaveGame();

   removeFromServerGuidList( %client.guid );
   messageAllExcept(%client, -1, 'MsgClientDrop', '\c1%1 has left the game.', %client.nameBase, %client);

   removeTaggedString(%client.namebase);
   echo("CDROP: " @ %client @ " " @ %client.getAddress());
   $Server::PlayerCount--;

   // Reset the server if everyone has left the game
   if( $Server::PlayerCount == 0 && $Server::Dedicated)
      schedule(0, 0, "resetServerDefaults");

   masterHeartbeat();
}


//-----------------------------------------------------------------------------

function GameConnection::startMission(%this)
{
   // Inform the client the mission starting
   commandToClient(%this, 'MissionStart', $missionSequence);
}


function GameConnection::endMission(%this)
{
   // Inform the client the mission is done
   commandToClient(%this, 'MissionEnd', $missionSequence);
}


//--------------------------------------------------------------------------
// Sync the clock on the client.

function GameConnection::syncClock(%client, %time)
{
   commandToClient(%client, 'syncClock', %time);
}


//--------------------------------------------------------------------------
// Update all the clients with the new score

function GameConnection::incScore(%this,%delta)
{
   %this.score += %delta;
   messageAll('MsgClientScoreChanged', "", %this.score, %this);
}