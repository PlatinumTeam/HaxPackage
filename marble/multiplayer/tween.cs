//-----------------------------------------------------------------------------
// tween.cs
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

//Tweening client code including actually tweening the position of the marble.
function clientCmdTweenDifference(%ghostSize, %ghostPos, %difference, %delta) {
   %ghost = $Game::GhostMarble[$Game::GhostSize[%ghostSize]];
   if (!isObject(%ghost)) { //Oh shit
      buildGhostList();
      return;
   }
   %ghost.tween = %difference;
   %ghost.setTransform(%ghostPos);
}

// Jeff & HiGuy: peform tweening
function clientTweening() {
   cancel($Game::ClientTweenSch);

   %delta = $MP::Tween::ClientTweenDelta;
   $Game::ClientTweenSch = schedule(%delta, 0, "clientTweening");

   for (%i = 0; %i < $Game::GhostCount; %i ++) {
      %ghost = $Game::Ghosts[%i];

      %unlocal = VectorScale(%ghost.tween, %delta / 1000);
      if (%unlocal $= "0 0 0")
         return;

      // Jeff: if the transformations are so close, don't make it look choopy,
      //       this is just to correct lag
      %transform = %ghost.getTransform();
      %position = getWords(%transform,0,2);
      %unlocal = VectorScale(%unlocal, $MP::Tween::ClientTweenScalar);
      %newTransform = VectorAdd(%transform, %unlocal);
      %distance = vectorDist(%position,getWords(%newTransform,0,2));
   //   echo(%distance, true);

      %ghost.setTransform(%newTransform);
   }
}

// HiGuy: Tweening server code which just sends position and speed datas to the clients
//        Update tweening
function serverTweening() {
   if (!$HostingMP)
      return;
   cancel($Game::ServerTweenSch);

   %delta = $MP::Tween::ServerTweenDelta;
   $Game::ServerTweenSch = schedule(%delta, 0, "serverTweening");

   for (%i = 0; %i < ClientGroup.getCount(); %i ++) {
      %client = ClientGroup.getObject(%i);
      if (!isObject(%client.player))
         continue;
      //Reset
      if (%client.lastMarble $= "")
         %client.lastMarble = %client.player.getTransform();

      //Get difference of last position
      %difference = VectorScale(VectorSub(%client.lastMarble, %client.player.getTransform()), 1000 / %delta); //Localize to 1sec
      %client.lastMarble = %client.player.getTransform();

      //Bandwidth is our friends!
      if (%client.lastDifference !$= "" && %client.lastDifference $= %difference)
         return;

      //Send it off
      %client.lastDifference = %difference;
      %client.speed = VectorLen(%difference);

      // Jeff: send it to all other clients except its own
      %tweenPosition = getWords(%difference, 0, 2);
      commandToAllExcept(%client, 'TweenDifference', %client.ghostSize, %client.ghost.getTransform(), %tweenPosition, %delta);
   }
}