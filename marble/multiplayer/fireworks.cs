//-----------------------------------------------------------------------------
// fireworks.cs
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


//-----------------------------------------------------------------------------
// FireWorks, currently implemented using the particle engine.
//-----------------------------------------------------------------------------

$Game::FireworkModNode = 1.001;
$Game::FireworkModSmoke = 1.004;
$Game::FireworkModRed = 1190;
$Game::FireworkModBlue = 1180;

function findFireworks() {
   if (!isObject(ServerConnection) || $HostingMP)
      return;

   for (%i = 0; %i < ServerConnection.getCount(); %i ++) {
      %obj = ServerConnection.getObject(%i);
      if (%obj.getClassName() $= "ExplosionData") {
         echo("X" SPC %obj.getId() SPC %obj.lifeTimeMS SPC "should be" SPC $Game::FireworkModRed SPC $Game::FireworkModBlue);
         if (%obj.lifeTimeMS == $Game::FireworkModRed) {
            %obj.setName(RedFireWorkExplosion);
            echo(%obj SPC "is RedFireWorkExplosion");
         }
         if (%obj.lifeTimeMS == $Game::FireworkModBlue) {
            %obj.setName(BlueFireWorkExplosion);
            echo(%obj SPC "is BlueFireWorkExplosion");
         }
      } else if (%obj.getClassName() $= "ParticleEmitterNodeData") {
         echo("N" SPC %obj.getId() SPC %obj.timeMultiple SPC "should be" SPC $Game::FireworkModNode);
         if (%obj.timeMultiple == $Game::FireworkModNode) {
            %obj.setName(FireWorkNode);
            echo(%obj SPC "is FireWorkNode");
         }
      } else if (%obj.getClassName() $= "ParticleEmitterData") {
         echo("E" SPC %obj.getId() SPC %obj.ejectionVelocity SPC "should be" SPC $Game::FireworkModSmoke);
         if (%obj.ejectionVelocity == $Game::FireworkModSmoke) {
            %obj.setName(FireWorkSmokeEmitter);
            echo(%obj SPC "is FireWorkSmokeEmitter");
         }
      }
   }
}

function startFireWorks(%pad)
{
   if (!%override && $Server::Dedicated) {
      commandToAll('startFireworks', %pad.getPosition(), %pad.getClassName());
      return;
   }
   if ($HostingMP) return;
   // Create the cleanup group
   if (!isObject(FireWorks)) {
      new SimGroup(FireWorks);
      MissionCleanup.add(FireWorks);
   }

   // Create a ParticleNode to run the emitter
   %position = %pad.getPosition();
   %rotation = %pad.rotation;

   %obj = new ParticleEmitterNode(){
      datablock = FireWorkNode;
      emitter = FireWorkSmokeEmitter;
      position = %position;
      rotation = %rotation;
   };
   FireWorks.add(%obj);

   // Create the explosions
   $Game::FireWorkSchedule = schedule(0,0,"launchWave2",0,%position, %rotation);
}

function endFireWorks2(%position)
{
   if (isObject(FireWorks))
      FireWorks.delete();
   cancel($Game::FireWorkSchedule);
}

//-----------------------------------------------------------------------------

$FireWorkWave[0] = "RedFireWorkExplosion";
$FireWorkWave[1] = "BlueFireWorkExplosion";

function launchWave2(%wave,%position, %rotation)
{
   // Create the explosions
   for (%i = 0; %i < 2; %i++) {
      %obj = new Explosion() {
         datablock = $FireWorkWave[%i];
         position = %position;
         rotation = %rotation;
      };
      FireWorks.add(%obj);
   }

   // Schedule next wave
   if (%wave < 3) {
      %delay = 500 + 1000 * getRandom();
      $Game::FireWorkSchedule = schedule(%delay,0,"launchWave2",%wave + 1,%position, %rotation);
   }
}