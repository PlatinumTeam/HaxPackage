//-----------------------------------------------------------------------------
// ghost.cs
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

//HiGuy: Datablock for the ghost
datablock StaticShapeData(DefaultGhost) {
   shapeFile = "~/data/shapes/balls/ghost-superball.dts";
};
datablock StaticShapeData(DefaultNoGhost) {
   shapeFile = "~/data/shapes/balls/ball-superball.dts";
};

//HiGuy: Create a ghost object for a GameConnection
function GameConnection::createGhost(%this) {
   %ghost = new StaticShape() {
      datablock = DefaultGhost;
      //Synchronize *all* the values
      position = %this.player.position;
      rotation = %this.player.rotation;
      scale    = %this.player.scale;
   };
   %this.ghost = %ghost;
   %ghost.setSkinName(%this.player.getSkinName());
   MissionCleanup.add(%ghost);
   if ($MP::Core::RotateMarbles)
      commandToClient(%this, 'startRotLoop');
}

//HiGuy: "serverCmdGhostRotation"
function serverCmdGR(%client, %rot) {
   if (!$MP::Core::RotateMarbles)
      return;
   //Rotation will be updated in updateGhosts()
   %client.ghostRot = %rot;
}

//HiGuy: The main ghost loop function. Hooray!
//       Here, we move all the ghosts and rotate them if necessary
function updateGhosts() {
   cancel($Game::UpdateGhostSchedule);
   for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
      %client = ClientGroup.getObject(%i);

      if (isObject(%client.ghost))
         %client.ghost.setTransform(%client.player.getPosition() SPC %client.ghostRot);
   }
   $Game::UpdateGhostSchedule = schedule($MP::Ghosts::UpdateDelta, 0, updateGhosts);
}