//-----------------------------------------------------------------------------
// particles.cs
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
//superjump, superspeed, superbounce, shockabsorber, helicopter

datablock ParticleEmitterNodeData(MarbleParticleNode) {
   timeMultiple = $Game::MPNMultiple;
};

function Marble::makePowerupParticles(%this, %powerup) {
   if (!$MP::Particles::EnableParticles)
      return;
   %id = %powerup.powerupId;
   if (%this.getDatablock().powerUpEmitter[%id] $= "") //If no emitter, don't emit!
      return;
   %client = %this.client;
   if (isObject(%client.emitter[%id]))
      %client.emitter[%id].delete();
   cancel(%client.emitterLockSch[%id]);
   cancel(%client.emitterSch[%id]);
   $Game::PartCount ++;
   %num = 1 + ($Game::PartCount / 10000);
   %client.emitter[%id] = new ParticleEmitterNode() {
      datablock = MarbleParticleNode;
      emitter = %this.getDatablock().powerUpEmitter[%id];
      position = %this.getWorldBoxCenter();
      rotation = "1 0 0 0";
      scale = %num SPC %num SPC %num;
   };
   if (!isObject(%client.emitter[%id])) //Oh well!
      return;
   MissionCleanup.add(%client.emitter[%id]);
   %time = %this.getDatablock().powerUpTime[%id] + 5;
   %client.emitterSch[%id] = %client.emitter[%id].schedule(%time, "delete");
   schedule(50, 0, "commandToAllExcept", %client, 'FollowParticle', %client.nameBase, %num);
   schedule(50, 0, "commandToClient", %client, 'RemoveParticle', %num);
}