//-----------------------------------------------------------------------------
// collision.cs
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

function updateGhostCollision() {
   cancel($CollisionSchedule);
   if (!$MP::Collision::EnableCollision)
      return;
   for (%i = 0; %i < clientGroup.getCount(); %i ++) {
      %client = ClientGroup.getObject(%i);
      if (!isObject(%client.player))
         continue;
      %player = %client.player;
      %ghost = %client.ghost;
      if (isObject(%player) && isObject(%ghost)) {
         %datablock1 = %client.player.getDatablock();
         %pos_p = %client.player.getPosition();
         for (%j = 0; %j < ClientGroup.getCount(); %j ++) {
            if (%j == %i)
               continue;
            %clientJ = ClientGroup.getObject(%j);
            if (!isObject(%clientJ.player))
               continue;

            %client.speed = max(%client.speed, 0.001);
            %clientJ.speed = max(%clientJ.speed, 0.001);

            %datablock2 = %clientJ.player.getDatablock();
            %pos_o = %clientJ.player.getPosition();

            %dist = VectorDist(%pos_p, %pos_o);
            %d2 = %dist - %datablock1.shapeRadius - %datablock2.shapeRadius;

            if ($MP::Collision::EnableNudge && %d2 < 0) {
               %vec = VectorSub(%pos_p, %pos_o);
               if (%vec $= "0 0 0")
                  %vec = "0.1 0 0.0";
               %vec = VectorScale(VectorNormalize(%vec), (%datablock1.shapeRadius + %datablock2.shapeRadius) / 2);
               %inv = VectorScale(%vec, -1);
               %client.nudge(%vec);
               %clientJ.nudge(%inv);
               %client.noCol = 6;
               %clientJ.noCol = 6;
               continue;
            }
            if ($MP::Collision::EnableClip && %d2 < 0) {
               %client.ghost.setDataBlock(DefaultNoGhost);
               %clientJ.ghost.setDataBlock(DefaultNoGhost);
               %client.clipping = true;
               %clientJ.clipping = true;
            } else if ($MP::Collision::EnableClip) {
               if (%client.clipping)
                  %client.ghost.setDataBlock(DefaultGhost);
               if (%clientJ.clipping)
                  %clientJ.ghost.setDataBlock(DefaultGhost);
            }

            %dist -= %datablock1.impactRadius;
            %dist -= %datablock2.impactRadius;
            if (%dist < 0) {

               if (%client.speed < %clientJ.speed || %client.lastCollision == %clientJ)
                  continue;

               if (%client.speed < %datablock1.impactMinimum)
                  continue;

//               %ray = ContainerRayCast(%client.player.position, VectorAdd(%client.player.position, %client.lastDifference), $TypeMasks::StaticShapeObjectType, %client.player);

               %skip = false;
               if (%client.noCol) {
                  %client.noCol --;
                  %skip = true;
               }
               if (%clientJ.noCol) {
                  %clientJ.noCol --;
                  %skip = true;
               }
               if (%skip)
                  continue;

               //collide
               %client.lastCollision = %clientJ;
               %clientJ.lastCollision = %client;

               //Maximum impulse multiplier =D
               %maximum = %datablock1.impactMaximum;
               %maximum2 = %datablock2.impactMaximum;

               //Default multiplier
               %multiplier = %datablock1.impactMultiplier;
               %multiplier2 = %datablock2.impactMultiplier;

               //Default reduction
               %reduction = %datablock1.impactReduction;
               %reduction2 = %datablock2.impactReduction;

               //Calculate marble speed
               %bSpeed = %client.speed + (%clientJ.speed * %datablock1.impactBounceBack);

               //Get source vectors
               %source = VectorSub(%pos_o, %pos_p);
               %source2 = VectorSub(%pos_p, %pos_o);

               //Get impulse vector
               %affect = %source;
               %affect2 = %source2;

               //Get impulse strength
               %affection = min(%bSpeed * %multiplier, %maximum);
               %affection2 = min(%bSpeed * %multiplier2, %maximum2);

               //Scale impulse vector to stength
               %affect = VectorScale(%affect, %affection);
               %affect2 = VectorScale(%affect2, %reduction2);

               //echo("//---------" NL "Max" SPC %maximum NL "Mult" SPC %multiplier NL "BSpeed" SPC %bSpeed NL "Source" SPC %source NL "Affect" SPC %affect NL "Affection" SPC %affection NL "ClientSpeed" SPC %client.speed NL "ClientJSpeed" SPC %clientJ.speed NL "I" SPC %i NL "J" SPC %j NL "//---------");

               //only affect the ghosted client...
               commandToClient(%clientJ,'applyImpulse',%source,%affect);
               commandToClient(%client,'applyImpulse',%source2,%affect2);
            } else {
               %client.lastCollision = "";
            }
         }
      }
   }
   $CollisionSchedule = schedule($MP::Collision::Delta,0,"updateGhostCollision");
}