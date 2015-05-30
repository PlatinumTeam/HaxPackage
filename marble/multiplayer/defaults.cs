//-----------------------------------------------------------------------------
// defaults.cs
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

$MP::Admin::EnableAdminPanel      = true;
$MP::Admin::EnableKick            = true;
$MP::Admin::EnableBan             = true;
$MP::Admin::EnableBanIP           = true;
$MP::Chat::EnableChat             = true;
$MP::CRC::EnableCRC               = false;
$MP::Collision::EnableCollision   = true;
$MP::Collision::EnableNudge       = false;
$MP::Collision::EnableClip        = true;
$MP::Collision::Delta             = 10;
$MP::Core::MPLoopDelta            = 30;
$MP::Core::RotateMarbles          = true;
$MP::Core::HideFreddieDelta       = 3000;
$MP::Core::BuildGhostListDelta    = 1000;
$MP::Core::OOBLoopDelta           = 100;
$MP::Core::Port                   = 28000;
$MP::Core::EnableNameDialog       = true;
$MP::Core::DefaultName            = "Poser";
$MP::Master::Mode                 = "Normal";
$MP::Core::WaitDelta              = 100;
$MP::Core::DisableMPs             = true;
$MP::Direct::EnableDirectDialog   = true;
$MP::Ghosts::UpdateDelta          = 30;
$MP::Master::EnableMasterServer   = true;
$Master::Mod                      = "";
$MP::Master::UpdateTime           = 20000;
$MP::Master::Version              = $MP_VERSION;
$MP::Particles::EnableParticles   = true;
$MP::Tween::ClientTweenDelta      = 30;
$MP::Tween::ClientTweenScalar     = -0.5;
$MP::Tween::ServerTweenDelta      = 100;