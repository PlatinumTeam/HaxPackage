//-----------------------------------------------------------------------------
// support.cs
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

// Jeff: send a commandToClient to every player
function commandToAll(%command,%a,%b,%c,%d,%e,%f,%g,%h,%i,%j,%k,%l,%m,%n,%o) {
   for (%counter = 0; %counter < ClientGroup.getCount(); %counter ++) {
      %client = ClientGroup.getObject(%counter);
      commandToClient(%client,%command,%a,%b,%c,%d,%e,%f,%g,%h,%i,%j,%k,%l,%m,%n,%o);
   }
}

// Jeff: send a commandToClient to every player except specified client
function commandToAllExcept(%except,%command,%a,%b,%c,%d,%e,%f,%g,%h,%i,%j,%k,%l,%m,%n,%o) {
   for (%counter = 0; %counter < ClientGroup.getCount(); %counter ++) {
      %client = ClientGroup.getObject(%counter);
      if (%client == %except)
         continue;
      commandToClient(%client,%command,%a,%b,%c,%d,%e,%f,%g,%h,%i,%j,%k,%l,%m,%n,%o);
   }
}

// Jeff: send a commandToClient to the host only
function commandToHost(%command,%a,%b,%c,%d,%e,%f,%g,%h,%i,%j,%k,%l,%m,%n,%o) {
   %client = LocalClientConnection;
   if (isObject(%client) && !$Server::Dedicated) {
      commandToClient(%client,%command,%a,%b,%c,%d,%e,%f,%g,%h,%i,%j,%k,%l,%m,%n,%o);
   }
}

// Jeff: check for a local host
function isHost() {
   if ($HostingMP && isObject(LocalClientConnection) && !$Server::Dedicated)
      return true;
   if ($Server::Dedicated && $Server::StandingHost)
      return true;
   return false;
}

//INCLUDE THIS IN MAC BUILD MORONS
function strlwr(%string) {
   %finishedString = "";
   %lower = "abcdefghijklmnopqrstuvwxyz";
   %upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
   for (%i = 0; %i < strlen(%string); %i ++) {
      %letter = getSubStr(%string, %i, 1);
      if (strPos(%upper, %letter) == -1) {
         %finishedString = %finishedString @ %letter;
         continue;
      }
      %pos = strPos(%upper, %letter);
      %letter = getSubStr(%lower, %pos, 1);
      %finishedString = %finishedString @ %letter;
   }
   return %finishedString;
}

function strupr(%string) {
   %finishedString = "";
   %lower = "abcdefghijklmnopqrstuvwxyz";
   %upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
   for (%i = 0; %i < strlen(%string); %i ++) {
      %letter = getSubStr(%string, %i, 1);
      if (strPos(%lower, %letter) == -1) {
         %finishedString = %finishedString @ %letter;
         continue;
      }
      %pos = strPos(%lower, %letter);
      %letter = getSubStr(%upper, %pos, 1);
      %finishedString = %finishedString @ %letter;
   }
   return %finishedString;
}

//Strips any char that isn't in %not
function stripNot(%string, %not) {
   %fin = "";
   for (%i = 0; %i < strLen(%string); %i ++) {
      %letter = getSubStr(%string, %i, 1);
      if (strPos(%not, %letter) != -1)
         %fin = %fin @ %letter;
   }
   return %fin;
}

//Return maximum of all inputs
function max(%a, %b, %c, %d, %e, %f, %g, %h) {
   %max = %a;
   if (%b > %max)
      %max = %b;
   if (%c > %max)
      %max = %c;
   if (%d > %max)
      %max = %d;
   if (%e > %max)
      %max = %e;
   if (%f > %max)
      %max = %f;
   if (%g > %max)
      %max = %g;
   if (%h > %max)
      %max = %h;
   return %max;
}

//Return minimum of all inputs
function min(%a, %b, %c, %d, %e, %f, %g, %h) {
   %min = %a;
   if (%b < %min && %b !$= "")
      %min = %b;
   if (%c < %min && %c !$= "")
      %min = %c;
   if (%d < %min && %d !$= "")
      %min = %d;
   if (%e < %min && %e !$= "")
      %min = %e;
   if (%f < %min && %f !$= "")
      %min = %f;
   if (%g < %min && %g !$= "")
      %min = %g;
   if (%h < %min && %h !$= "")
      %min = %h;
   return %min;
}

//:D
function rot13(%string) {
	%finishedString = "";
	%notRotLower = "abcdefghijklmnopqrstuvwxyz";
	%rotLower = "nopqrstuvwxyzabcdefghijklm";
	%notRotUpper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	%rotUpper = "NOPQRSTUVWXYZABCDEFGHIJKLM";
	for (%i = 0; %i < strlen(%string); %i ++) {
		%letter = getSubStr(%string, %i, 1);
		if (strPos(%notRotLower, %letter) == -1) {
			if (strPos(%notRotUpper, %letter) == -1) {
				%finishedString = %finishedString @ %letter;
				continue;
			}
			%pos = strPos(%notRotUpper, %letter);
			%letter = getSubStr(%rotUpper, %pos, 1);
			%finishedString = %finishedString @ %letter;
			continue;
		}
		%pos = strPos(%notRotLower, %letter);
		%letter = getSubStr(%rotLower, %pos, 1);
		%finishedString = %finishedString @ %letter;
	}
	return %finishedString;
}

//Dumps an object into an eval-able string
function dumpObject(%obj, %format) {
   if ($Server::Dedicated) {
      //Never really finished this. Oh well!
      return "";
      //We don't have that fancy snazz
      %saveSpot = expandFilename("~/data/mpData/temp.mpData");
      %obj.save(%saveSpot);
      %in = new FileObject();
      if (!%in.openForRead(%saveSpot)) {
         %in.close();
         %in.delete();
         return "";
      }
      %inInfo = false;
      %info = "";
      while (!%in.isEOF()) {
         %line = trim(%in.readLine());
         if (%line $= "new ScriptObject(MissionInfo) {") {
            %inInfo = true;
         }
         if (%inInfo) {
            %info = %info @ %line;
         }
         if (%line $= "};" && %inInfo) {
            %inInfo = false;
            break;
         }
      }
      %in.close();
      %in.delete();
      %j = new FileObject();
      if (%j.openForWrite(%saveSpot)) {
         %j.writeLine("Temporary File! Delete me!");
      }
      %j.close();
      %j.delete();
      return %info;
   }
   if (%obj.getClassName() $= "")
      %ret = "new" SPC %obj.className @ "(" @ %obj.getName() @ ") {";
   else
      %ret = "new" SPC %obj.getClassName() @ "(" @ %obj.getName() @ ") {";
   if (!isObject(dumpInspectFields)) {
      new GuiInspector(dumpInspectFields) {
         profile = "GuiDefaultProfile";
         horizSizing = "width";
         vertSizing = "bottom";
         position = "0 0";
         extent = "8 8";
         minExtent = "8 8";
         visible = "true";
         setFirstResponder = "false";
         modal = "true";
         helpTag = "0";
      };
      RootGroup.add(dumpInspectFields);
   }
   dumpInspectFields.inspect(%obj);
   //Get each field...
   for (%i = 0; %i < dumpInspectFields.getCount(); %i ++) {
      %field = dumpInspectFields.getObject(%i);
      if (strStr(%field.getName(), "InspectStatic") == 0) {
         %name = (%format && %format != 4 ? "   " : "") @ getSubStr(%field.getName(), strLen("InspectStatic"), strLen(%field.getName()));
         %name = getSubStr(%name, 0, lastPos(%name, "_"));
         %value = %field.getValue();
         if (%format == 4 && %value == 0 || %value $= "<NULL>" || %value $= "")
            continue;
         %ret = (%format && %format != 4 ? %ret NL %name : %ret @ %name) @ (%format == 2 ? ":" : " = \"") @ %value @ (%format == 2 ? "" : "\";");
      }
      if (strStr(%field.getName(), "InspectDynamic") == 0) {
         %name = (%format && %format != 4 ? "      " : "") @ getSubStr(%field.getName(), strLen("InspectDynamic"), strLen(%field.getName()));
         %name = getSubStr(%name, 0, lastPos(%name, "_"));
         %value = %field.getValue();
         if (%format == 4 && %value == 0 || %value $= "<NULL>" || %value $= "")
            continue;
         %ret = (%format && %format != 4 ? %ret NL %name : %ret @ %name) @ (%format == 2 ? ":" : " = \"") @ %value @ (%format == 2 ? "" : "\";");
      }
   }
   %ret = (%format && %format != 4 ? %ret NL "};" : %ret @ "};");
   if (%format == 3)
      echo(%ret);
   return %ret;
}

//Credits to seizure22!
function dumpgrp(%group, %in) {
   if (%in $= "")
      %in = 0;
   for (%i = 0; %i < %group.getcount(); %i++) {
      %obj = %group.getobject(%i);
      %spaces = "";
      for (%h = %in; %h > 0; %h--) {
         if (%h == 0)
            break;
         %spaces = %spaces @ "   ";
      }
      %name = %obj.getName();
      if (%name $= "")
         %name = %obj.getId();
      echo(%spaces  @ %name @ " - " @ %obj.getclassname());
      if (%obj.getclassname() $= "SimGroup")
         dumpgrp(%obj, %in+1);
   }
}

function lastPos(%string, %search) {
   //Return the last case of %search in %string
   //Example:
   //lastPos("this_text_is_text", "_")
   //Would return strLen("this_text_is");
   for (%i = strLen(%string) - 1; %i >= 0; %i --) {
      %sub = getSubStr(%string, %i, strLen(%search));
      if (%sub $= %search)
         return %i;
   }
   return strLen(%string);
}

// Jeff: gets the line count of a string
function getLineCount(%string) {
   %lineCount = 0;
   %str = strlwr(expandEscape(%string));
   while (strstr(%str,"\\n") != -1) {
      %strPos = strpos(%str,"\\n") + 1;
      %str = getSubStr(%str,%strPos,strlen(%str));
      %lineCount ++;
   }
   return %lineCount;
}

//Absolute value of a vector
function vectorAbs(%vec) {
   %vector = "";
   for (%i = 0; %i < getWordCount(%vec); %i ++) {
      %x = mAbs(getWord(%vec,%i));
      %vector = (%vector $= "") ? %x : %x SPC %vector;
   }
   return %vector;
}

//Exec cs file by name
function eCS(%name) {
   for (%file = findFirstFile("*/" @ %name @ ".cs"); %file !$= ""; %file = findNextFile("*/" @ %name @ ".cs"))
      exec(%file);
}

//Exec gui file by name
function eGUI(%name) {
   for (%file = findFirstFile("*/" @ %name @ ".gui"); %file !$= ""; %file =  findNextFile("*/" @ %name @ ".gui"))
      exec(%file);
}

//Exec dso file by name
function eDSO(%name) {
   for (%file = findFirstFile("*/" @ %name @ ".dso"); %file !$= ""; %file =  findNextFile("*/" @ %name @ ".dso"))
      exec(%file);
}
//Exec file by name
function e(%name) {
   eCS(%name);
   eGUI(%name);
   eDSO(%name);
}

function GuiMLTextCtrl::onAdd(%this) {
   if (%this.text !$= "")
      %this.setText(%this.text);
}