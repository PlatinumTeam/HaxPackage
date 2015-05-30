//-----------------------------------------------------------------------------
// Torque Game Engine
//
// Copyright (c) 2001 GarageGames.Com
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Penalty and bonus times.
$Game::TimeTravelBonus = 5000;

// Item respawn values, only powerups currently respawn
$Item::RespawnTime = 7 * 1000;
$Item::PopTime = 10 * 1000;

// Game duration in secs, no limit if the duration is set to 0
$Game::Duration = 0;

// Pause while looking over the end game screen (in secs)
$Game::EndGamePause = 5;

//-----------------------------------------------------------------------------
// Variables extracted from the mission
$Game::GemCount = 0;
$Game::StartPad = 0;
$Game::EndPad = 0;


//-----------------------------------------------------------------------------
//  Functions that implement game-play
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------

function onServerCreated()
{
   // Server::GameType is sent to the master server.
   // This variable should uniquely identify your game and/or mod.
   $Server::GameType = "Marble Game";

   // Server::MissionType sent to the master server.  Clients can
   // filter servers based on mission type.
   $Server::MissionType = "Deathmatch";

   // GameStartTime is the sim time the game started. Used to calculated
   // game elapsed time.
   $Game::StartTime = 0;

   // Load up all datablocks, objects etc.  This function is called when
   // a server is constructed.
   exec("./audioProfiles.cs");
   exec("./camera.cs");
   exec("./markers.cs");
   exec("./triggers.cs");
   exec("./inventory.cs");
   exec("./shapeBase.cs");
   exec("./staticShape.cs");
   exec("./item.cs");

   // Basic items
   exec("./marble.cs");
   exec("./gems.cs");
   exec("./powerUps.cs");
   exec("./buttons.cs");
   exec("./hazards.cs");
   exec("./pads.cs");
   exec("./bumpers.cs");
   exec("./signs.cs");

   exec("./fireworks.cs");

   // Platforms and interior doors
   exec("./pathedInteriors.cs");

   // Keep track of when the game started
   $Game::StartTime = $Sim::Time;

   initMultiplayerServer();
}

function onServerDestroyed()
{
   // Perform any game cleanup without actually ending the game
   destroyGame();
}


//-----------------------------------------------------------------------------

function onMissionLoaded()
{
   // Called by loadMission() once the mission is finished loading.
   // Nothing special for now, just start up the game play.

   $Game::GemCount = countGems(MissionGroup);

   commandToAll('setGravityDir', "1 0 0 0 -1 0 0 0 -1", true);

   // Start the game here if multiplayer...
   if ($Server::ServerType $= "MultiPlayer") {
      masterStartGame();
      startGame();
      if ($Server::Dedicated) {
         switch$ ($platform) {
         case "windows":
            echo("----------------------------------");
            if ($Dcon::Active)
               echo(" For server help, type \"help\"   ");
            else
               echo(" For server help, type \"help();\"");
            echo("----------------------------------");
            if ($Dcon::Active)
               activatePackage(dcon);
         case "macos":
            echo("----------------------------------");
            echo(" There\'s this odd bug in some");
            echo(" Mac servers that makes console");
            echo(" entry not work. We haven\'t found");
            echo(" any solutions as of now, and are");
            echo(" sorry for the inconvenience.");
            echo("----------------------------------");
         case "x86UNIX":
            echo("-----------------------------------");
            if ($Dcon::Active)
               echo(" For server help, type \"help\"");
            else
               echo(" For server help, type \"help();\"");
            echo();
            echo(" Currently, linux servers are");
            echo(" unsupported, but they will still");
            echo(" work for any windows/linux clients");
            echo("-----------------------------------");
            if ($Dcon::Active)
               activatePackage(dcon);
         }
      }
   }
}

function onMissionEnded()
{
   // Called by endMission(), right before the mission is destroyed
   // This part of a normal mission cycling or end.
   endGame();
}

function onMissionReset()
{
   endFireWorks();

   // Reset the players and inform them we're starting
   for( %clientIndex = 0; %clientIndex < ClientGroup.getCount(); %clientIndex++ ) {
      %cl = ClientGroup.getObject( %clientIndex );
      commandToClient(%cl, 'GameStart');
      %cl.resetStats();
   }

   // Start the game duration timer
   if ($Game::Duration)
      $Game::CycleSchedule = schedule($Game::Duration * 1000, 0, "onGameDurationEnd" );
   $Game::Running = true;
   $Game::EndPadGems = 0;

   ServerGroup.onMissionReset();

   // Set the initial state
   if ($Server::ServerType $= "SinglePlayer")
      setGameState("Start");
}

function SimGroup::onMissionReset(%this)
{
   for(%i = 0; %i < %this.getCount(); %i++)
      %this.getObject(%i).onMissionReset();
}

function SimObject::onMissionReset(%this)
{
}

function GameBase::onMissionReset(%this)
{
   %this.getDataBlock().onMissionReset(%this);
}

//-----------------------------------------------------------------------------

function startGame()
{
   if ($Game::Running) {
      error("startGame: End the game first!");
      return;
   }
   $Game::Running = true;
   $Game::Qualified = false;
   $Game::FirstSpawn = false;
   onMissionReset();
   // Matt: set the game state for everyone
   if ($Server::ServerType $= "MultiPlayer")
      setGameState("Waiting");
}

function endGame()
{
   if (!$Game::Running) {
      error("endGame: No game running!");
      return;
   }
   destroyGame();

   $Game::FirstSpawn = false;

   // Inform the clients the game is over
   for (%index = 0; %index < ClientGroup.getCount(); %index++)  {
      %client = ClientGroup.getObject(%index);
      commandToClient(%client, 'EndGameTimes', %client.penaltyTime);
      commandToClient(%client, 'GameEnd', %client.endPad && ($Game::GemCount > 0 ? %client.gemCount : true));
      %client.stopTimer();
   }
}

function pauseGame()
{
   if ($Server::ServerType $= "SinglePlayer")
      $gamePaused = true;
}

function resumeGame()
{
   $gamePaused = false;
}

function destroyGame()
{
   // Cancel any client timers
   for (%index = 0; %index < ClientGroup.getCount(); %index++)
      cancel(ClientGroup.getObject(%index).respawnSchedule);

   // Perform cleanup to reset the game.
   cancel($Game::CycleSchedule);

   $Game::Running = false;
}

//-----------------------------------------------------------------------------

function onGameDurationEnd()
{
   // This "redirect" is here so that we can abort the game cycle if
   // the $Game::Duration variable has been cleared, without having
   // to have a function to cancel the schedule.
   if ($Game::Duration && !isObject(EditorGui))
      cycleGame();
}

function cycleGame()
{
   // This is setup as a schedule so that this function can be called
   // directly from object callbacks.  Object callbacks have to be
   // carefull about invoking server functions that could cause
   // their object to be deleted.
   if (!$Game::Cycling) {
      $Game::Cycling = true;
      $Game::CycleSchedule = schedule(0, 0, "onCycleExec");
   }
}

function onCycleExec()
{
   // End the current game and start another one, we'll pause for a little
   // so the end game victory screen can be examined by the clients.
   endGame();
   $Game::CycleSchedule = schedule($Game::EndGamePause * 1000, 0, "onCyclePauseEnd");
}

function onCyclePauseEnd()
{
   $Game::Cycling = false;
   loadNextMission();
}

function loadNextMission()
{
   %nextMission = "";

   // Cycle to the next level, or back to the start if there aren't
   // any more levels.
   for (%file = findFirstFile($Server::MissionFileSpec);
         %file !$= ""; %file = findNextFile($Server::MissionFileSpec))
      if (strStr(%file, "CVS/") == -1 && strStr(%file, "common/") == -1)
      {
         %mission = getMissionObject(%file);
         if (%mission.type $= MissionInfo.type) {
            if (%mission.level == 1)
               %nextMission = %file;
            if ((%mission.level + 0) == MissionInfo.level + 1) {
               echo("Found one!");
               %nextMission = %file;
               break;
            }
         }
      }
   loadMission(%nextMission);
}



//-----------------------------------------------------------------------------
// GameConnection Methods
// These methods are extensions to the GameConnection class. Extending
// GameConnection make is easier to deal with some of this functionality,
// but these could also be implemented as stand-alone functions.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------

function GameConnection::incPenaltyTime(%this,%dt)
{
   %this.adjustTimer(%dt);
   %this.penaltyTime += %dt;
}

function GameConnection::incBonusTime(%this,%dt)
{
   %this.addBonusTime(%dt);
   %this.bonusTime += %dt;
}


//-----------------------------------------------------------------------------

function GameConnection::onClientEnterGame(%this)
{
   // Create a new camera object.
   %this.camera = new Camera() {
      dataBlock = Observer;
   };
   MissionCleanup.add( %this.camera );
   %this.camera.scopeToClient(%this);

   // Setup game parameters and create the player
   %this.resetStats();
   %this.spawnPlayer();

   // Anchor the player to the start pad
   %this.player.setMode(Start);

   %this.resetTimer();

   // Start the game here for single player
   if ($Server::ServerType $= "SinglePlayer")
      startGame();
   else if ($HostingMP && !%this.isHost()) {
      $Server::Ready[%this] = false;
      %this.respawnPlayer();
      if ($Game::FirstSpawn) {
         commandToClient(%this, 'popPregame');
         %this.setGameState("start");
      } else
         %this.setGameState("waiting");
   }
}

function GameConnection::onClientLeaveGame(%this)
{
   if (isObject(%this.camera))
      %this.camera.delete();
   if (isObject(%this.player))
      %this.player.delete();
   if (isObject(%this.ghost))
      %this.ghost.delete();

   %this.resetGems();
   if ($Server::ServerType $= "SinglePlayer" || $Server::PlayerCount <= 1)
      MissionGroup.onMissionReset();
}

function GameConnection::resetStats(%this)
{
   // Reset game stats
   %this.bonusTime = 0;
   %this.penaltyTime = 0;
   %this.gemCount = 0;

   // Reset the checkpoint
   if (isObject(%this.checkPoint))
      %this.checkPoint.delete();
   %this.checkPoint = new ScriptObject() {
      pad = $Game::StartPad;
      time = 0;
      gemCount = 0;
      penaltyTime = 0;
      bonusTime = 0;
      powerUp = 0;
   };
}


//-----------------------------------------------------------------------------

function GameConnection::onEnterPad(%this)
{
   if (%this.player.getPad() == $Game::EndPad) {
      %this.endPad = true;

      if ($Game::GemCount && %this.gemCount < $Game::GemCount) {
         %this.play2d(MissingGemsSfx);

         if ($Server::ServerType $= "MultiPlayer" && $Server::PlayerCount > 1) {
            $Game::EndPadGems += %this.gemCount;
            if ($Game::EndPadGems < $Game::GemCount) {
               messageClient(%this, 'MsgMissingGems', '\c0Get the other players with gems on the finish pad to group finish!');
               if (%this.gemCount > 0) {
                  for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
                     %client = ClientGroup.getObject(%i);
                     if (!%client.endPad && %client.gemCount)
                        messageClient(%client, 'MsgMissingGems', '\c0%1 is waiting on the finish pad to group finish!', %this.namebase);
                  }
               }
            } else {
               setGameState("endScreen");
               for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
                  %client = ClientGroup.getObject(%i);
                  if (%client.endPad && %client.gemCount) {
                     %client.player.setMode(Victory);
                     messageClient(%client, 'MsgRaceOver', '\c0Congratulations! You\'ve group finished!');
                     gameOver();
                  }
               }
            }
         } else
            messageClient(%this, 'MsgMissingGems', '\c0You can\'t finish without all the gems!!');
      }
      else {
         setGameState("endScreen");
         %this.player.setMode(Victory);
         messageClient(%this, 'MsgRaceOver', '\c0Congratulations! You\'ve finished!');
         gameOver();
      }
   }
}

function GameConnection::onLeavePad(%this)
{
   %this.endPad = false;
   if ($Server::ServerType $= "MultiPlayer") {
      $Game::EndPadGems -= %this.gemCount;
   }
}


//-----------------------------------------------------------------------------

function GameConnection::onOutOfBounds(%this)
{
   if (%this.gameState $= "End")
      return;

   // Reset the player back to the last checkpoint
   %this.setMessage("outOfBounds",2000);
   %this.play2d(OutOfBoundsVoiceSfx);
   %this.player.setOOB(true);
   if(!isEventPending(%this.respawnSchedule))
      %this.respawnSchedule = %this.schedule(2500, respawnPlayer);
}

function Marble::onOOBClick(%this) {
   if ($HostingMP && %this.getOOB())
      %this.client.respawnPlayer();
   if ($Server::ServerType $= "SinglePlayer")
      LocalClientConnection.respawnPlayer();
}

function GameConnection::onDestroyed(%this)
{
   if (%this.gameState $= "End")
      return;

   // Reset the player back to the last checkpoint
   %this.setMessage("destroyed",2000);
   %client.play2d(DestroyedVoiceSfx);
   %this.player.setOOB(true);
   if(!isEventPending(%this.respawnSchedule))
      %this.respawnSchedule = %this.schedule(2500, respawnPlayer);
}

function GameConnection::onFoundGem(%this,%amount)
{
   %this.gemCount += %amount;
   %remaining = $Game::gemCount - %this.gemCount;
   if (%remaining <= 0) {
      messageClient(%this, 'MsgHaveAllGems', '\c0You have all the gems, head for the finish!');
      %this.play2d(GotAllGemsSfx);
      %this.gemCount = $Game::GemCount;
   }
   else
   {
      if(%remaining == 1)
         %msg = '\c0You picked up a gem.  Only one gem to go!';
      else
         %msg = '\c0You picked up a gem.  %1 gems to go!';

      messageClient(%this, 'MsgItemPickup', %msg, %remaining);
      %this.play2d(GotGemSfx);
   }

   %this.setGemCount(%this.gemCount);
}


//-----------------------------------------------------------------------------

function GameConnection::spawnPlayer(%this)
{
   // Combination create player and drop him somewhere
   %spawnPoint = %this.getCheckpointPos();
   %this.createPlayer(%spawnPoint);
   serverPlay2d(spawnSfx);
}

function restartLevel()
{
   LocalClientConnection.respawnPlayer();
}

function GameConnection::respawnPlayer(%this)
{
   //HiGuy: Don't respawn two people at the same place / time
   //       Wait 500ms in-between spawns
   if (getRealTime() - $Game::LastRespawnTime < 500) {
      %this.schedule(300, "respawnPlayer");
      return;
   }
   $Game::LastRespawnTime = getRealTime();

   %this.respawning = true;

   cancelAll(%this.player);
   cancelAll(%this.ghost);
   %this.player.unmountImage(0);
   %this.player.unmountImage(1);
   %this.player.unmountImage(2);
   %this.ghost.unmountImage(0);
   %this.ghost.unmountImage(1);
   %this.ghost.unmountImage(2);

   %this.setGravityDir("1 0 0 0 -1 0 0 0 -1", true);

   %this.resetGems();
   %this.resetStats();

   // Reset the player back to the last checkpoint
   cancel(%this.respawnSchedule);
   if ($Server::ServerType $= "SinglePlayer" || $Server::PlayerCount < 2)
      onMissionReset();
   %this.setGameState("Start"); // Matt
   %this.player.setOOB(false);
   %this.player.setMode(Start);
   %this.player.setPosition(%this.getCheckpointPos(), 0.45);
   %this.player.setPowerUp(%this.checkPoint.powerUp,true);

   %this.gemCount = %this.checkPoint.gemCount;
   %this.penaltyTime = %this.checkPoint.penaltyTime;
   %this.bonusTime = %this.checkPoint.bonusTime;

   %this.setTime(%this.checkPoint.time);
   %this.setGemCount(%this.gemCount);
   serverPlay2d(spawnSfx);

   %this.respawning = false;
}

//-----------------------------------------------------------------------------

function GameConnection::createPlayer(%this, %spawnPoint)
{
   // Matt: No Longer Needed
   //if (%this.player > 0)  {
      //// The client should not have a player currently
      //// assigned.  Assigning a new one could result in
      //// a player ghost.
      //error( "Attempting to create an angus ghost!" );
   //}


   %player = new Marble() {
      dataBlock = DefaultMarble;
      client = %this;
   };
   MissionCleanup.add(%player);

   // Player setup...
   %player.setPosition(%spawnPoint, 0.45);
   %player.setEnergyLevel(60);
   %player.setShapeName(%this.name);
   %player.client.status = 1;

   // Update the camera to start with the player
   %this.camera.setTransform(%player.getEyeTransform());

   // Give the client control of the player
   %this.player = %player;
   %this.setControlObject(%player);

   if ($Server::ServerType $= "Multiplayer") {
      %playerSize = 1 - ($Game::MarbleCount / 10000);
      %player.scale = %playerSize SPC %playerSize SPC %playerSize;
      $Game::MarbleCount ++;

      %this.createGhost();
      %this.ghostSize = %playerSize;

      commandToClient(%this, 'yourScale', %playerSize);
      for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
         commandToClient(%this, 'newMarble', ClientGroup.getObject(%i).namebase, ClientGroup.getObject(%i).ghostSize);
      }
      commandToAll('newMarble', %this.nameBase, %playerSize);
   }
}


//-----------------------------------------------------------------------------

function GameConnection::setCheckpoint(%this,%object)
{
   // Store the last checkpoint which will be used to restore
   // the player when he goes out of bounds.
   if (%object != %this.checkPoint.pad) {
      %this.checkPoint.delete();
      %this.checkPoint = new ScriptObject() {
         pad = %object;
         time = PlayGUI.elapsedTime;
         gemCount = %this.gemCount;
         penaltyTime = %this.penaltyTime;
         bonusTime = %this.bonusTime;
         powerUp = %this.player.getPowerUp();
      };

      messageClient(%this, 'MsgCheckPoint', "\c0Check Point " @ %object.number @ " reached!");
   }
}

//-----------------------------------------------------------------------------
// Support functions
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------

function countGems(%group)
{
   // Count up all gems out there are in the world
   %gems = 0;
   for (%i = 0; %i < %group.getCount(); %i++)
   {
      %object = %group.getObject(%i);
      %type = %object.getClassName();
      if (%type $= "SimGroup")
         %gems += countGems(%object);
      else
         if (%type $= "Item" &&
               %object.getDatablock().classname $= "Gem")
            %gems++;
   }
   return %gems;
}
