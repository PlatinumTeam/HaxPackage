//-----------------------------------------------------------------------------
// crc.cs
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

function GameConnection::transmitCRCs(%this) {
   if ($MP::CRC::EnableCRC && !%this.isHost())
      commandToClient(%this, 'requestCRCs');
   else
      %this.transmitDataBlocks($missionSequence);
}

function clientCmdRequestCRCs() {
   //Kill trace
//   $Con::LogBufferEnabled = false;
//   trace(false);
//   $Con::LogBufferEnabled = true;

   //Get all dsos
   %wildcard = "*/" @ fileBase(filePath($Con::File)) @ "/*.dso";
   for (%file = findFirstFile(%wildcard); %file !$= ""; %file = findNextFile(%wildcard)) {

      //Calculate these
      %crc = getFileCRC(%file);
      %result = "";
      %rand = getSubStr(%crc, 0, 2) * 7391;

      //Hash this
      for (%i = 0; %i < strlen(%crc); %i ++) {
         %char = getSubStr(%crc, %i, 1);
         switch$ (%char) {
            //Making it confusing
            case "-": %result = %result @ "z";
            case "0": %result = %result @ "i";
            case "1": %result = %result @ "e";
            case "2": %result = %result @ "s";
            case "3": %result = %result @ "x";
            case "4": %result = %result @ "v";
            case "5": %result = %result @ "f";
            case "6": %result = %result @ "t";
            case "7": %result = %result @ "w";
            case "8": %result = %result @ "o";
            case "9": %result = %result @ "n";
         }
      }
      //Send
      commandToServer('challengeCRC', fileBase(%file) @ fileExt(%file), %result, %rand);
   }
}

function serverCmdChallengeCRC(%client, %file, %result, %rand) {
//   $Con::LogBufferEnabled = false;
//   trace(false);
//   $Con::LogBufferEnabled = true;

   %rand /= 7391;
   %crc = "";

   for (%i = 0; %i < strlen(%result); %i ++) {
      %char = getSubStr(%result, %i, 1);
      switch$ (%char) {
         case "z": %crc = %crc @ "-";
         case "i": %crc = %crc @ "0";
         case "e": %crc = %crc @ "1";
         case "s": %crc = %crc @ "2";
         case "x": %crc = %crc @ "3";
         case "v": %crc = %crc @ "4";
         case "f": %crc = %crc @ "5";
         case "t": %crc = %crc @ "6";
         case "w": %crc = %crc @ "7";
         case "o": %crc = %crc @ "8";
         case "n": %crc = %crc @ "9";
      }
   }

   %wildcard = "*/" @ fileBase(filePath($Con::File)) @ "/*.dso";
   for (%file1 = findFirstFile(%wildcard); %file1 !$= ""; %file1 = findNextFile(%wildcard))
      %files ++;

   if (mCeil(%rand) == mFloor(%rand) && getSubStr(%crc, 0, 2) $= %rand) {
      %file = filePath($Con::File) @ "/" @ %file;
      if (isFile(%file) && getFileCRC(%file) $= %crc) {
         %client.successfulFiles ++;
         if (%client.successfulFiles == %files)
            %client.transmitDataBlocks($missionSequence);
         return;
      }
   }
   kick(%client, false, "Invalid CRCs");
}

//   %client.transmitDataBlocks($missionSequence);
