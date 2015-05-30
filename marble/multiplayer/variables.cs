//-----------------------------------------------------------------------------
// variables.cs
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

// -- Admin Panel --

// Enable or diable the admin panel. Default: true
$MP::Admin::EnableAdminPanel = true;

// Allow admins to kick people. Default: true
$MP::Admin::EnableKick = true;

// Allow admins to ban people. Default: true
$MP::Admin::EnableBan = true;

// Allow admins to ban people by their IPs. Default: true
$MP::Admin::EnableBanIP = true;

// -- Ban / Kick --

// Enable banning and kicking. Default: true
$MP::Ban::EnableBan = true;

// Amount of time in seconds that a player or ip address cannot connect to a server for after they are banned. Set this to 0 for forever. Default: 600
$MP::Ban::BanTime = 600;

// Amount of time in seconds that a player or ip address cannot connect to a server for after they are kicked. Default: 30
$MP::Ban::KickTime = 30;

// -- Chat --

// Enable or disable chat. Default: true
$MP::Chat::EnableChat = true;

// -- Collision --

// Enable or disable collision. Default: true
$MP::Collision::EnableCollision = true;

// Enable or disable nudging the marble when two collide. Use of this is discouraged. Default: false
$MP::Collision::EnableNudge = false;

// Enable or disable clipping marbles when two collide. Highly recommended. Default: true
$MP::Collision::EnableClip = true;

// The amount of time between collision checks. Smaller values are faster, but more laggy. Default: 10
$MP::Collision::Delta = 10;

// -- CRC --

// Validate the CRCs of client's DSO files. Disable to allow modified clients to connect. This is currently bug-prone, as loading levels will disconnect clients. Default: false
$MP::CRC::EnableCRC = false;

// -- Core --

// The amount of time between the main mp loops. Smaller values are faster, but more laggy. Default: 30
$MP::Core::MPLoopDelta = 30;

// Enable rotation on ghosted marbles. Highly recommended! Default: true
$MP::Core::RotateMarbles = true;

// The amount of time between loop for hiding the ghost marbles on the start pad. This doesn't need to be too fast, but should update regularly. Default: 3000
$MP::Core::HideFreddieDelta = 3000;

// The amount of time between builds of the ghost list. This should be somewhat quick, as ghosts will change if someone joins / leaves. Default: 1000
$MP::Core::BuildGhostListDelta = 1000;

// The amount of time between when the game checks for OOB marbles. This should be fast, as OOBs can happen at any time. Default: 100
$MP::Core::OOBLoopDelta = 100;

// The port that MarbleBlast connects to servers on. Must be greater than 1000. Default: 28000
$MP::Core::Port = 28000;

// Enable the dialog that asks for a username when hosting or joining. Default: true
$MP::Core::EnableNameDialog = true;

// The default name for hosting or joining a server. Default: Poser
$MP::Core::DefaultName = "Poser";

// The wait time between updates when waiting for players to finish a level
$MP::Core::WaitDelta = 100;

// Allow playing of levels with moving platforms. Highly discouraged, as moving platforms will crash servers or act in glitchy manners. Default: true
$MP::Core::DisableMPs = true;

// -- Direct Connect --

// Enables the direct connect-to-ip dialog. Highly recommended. Default: true
$MP::Direct::EnableDirectDialog = true;

// -- Ghosts --

// The update time for ghost movement. Smaller values are faster, but are not really sent due to lag. Default: 30
$MP::Ghosts::UpdateDelta = 30;

// -- Master Server --

// Enable the master server list, showing all currently running games
$MP::Master::EnableMasterServer = true;

// The mod name for the master server. No default value, set this accordingly.
$Master::Mod = "ChangeMe";

// The default mode for the master server. You can change this via script and it will update on the next heartbeat.
$MP::Master::Mode = "Normal";

// The update interval to the master server. Anything over 25000 is risky, as the master server might timeout your servers. Shouldn't be too often, as it's using internets. Default: 20000
$MP::Master::UpdateTime = 20000;

// The version number sent to the master server. It will only give you servers with this version, so be sure to change it for updates! Default: $MP_VERSION
$MP::Master::Version = $MP_VERSION;

// -- Particles --

// Enable ghosting of powerup particles to clients. Highly recommended. Default: true
$MP::Particles::EnableParticles = true;

// -- Tweening --

// The tween update time for client-side. Smaller values are faster, but more laggy. Default: 30
$MP::Tween::ClientTweenDelta  = 30;

// The tween update scalar for clients, higher values make the marble glitchier, lower values make the marble laggier. Default: -0.5
$MP::Tween::ClientTweenScalar = -0.5;

// The tween update time for server-side. Smaller values are faster, but more laggy. Default: 100
$MP::Tween::ServerTweenDelta  = 100;
