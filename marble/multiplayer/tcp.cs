//-----------------------------------------------------------------------------
// tcp.cs
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
// tcp support
// Programmed from project revolution by Jeff Hutchinson and HiGuy
// Portions from the GG forums, documented below.
//
// Jeff: these methods that are listed here are basically support templet methods
//       that hook into the tcpObject class.  Some of them can be over-ridden if
//       necessary.  These methods will make it easier for the programmer to send
//       requests and querys to the server.
//-----------------------------------------------------------------------------

// Jeff: declaration of "constants" - play with them for faster/slower execution of scripts
$TcpObject::reconnectTimer = 2000;
$TcpObject::requestTimer = 700;
$TcpObject::retryTimer = ($platform $= "macos" ? 500 : 800);

function TCPObject::get(%this, %server, %file) {
   %this.auto = true;
   %this.file = %file;
   %this.host = %server;
   %this.mode = "get";
   %this.server = %server;
   %this.retries = 5;
   %this.connect(%server);
   %this.schedule($TcpObject::reconnectTimer, 0, "reconnect");
}

function TCPObject::post(%this, %server, %file, %values) {
   %this.auto = true;
   %this.file = %file;
   %this.values = %values;
   %this.host = %server;
   %this.mode = "post";
   %this.server = %server;
   %this.retries = 5;
   %this.connect(%server);
   %this.schedule($TcpObject::reconnectTimer, 0, "reconnect"); // Jeff: used for mac support, as reconnecting allows tcp sockets to work
}

function TCPObject::reconnect(%this) {
   if (!%this.auto)
      return;
   %this.disconnect();
   cancelAll(%this);
   %this.schedule($TcpObject::reconnectTimer, 0, "reconnect"); // Jeff: used for mac support, as reconnecting allows tcp sockets to work
   %this.connect(%this.server);
}

function TCPObject::onConnected(%this) {
   if (!%this.auto)
      return;
   cancelAll(%this);

   %this.retryTimer = $TcpObject::retryTimer;

   %file = %this.file;
   %host = %this.host;
   if (%this.mode $= "get") {
      %this.lineCount = 0;
      %this.performGet(%file, %host);
      %this.schedule(%this.retryTimer, "retryGet", 0, %file, %host); // Jeff: used for mac support, as reconnecting allows tcp sockets to work
   } else if (%this.mode $= "post") {
      %this.performPost(%file, %host, URLEncode(%this.values));
      %this.schedule(%this.retryTimer, "retryPost", 0, %file, %host, URLEncode(%this.values)); // Jeff: used for mac support, as reconnecting allows tcp sockets to work
   }
}

function TCPObject::onLine(%this, %line) {
   if (!%this.auto)
      return;
	// Jeff: a base method, be sure to parent the current tcp object to this method,
	// so that it will provide extended functionallity to the getOutput() method
	// purpose: keeps track of every single line that is received from the server

   if ($TCPShowOutput || %this.showOutput)
	   %this.echo(%line);

   cancelAll(%this);
   %this.line[%this.lineCount] = trim(%line);
   %this.lineCount ++;
}

function TCPObject::getOutput(%this) {
   if (!%this.auto)
      return;
	// Jeff: this method here will output all information from the onLine() function
	// of the current TCP object by returning it as a string, so be sure to echo if you
	// want to see the results.

   %string = "";
   for (%i = 0; %i < %this.lineCount; %i ++) {
      if (%string $= "") {
         %string = %string @ %this.line[%i];
      } else {
         %string = %string NL %this.line[%i];
      }
   }
   return %string;
}

function TCPObject::performGet(%this, %file, %host) {
   if (!%this.auto)
      return;
	// Jeff:  The actual method that performs the get query
   cancelAll(%this);

   %this.send("GET" SPC %file SPC "HTTP/1.0\r\nHost:" SPC %host SPC "\r\n\r\n");
}

function TCPObject::performPost(%this, %file, %host, %values) {
   if (!%this.auto)
      return;
	// Jeff:  The actual method that performs the post query
	//        %values are sent to the server as arguments
   cancelAll(%this);

   %this.send("POST" SPC %file SPC "HTTP/1.1\nHost:" SPC %host @ "\n" @
      "User-Agent: Torque 1.0 \n" @ "Accept: */*\n" @
      "Content-Type: application/x-www-form-urlencoded; charset=UTF-8\n" @
      "Content-Length: " @ strLen(%values) @ "\n\n" @ %values @ "\n");
}

function TCPObject::retryGet(%this,%count,%file,%host) {
   if (!%this.auto)
      return;
   cancelAll(%this);
   if (%count >= %this.retries) {
      %this.cancel();
      return;
   }
   %this.onRetrySend();
   %this.performGet(%file, %host);

   %this.retryTimer *= 1.5;

   cancelAll(%this);
   %this.schedule(%this.retryTimer, "retryGet", %count + 1, %file, %host);
}

function TCPObject::retryPost(%this, %count, %file, %host, %values) {
   if (!%this.auto)
      return;
   cancelAll(%this);
   if (%count >= %this.retries) {
      %this.cancel();
      return;
   }
   %this.onRetrySend();
   %this.performPost(%file, %host, %values);

   %this.retryTimer *= 1.5;

   cancelAll(%this);
   %this.schedule(%this.retryTimer, "retryPost", %count + 1, %file, %host, %values);
}

function TCPObject::onDisconnect(%this) {
   if (!%this.auto)
      return;
   cancelAll(%this);
   %this.cancel(true);
}

function TCPObject::cancel(%this, %dontDis) {
   if (!%this.auto)
      return;
   cancelAll(%this);
   if (!%dontDis)
      %this.disconnect();
   %this.onFinish();
}

function TCPObject::onFinish(%this) {
   //Jeff: Nothing, just a template for overriding it
}

function TCPObject::onRetrySend(%this) {
   //HiGuy: Nothing here, override this!
}

function TCPObject::onDNSFailed(%this) {
   %this.cancel();
}

function TCPObject::onConnectFailed(%this) {
   %this.cancel();
}

//HiGuy: Cannot remember which one it is
function TCPObject::onConnectionFailed(%this) {
   %this.cancel();
}

function TCPObject::echo(%this, %text) {
   if (!isObject(%this))
      return;

   %name = %this.getName();
   if (%this.auto) {
      if (%name $= "")
         %name = %this.server;
      if (%name $= "")
         %name = %this.file;
   }
   if (%name $= "")
      %name = %this.getId();

   %text = strReplace(%text, "\n", "\n" @ %name SPC ":: ");

   echo(%name SPC "::" SPC %text);
}

//-----------------------------------------------------------------------------
// http://www.garagegames.com/community/blogs/view/10202
//-----------------------------------------------------------------------------

function dec2hex(%val)
{
	// Converts a decimal number into a 2 digit HEX number
	%digits ="0123456789ABCDEF";	//HEX digit table

	// To get the first number we divide by 16 and then round down, using
	// that number as a lookup into our HEX table.
	%firstDigit = getSubStr(%digits,mFloor(%val/16),1);

	// To get the second number we do a MOD 16 and using that number as a
	// lookup into our HEX table.
	%secDigit = getSubStr(%digits,%val % 16,1);

	// return our two digit HEX number
	return %firstDigit @ %secDigit;
}

function hex2dec(%val)
{
	// Converts a decimal number into a 2 digit HEX number
	%digits ="0123456789ABCDEF";	//HEX digit table

	// To get the first number we divide by 16 and then round down, using
	// that number as a lookup into our HEX table.
	%firstDigit = strPos(%digits, getSubStr(%val, 0, 1));

	// To get the second number we do a MOD 16 and using that number as a
	// lookup into our HEX table.
	%secondDigit = strPos(%digits, getSubStr(%val, 1, 1));

	// return our two digit HEX number
	return (%firstDigit * 16) + %secondDigit;
}

function chrValue(%chr)
{
	// So we don't have to do any C++ changes we approximate the function
	// to return ASCII Values for a character.  This ignores the first 31
	// characters and the last 128.

	// Setup our Character Table.  Starting with ASCII character 32 (SPACE)
	%charTable = " !\"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^\'_abcdefghijklmnopqrstuvwxyz{|}~\t\n\r";

	//Find the position in the string for the Character we are looking for the value of
	%value = strpos(%charTable,%chr);

	// Add 32 to the value to get the true ASCII value
	%value = %value + 32;

	//HACK:  Encode TAB, New Line and Carriage Return

	if (%value >= 127)
	{
			if(%value == 127)
				%value = 9;
			if(%value == 128)
				%value = 10;
			if(%value == 129)
				%value = 13;
	}

	//return the value of the character
	return %value;
}

function chrForValue(%chr)
{
	// So we don't have to do any C++ changes we approximate the function
	// to return ASCII Values for a character.  This ignores the first 31
	// characters and the last 128.

	// Setup our Character Table.  Starting with ASCII character 32 (SPACE)
	%charTable = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_'abcdefghijklmnopqrstuvwxyz{|}~\t\n\r";

   %chr -= 32;

	%value = getSubStr(%charTable,%chr, 1);

	return %value;
}

function URLEncode(%rawString)
{
	// Encode strings to be HTTP safe for URL use

	// Table of characters that are valid in an HTTP URL
  %validChars = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz:/.?=_-$(){}~&";


	// If the string we are encoding has text... start encoding
	if (strlen(%rawString) > 0)
	{
		// Loop through each character in the string
		for(%i=0;%i<strlen(%rawString);%i++)
		{
			// Grab the character at our current index location
			%chrTemp = getSubStr(%rawString,%i,1);

			//  If the character is not valid for an HTTP URL... Encode it
 		  if (strstr(%validChars,%chrTemp) == -1)
 		  {
 		  	//Get the HEX value for the character
 		  	%chrTemp = dec2hex(chrValue(%chrTemp));

 		  	// Is it a space?  Change it to a "+" symbol
 		  	if (%chrTemp $= "20")
 		  	{
 		  		%chrTemp = "+";
 		  	}
 		  	else
 		  	{
 		  			// It's not a space, prepend the HEX value with a %
 		  			%chrTemp = "%" @ %chrTemp;
 		  	}
 		  }
 		  // Build our encoded string
			%encodeString = %encodeString @ %chrTemp;
		}
	}
	// Return the encoded string value
	return %encodeString;
}
function URLDecode(%rawString)
{
	// Encode strings from HTTP safe for URL use

	// If the string we are encoding has text... start encoding
	if (strlen(%rawString) > 0)
	{
		// Loop through each character in the string
		for(%i=0;%i<strlen(%rawString);%i++)
		{
			// Grab the character at our current index location
			%chrTemp = getSubStr(%rawString,%i,1);

 		   if (%chrTemp $= "+") {
    		  	// Was it a "+" symbol?  Change it to a space
 		      %chrTemp = " ";
 		   }
			//  If the character was not valid for an HTTP URL... Decode it
 		   if (%chrTemp $= "%") {
 		  	//Get the dec value for the character
 		  	%chrTemp = chrForValue(hex2dec(getSubStr(%rawString, %i + 1, 2)));
         %i += 2;
 		  }
 		  // Build our encoded string
			%encodeString = %encodeString @ %chrTemp;
		}
	}
	// Return the encoded string value
	return %encodeString;
}