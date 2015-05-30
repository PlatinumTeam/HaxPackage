//-----------------------------------------------------------------------------
// chat.cs
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

if (!isObject(PlayGuiChatText))
{
   %chat = new GuiMLTextCtrl(PlayGuiChatText) {
      profile = "GuiMLTextProfile";
      horizSizing = "left";
      vertSizing = "bottom";
      position = "416 100";
      extent = "216 14";
      minExtent = "8 8";
      visible = "1";
      helpTag = "0";
      lineSpacing = "2";
      allowColorChars = "0";
      maxChars = "-1";
   };
   PlayGui.add(%chat);
}



function receiveChat(%msg)
{
   cancel($scrollChat);
   if (ReceiveChatText.getValue() $= "" || getLineCount(ReceiveChatText.getValue()) > 10)
   {
      ReceiveChatText.scroll();
   }
   if (PlayGuiChatText.getValue() $= "" || getLineCount(PlayGuiChatText.getValue()) > 10)
   {
      PlayGuiChatText.scroll();
   }
   %text = (ReceiveChatText.getValue() @ "" @ %msg @ "\n");
   ReceiveChatText.setText(%text);
   %textall = (PlayGuiChatText.getValue() @ "" @ %msg @ "\n");
   ReceiveChatText.setText(%text);
   PlayGuiChatText.setText(%textall);
   //if (PlayGui.isAwake())
   //   Canvas.pushDialog(ReceiveChatDlg);
   $scrollChat = schedule(5000, 0, "chatScroll");
}

function chatScroll() {
   //HiGuy: Added "scrolling"
   cancel($scrollChat);
//   ReceiveChatText.scroll();
   PlayGuiChatText.scroll();
   $scrollChat = schedule(5000, 0, "chatScroll");
}

function ReceiveChatText::scroll(%this)
{
   %this.setText("<just:left><font:DomCasualD:20>" @ getFields(%this.getValue(), 1));
}

function PlayGuiChatText::scroll(%this)
{
   %this.setText("<just:left><font:DomCasualD:20>" @ getFields(%this.getValue(), 1));
}

function clearChat()
{
   ReceiveChatText.setText("");
   PlayGuiChatText.setText("");
}