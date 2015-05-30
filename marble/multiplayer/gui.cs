//-----------------------------------------------------------------------------
// gui.cs
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

//GuiInvisibleScrollProfile by Matt

if(!isObject(GuiInvisibleScrollProfile)) new GuiControlProfile (GuiInvisibleScrollProfile)
{
   opaque = false;
   bitmap = ($platform $= "macos") ? "common/ui/osxScroll" : "common/ui/darkScroll";
   hasBitmapArray = true;
};

// Matt
if(!isObject(GuiBigInvisibleScrollProfile)) new GuiControlProfile (GuiBigInvisibleScrollProfile : GuiInvisibleScrollProfile)
{
   fontType = "DomCasualD";
   fontSize = 32;
};

// GuiInvisibleTextEditProfile by Matt

if(!isObject(GuiInvisibleTextEditProfile)) new GuiControlProfile (GuiInvisibleTextEditProfile) {
   opaque = false;
   fontColor = "0 0 0";
   fontColorHL = "255 255 255";
   fontColorNA = "128 128 128";
   textOffset = "0 2";
   autoSizeWidth = false;
   autoSizeHeight = false;
   tab = true;
   canKeyFocus = true;
};

// HiGuy
if(!isObject(GuiMasterTextListProfile)) new GuiControlProfile (GuiMasterTextListProfile : GuiTextListProfile)
{
   fontSize = 12;
   fontColor = "0 0 0";
   fontColors[4] = "0 51 0";
   fontType = "Arial";
   justify = "left";
};

// Matt
if(!isObject(GuiBigMasterTextListProfile)) new GuiControlProfile (GuiBigMasterTextListProfile : GuiMasterTextListProfile)
{
   fontType = "DomCasualD";
   fontSize = 20;
};

// Matt
if(!isObject(GuiBigTextListProfile)) new GuiControlProfile (GuiBigTextListProfile : GuiTextListProfile)
{
   fontType = "DomCasualD";
   fontSize = 20;
};

// Matt
if(!isObject(GuiBigWhiteTextEditProfile)) new GuiControlProfile (GuiBigWhiteTextEditProfile : GuiTextEditProfile)
{
   fontType = "DomCasualD";
   fontSize = 32;
   fillColor = "0 0 0";
   fillColorHL = "50 50 50";
   border = 1;
   borderThickness = 0;
   borderColor = "138 157 237";
   fontColor = "255 255 255";
   fontColorHL = "255 255 255";
   fontColorNA = "128 128 128";
   autoSizeHeight = false;
};
