using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Dalamud.Game.Chat;
using Dalamud.Game.Chat.SeStringHandling;
using Dalamud.Plugin;
using Dalamud.Game.Command;
using IvanAkcheurov.NTextCat.Lib;
using Yandex;
using ImGuiNET;
using GoogleTranslateFreeApi;
using System.IO;
using System.Runtime.CompilerServices;
using Dalamud.Configuration;
using Num = System.Numerics;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;


namespace DalamudPlugin
{
    public partial class ChatExtenderPlugin : IDalamudPlugin
    {
        private void ChatUI()
        {
            if (nulled)
            {
                sleep--;
                if (sleep > 0) { return; }

                scan1 = pluginInterface.TargetModuleScanner.ScanText("E8 ?? ?? ?? ?? 41 b8 01 00 00 00 48 8d 15 ?? ?? ?? ?? 48 8b 48 20 e8 ?? ?? ?? ?? 48 8b cf");
                scan2 = pluginInterface.TargetModuleScanner.ScanText("e8 ?? ?? ?? ?? 48 8b cf 48 89 87 ?? ?? 00 00 e8 ?? ?? ?? ?? 41 b8 01 00 00 00");

                getBaseUIObj = Marshal.GetDelegateForFunctionPointer<GetBaseUIObjDelegate>(scan1);
                getUI2ObjByName = Marshal.GetDelegateForFunctionPointer<GetUI2ObjByNameDelegate>(scan2);
                chatLog = getUI2ObjByName(Marshal.ReadIntPtr(getBaseUIObj(), 0x20), "ChatLog", 1);

                if (chatLog != IntPtr.Zero)
                {
                    chatLogPanel_0 = getUI2ObjByName(Marshal.ReadIntPtr(getBaseUIObj(), 0x20), "ChatLogPanel_0", 1);
                    chatLogStuff = Marshal.ReadIntPtr(chatLog, 0xc8);
                }

            }

            if (pluginInterface.ClientState.LocalPlayer == null || getUI2ObjByName(Marshal.ReadIntPtr(getBaseUIObj(), 0x20), "ChatLog", 1) == IntPtr.Zero)
            {
                if(getUI2ObjByName(Marshal.ReadIntPtr(getBaseUIObj(), 0x20), "ChatLog", 1) == IntPtr.Zero)
                {
                    sleep = 1000;
                    nulled = true;
                    chatLogStuff = IntPtr.Zero;
                    chatLog = IntPtr.Zero;
                    chatLogPanel_0 = IntPtr.Zero;
                }
            }
            else
            {
                nulled = false;
            }

            if (nulled) { return; }

            ImGuiWindowFlags chat_window_flags = 0;
            ImGuiWindowFlags chat_sub_window_flags = 0;
            if (no_titlebar) chat_window_flags |= ImGuiWindowFlags.NoTitleBar;
            if (no_scrollbar) chat_window_flags |= ImGuiWindowFlags.NoScrollbar;
            if (no_scrollbar) chat_sub_window_flags |= ImGuiWindowFlags.NoScrollbar;
            if (!no_menu) chat_window_flags |= ImGuiWindowFlags.MenuBar;
            if (no_move) chat_window_flags |= ImGuiWindowFlags.NoMove;
            if (no_resize) chat_window_flags |= ImGuiWindowFlags.NoResize;
            if (no_collapse) chat_window_flags |= ImGuiWindowFlags.NoCollapse;
            if (no_nav) chat_window_flags |= ImGuiWindowFlags.NoNav;
            if (no_mouse) { chat_window_flags |= ImGuiWindowFlags.NoMouseInputs; }
            if (no_mouse2) { chat_sub_window_flags |= ImGuiWindowFlags.NoMouseInputs; }


            //otherwise update all the values
            if (chatLogStuff != IntPtr.Zero)
            {
                var chatLogProperties = Marshal.ReadIntPtr(chatLog, 0xC8);
                Marshal.Copy(chatLogProperties + 0x44, chatLogPosition, 0, 2);
                Width = Marshal.ReadInt16(chatLogProperties + 0x90);
                Height = Marshal.ReadInt16(chatLogProperties + 0x92);
                Alpha = Marshal.ReadByte(chatLogProperties + 0x73);
                BoxHide = Marshal.ReadByte(chatLogProperties + 0x182);
            }
            //Get initial hooks in
            else
            {
                chatLog = getUI2ObjByName(Marshal.ReadIntPtr(getBaseUIObj(), 0x20), "ChatLog", 1);
                if (chatLog != IntPtr.Zero)
                {
                    chatLogPanel_0 = getUI2ObjByName(Marshal.ReadIntPtr(getBaseUIObj(), 0x20), "ChatLogPanel_0", 1);
                    chatLogStuff = Marshal.ReadIntPtr(chatLog, 0xc8);
                }
            }

            if (!skipfont)
            {
                ImGui.PushFont(font);
                if (hideWithChat & Alpha.ToString() != "0")
                {
                    if (chatWindow)
                    {
                        if (flickback)
                        {
                            no_mouse = false;
                            flickback = false;
                        }
                        ImGui.SetNextWindowSize(new Num.Vector2(200, 100), ImGuiCond.FirstUseEver);
                        ImGui.SetNextWindowBgAlpha(alpha);
                        ImGui.Begin("Another Window", ref chatWindow, chat_window_flags);
                        ImGuiTabBarFlags tab_bar_flags = ImGuiTabBarFlags.None;

                        if (overrideChat)
                        {
                            ImGui.SetWindowPos(new Num.Vector2(chatLogPosition[0] + 15, chatLogPosition[1] + 10));
                            ImGui.SetWindowSize(new Num.Vector2(Width - 27, Height - 75));
                            //Marshal.WriteByte(chatLogPanel_0 + 0x182, BoxOff);
                        }
                        else
                        {
                            //Marshal.WriteByte(chatLogPanel_0 + 0x182, BoxOn);
                        }

                        if (ImGui.BeginTabBar("Tabs", tab_bar_flags))
                        {
                            int loop = 0;
                            foreach (var tab in items)
                            {
                                if (tab.Enabled)
                                {
                                    //WIP

                                    if (tab.sel)
                                    {
                                        ImGui.PushStyleColor(ImGuiCol.Tab, tab_sel);
                                        ImGui.PushStyleColor(ImGuiCol.Text, tab_sel_text);
                                        tab.sel = false;
                                    }
                                    else if (tab.msg)
                                    {
                                        ImGui.PushStyleColor(ImGuiCol.Tab, tab_ind);
                                        ImGui.PushStyleColor(ImGuiCol.Text, tab_ind_text);
                                    }
                                    else
                                    {
                                        ImGui.PushStyleColor(ImGuiCol.Tab, tab_norm);
                                        ImGui.PushStyleColor(ImGuiCol.Text, tab_norm_text);
                                    }



                                    if (ImGui.BeginTabItem(tab.Title))
                                    {
                                        tab.sel = true;

                                        float footer = (ImGui.GetStyle().ItemSpacing.Y) / 2 + ImGui.GetFrameHeightWithSpacing();
                                        if (!tab.FilterOn) { footer = 0; }
                                        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Num.Vector2(space_hor, space_ver));
                                        ImGui.BeginChild("scrolling", new Num.Vector2(0, -footer), false, chat_sub_window_flags);


                                        foreach (ChatText line in tab.Chat)
                                        {
                                            if (tab.FilterOn)
                                            {
                                                if (ContainsText(line.Text, tab.Filter))
                                                {
                                                    if (tab.Config[0])
                                                    {
                                                        if (fontShadow){ShadowFont(line.Time + " ");}
                                                        ImGui.TextColored(timeColour, line.Time + " "); ImGui.SameLine();
                                                    }
                                                    if (tab.Config[1] && tab.Chans[ConvertForArray(line.Channel)]) 
                                                    {
                                                        if (fontShadow) { ShadowFont(line.ChannelShort + " "); }
                                                        ImGui.TextColored(chanColour[ConvertForArray(line.Channel)], line.ChannelShort + " "); ImGui.SameLine();
                                                    }
                                                    if (line.Sender.Length > 0)
                                                    {
                                                        if (fontShadow) { ShadowFont(line.Sender + ":"); }
                                                        ImGui.TextColored(nameColour, line.Sender + ":"); ImGui.SameLine();
                                                    }

                                                    int count = 0;
                                                    foreach (TextTypes textTypes in line.Text)
                                                    {
                                                        if (textTypes.Type == PayloadType.RawText)
                                                        {
                                                            ImGui.PushStyleColor(ImGuiCol.Text, logColour[line.ChannelColour]);
                                                            Wrap(textTypes.Text);
                                                            ImGui.PopStyleColor();
                                                        }

                                                        if (textTypes.Type == PayloadType.MapLink)
                                                        {
                                                            if (ImGui.GetContentRegionAvail().X - 5 - ImGui.CalcTextSize(textTypes.Text).X < 0) { ImGui.Text(""); }
                                                            if (ImGui.SmallButton(textTypes.Text))
                                                            {
                                                                this.pluginInterface.Framework.Gui.OpenMapWithMapLink((Dalamud.Game.Chat.SeStringHandling.Payloads.MapLinkPayload)textTypes.Payload);
                                                            }
                                                        }

                                                        if (count < (line.Text.Count - 1))
                                                        {
                                                            ImGui.SameLine(); count++;
                                                        }

                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (tab.Config[0])
                                                {
                                                    if (fontShadow) { ShadowFont(line.Time + " "); }
                                                    ImGui.TextColored(timeColour, line.Time + " "); ImGui.SameLine();
                                                }
                                                if (tab.Config[1] && tab.Chans[ConvertForArray(line.Channel)])
                                                {
                                                    if (fontShadow) { ShadowFont(line.ChannelShort + " "); }
                                                    ImGui.TextColored(chanColour[ConvertForArray(line.Channel)], line.ChannelShort + " "); ImGui.SameLine();
                                                }
                                                if (line.Sender.Length > 0)
                                                {
                                                    if (fontShadow) { ShadowFont(line.Sender + ":"); }
                                                    ImGui.TextColored(nameColour, line.Sender + ":"); ImGui.SameLine();
                                                }

                                                int count = 0;
                                                foreach (TextTypes textTypes in line.Text)
                                                {
                                                    if (textTypes.Type == PayloadType.RawText)
                                                    {
                                                        ImGui.PushStyleColor(ImGuiCol.Text, logColour[line.ChannelColour]);
                                                        Wrap(textTypes.Text);
                                                        ImGui.PopStyleColor();
                                                    }

                                                    if (textTypes.Type == PayloadType.MapLink)
                                                    {
                                                        if (ImGui.GetContentRegionAvail().X - 5 - ImGui.CalcTextSize(textTypes.Text).X < 0) { ImGui.Text(""); }
                                                        if (ImGui.SmallButton(textTypes.Text))
                                                        {
                                                            this.pluginInterface.Framework.Gui.OpenMapWithMapLink((Dalamud.Game.Chat.SeStringHandling.Payloads.MapLinkPayload)textTypes.Payload);
                                                        }
                                                    }

                                                    if (count < (line.Text.Count - 1))
                                                    {
                                                        ImGui.SameLine();
                                                        count++;
                                                    }

                                                }

                                            }


                                        }
                                        if (tab.Scroll == true)
                                        {
                                            ImGui.SetScrollHereY();
                                            tab.Scroll = false;
                                        }
                                        ImGui.PopStyleVar();
                                        ImGui.EndChild();

                                        if (tab.FilterOn)
                                        {
                                            ImGui.InputText("Filter Text", ref tab.Filter, 999);
                                            if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Only show lines with this text."); }
                                        }

                                        if (no_mouse2 && !no_mouse)
                                        {
                                            Num.Vector2 vMin = ImGui.GetWindowContentRegionMin();
                                            Num.Vector2 vMax = ImGui.GetWindowContentRegionMax();

                                            vMin.X += ImGui.GetWindowPos().X;
                                            vMin.Y += ImGui.GetWindowPos().Y + 22;
                                            vMax.X += ImGui.GetWindowPos().X - 22;
                                            vMax.Y += ImGui.GetWindowPos().Y;

                                            if (ImGui.IsMouseHoveringRect(vMin, vMax)) { no_mouse = true; flickback = true; }
                                        }
                                        tab.msg = false;
                                        ImGui.EndTabItem();
                                    }
                                    ImGui.PopStyleColor();
                                    ImGui.PopStyleColor();
                                }
                                loop++;
                            }
                            ImGui.EndTabBar();
                            ImGui.End();
                        }
                    }
                }
                ImGui.PopFont();
            }



            if (configWindow)
            {
                ImGui.SetNextWindowSize(new Num.Vector2(300, 500), ImGuiCond.FirstUseEver);
                ImGui.Begin("Chat Config", ref configWindow);
                ImGuiTabBarFlags tab_bar_flags = ImGuiTabBarFlags.None;

                float footer = (ImGui.GetStyle().ItemSpacing.Y) / 2 + ImGui.GetFrameHeightWithSpacing();
                ImGui.BeginChild("scrolling", new Num.Vector2(0, -footer), false);

                if (ImGui.BeginTabBar("Tabs", tab_bar_flags))
                {

                    if (ImGui.BeginTabItem("Config"))
                    {

                        ImGui.Text("");

                        ImGui.Columns(3);

                        ImGui.Checkbox("Show Chat Extender", ref chatWindow);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Enable/Disable the Chat Extender"); }
                        ImGui.NextColumn();
                        ImGui.Checkbox("Enable Translations", ref allowTranslation);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Enable Translations from JPN to ENG"); }
                        ImGui.NextColumn();
                        ImGui.Checkbox("Chat Bubbles", ref bubblesWindow);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Enable Chat Bubbles"); }
                        ImGui.NextColumn();

                        ImGui.Checkbox("24 Hour Time", ref hourTime);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Switch to 24 Hour (Military) time."); }
                        ImGui.NextColumn();

                        //ImGui.Checkbox("Hide with FFXIV Chat", ref hideWithChat);
                        //if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Spoopy"); }
                        ImGui.Text("");
                        ImGui.NextColumn();
                        ImGui.Text("");
                        ImGui.NextColumn();

                        ImGui.Checkbox("Hide Scrollbar", ref no_scrollbar);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Shows ScrollBar"); }
                        ImGui.NextColumn();
                        ImGui.Checkbox("Lock Window Position", ref no_move);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Lock/Unlock the position of the Chat Extender"); }
                        ImGui.NextColumn();
                        ImGui.Checkbox("Lock Window Size", ref no_resize);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Lock/Unlock the size of the Chat Extender"); }
                        ImGui.NextColumn();

                        ImGui.Checkbox("ClickThrough Tab Bar", ref no_mouse);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Enable/Disable being able to clickthrough the Tab Bar of the Chat Extender"); }
                        ImGui.NextColumn();
                        ImGui.Checkbox("ClickThrough Chat", ref no_mouse2);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Enable/Disable being able to clickthrough the Chat Extension chatbox"); }
                        ImGui.NextColumn();
                        ImGui.Text("");
                        ImGui.NextColumn();

                        ImGui.Columns(1);
                        ImGui.SliderFloat("Chat Extender Alpha", ref alpha, 0.001f, 0.999f);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Alter the Alpha of the Chat Extender"); }
                        ImGui.Text("");

                        ImGui.Text("");
                        ImGui.Text("Highlight Example");
                        HighlightText();
                        ImGui.InputText("##HighlightText", ref tempHigh, 999); if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Will highlight EXACT matches only. Seperate words with [,]."); }
                        ImGui.SameLine();
                        if (ImGui.Button("Apply"))
                        {
                            high.highlights = tempHigh.Split(',');
                        }
                        ImGui.Columns(4);
                        ImGui.SliderInt("Alpha", ref high.htA, 0, 255); ImGui.NextColumn();
                        ImGui.SliderInt("Blue", ref high.htB, 0, 255); ImGui.NextColumn();
                        ImGui.SliderInt("Green", ref high.htG, 0, 255); ImGui.NextColumn();
                        ImGui.SliderInt("Red", ref high.htR, 0, 255); ImGui.NextColumn();
                        ImGui.Columns(1);
                        ImGui.Text("");

                        ImGui.Columns(1);


                        ImGui.EndTabItem();
                    }


                    if (ImGui.BeginTabItem("Channels"))
                    {
                        ImGui.Columns(4);
                        ImGui.Text("Example"); ImGui.NextColumn();
                        ImGui.Text("Colour 1"); ImGui.NextColumn();
                        ImGui.Text("Colour 2"); ImGui.NextColumn();
                        ImGui.Text(""); ImGui.NextColumn();
                        ImGui.TextColored(timeColour, "[12:00]"); ImGui.NextColumn();
                        ImGui.ColorEdit4("Time Colour", ref timeColour, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel); ImGui.NextColumn();
                        ImGui.Text(""); ImGui.NextColumn();
                        ImGui.Text(""); ImGui.NextColumn();
                        ImGui.TextColored(nameColour, "Player Names"); ImGui.NextColumn();
                        ImGui.ColorEdit4("Name Colour", ref nameColour, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel); ImGui.NextColumn();
                        ImGui.Text(""); ImGui.NextColumn();
                        ImGui.Text(""); ImGui.NextColumn();
                        for (int i = 0; i < (Channels.Length); i++)
                        {
                            ImGui.InputText("##Tab Name" + i.ToString(), ref Chan[i], 99); ImGui.NextColumn();
                            ImGui.TextColored(chanColour[i], "[" + Channels[i] + "]"); ImGui.SameLine(); ImGui.TextColored(logColour[i], "Text"); ImGui.NextColumn();
                            ImGui.ColorEdit4(Channels[i] + " Colour1", ref chanColour[i], ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel); ImGui.NextColumn();
                            ImGui.ColorEdit4(Channels[i] + " Colour2", ref logColour[i], ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel); ImGui.NextColumn();
                        }
                        ImGui.Columns(1);
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Tabs"))
                    {

                        if (ImGui.Button("Add New Tab"))
                        {
                            tempTitle = "New";

                            while (CheckDupe(items, tempTitle))
                            { tempTitle += "."; }

                            items.Add(new DynTab(tempTitle, new ConcurrentQueue<ChatText>(), true));
                            tempTitle = "Title";
                        }
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Add a new Tab to the Chat Extender"); }

                        if (ImGui.TreeNode("Tab Order"))
                        {

                            ImGui.Columns(3);
                            ImGui.Text("Tab"); ImGui.NextColumn();
                            ImGui.Text(""); ImGui.NextColumn();
                            ImGui.Text(""); ImGui.NextColumn();

                            List<TabBase> temp_clone = new List<TabBase>();
                            temp_clone = CopyItems(items);
                            for (int i = 0; i < (items.Count); i++)
                            {
                                ImGui.Text(items[i].Title); ImGui.NextColumn();
                                if (i > 0)
                                {
                                    if (ImGui.Button("^##" + i.ToString()))
                                    {
                                        TabBase mover = temp_clone[i];
                                        temp_clone.RemoveAt(i);
                                        temp_clone.Insert(i - 1, mover);
                                    }
                                }
                                ImGui.NextColumn();
                                if (i < items.Count - 1)
                                {
                                    if (ImGui.Button("v##" + i.ToString()))
                                    {
                                        TabBase mover = temp_clone[i];
                                        temp_clone.RemoveAt(i);
                                        temp_clone.Insert(i + 1, mover);
                                    }
                                }
                                ImGui.NextColumn();
                            }
                            ImGui.Columns(1);
                            items = CopyItems(temp_clone);
                            ImGui.TreePop();


                        }

                        ImGui.Separator();
                        foreach (var tab in items)
                        {
                            if (tab.Enabled)
                            {
                                if (ImGui.TreeNode(tab.Title))
                                {
                                    float footer2 = (ImGui.GetStyle().ItemSpacing.Y) / 2 + ImGui.GetFrameHeightWithSpacing();
                                    ImGui.BeginChild("scrolling", new Num.Vector2(0, -footer2), false);
                                    ImGui.InputText("##Tab Name", ref tempTitle, bufSize);
                                    ImGui.SameLine();
                                    if (ImGui.Button("Set Tab Title"))
                                    {
                                        if (tempTitle.Length == 0) { tempTitle += "."; }

                                        while (CheckDupe(items, tempTitle))
                                        { tempTitle += "."; }

                                        tab.Title = tempTitle;
                                        tempTitle = "Title";
                                    }
                                    if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Change the title of the Tab"); }

                                    ImGui.Columns(3);

                                    ImGui.Checkbox("Time Stamp", ref tab.Config[0]);
                                    if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Show Timestamps in this Tab"); }
                                    ImGui.NextColumn();
                                    ImGui.Checkbox("Channel", ref tab.Config[1]);
                                    if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Show the Channel the message came from"); }
                                    ImGui.NextColumn();
                                    ImGui.Checkbox("Translate", ref tab.Config[2]);
                                    if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Enable Japanese -> English translation"); }
                                    ImGui.NextColumn();

                                    ImGui.Checkbox("AutoScroll", ref tab.AutoScroll);
                                    if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Enable the Chat to scroll automatically on a new message"); }
                                    ImGui.NextColumn();
                                    ImGui.Checkbox("Save to file", ref tab.Config[3]);
                                    if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Write this tab to '\\My Documents\\FFXIV_ChatExtender\\Logs\\<YYYYMMDD>_TAB.txt"); }
                                    ImGui.NextColumn();
                                    ImGui.Checkbox("Enable Filter", ref tab.FilterOn);
                                    if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Enable Filtering of text"); }
                                    ImGui.NextColumn();

                                    ImGui.Columns(1);

                                    ImGui.Text("");


                                    //TODO: Add a confirm prompt

                                    if (EnabledTabs(items) > 1)
                                    {
                                        if (ImGui.Button("Delete Tab"))
                                        { tab.Enabled = false; }
                                    }
                                    if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Removes Tab"); }



                                    ImGui.Columns(2);
                                    ImGui.Text("Channel"); ImGui.NextColumn();
                                    if (tab.Config[1]) { ImGui.Text("Show Short"); }
                                    else { ImGui.Text(""); }
                                    ImGui.NextColumn();

                                    for (int i = 0; i < (Channels.Length); i++)
                                    {
                                        ImGui.PushStyleColor(ImGuiCol.Text, chanColour[i]);
                                        ImGui.Checkbox("[" + Channels[i] + "]", ref tab.Logs[i]); ImGui.NextColumn();
                                        if (tab.Config[1]) { ImGui.Checkbox(Chan[i], ref tab.Chans[i]); }
                                        else { ImGui.Text(""); }
                                        ImGui.NextColumn();
                                        ImGui.PopStyleColor();
                                    }
                                    ImGui.Columns(1);
                                    ImGui.EndChild();
                                    ImGui.TreePop();


                                }
                            }
                        }
                        ImGui.EndTabItem();
                    }

                    if (allowTranslation)
                    {
                        if (ImGui.BeginTabItem("Translator"))
                        {
                            ImGui.Checkbox("Inject Translation", ref injectChat);
                            if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Inject translated text into the normal FFXIV Chatbox"); }

                            ImGui.Text("Surrounds of Translated text");
                            ImGui.PushItemWidth(24);
                            ImGui.InputText("##Left", ref lTr, 3); ImGui.SameLine();
                            if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Alter the characters on the left of Translated text"); }
                            ImGui.PopItemWidth();
                            ImGui.Text("Translation"); ImGui.SameLine();
                            ImGui.PushItemWidth(24);
                            ImGui.InputText("##Right", ref rTr, 3);
                            if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Alter the characters on the right of Translated text"); }
                            ImGui.PopItemWidth();
                            ImGui.Text("");
                            ImGui.EndTabItem();

                            ImGui.InputText("Yandex Key", ref yandex, 999);
                            if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Key to allow the translator to use the Yandex service"); }

                            ImGui.Text("Translator");
                            if (translator == 1)
                            {
                                ImGui.Text("[Google] is set.");
                                if (ImGui.Button("Switch to Yandex"))
                                {
                                    translator = 2;
                                }
                            }

                            if (translator == 2)
                            {
                                ImGui.Text("[Yandex] is set.");
                                if (ImGui.Button("Switch to Google"))
                                {
                                    translator = 1;
                                }
                            }
                            ImGui.EndTabItem();

                        }
                    }

                    if (ImGui.BeginTabItem("Font"))
                    {
                        ImGui.Columns(3);
                        ImGui.PushItemWidth(124);
                        ImGui.InputInt("H Space", ref space_hor);
                        ImGui.PopItemWidth();
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Horizontal spacing of chat text"); }
                        ImGui.NextColumn();
                        ImGui.PushItemWidth(124);
                        ImGui.InputInt("V Space", ref space_ver);
                        ImGui.PopItemWidth();
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Vertical spacing of cha text"); }
                        ImGui.NextColumn();
                        ImGui.Text("");
                        ImGui.NextColumn();
                        ImGui.Columns(1);
                        ImGui.PushItemWidth(124);
                        ImGui.InputInt("Font Size", ref fontsize); ImGui.SameLine();
                        ImGui.PopItemWidth();
                        if (ImGui.SmallButton("Apply"))
                        {
                            UpdateFont();
                        }
                        ImGui.Checkbox("Font Shadow", ref fontShadow);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("WARNING! This is a large tax on processing.\nIf you encounter slowdown, disable this!"); }
                        ImGui.EndTabItem();
                    }
                    if (bubblesWindow)
                    {
                        if (ImGui.BeginTabItem("Bubbles"))
                        {
                            ImGui.Columns(3);
                            //ImGui.Checkbox("Debug", ref drawDebug);
                            ImGui.Checkbox("Displacement Up", ref boolUp); ImGui.NextColumn();
                            if (ImGui.IsItemHovered()) { ImGui.SetTooltip("When bubbles collide, move the newest one Up instead of Down."); }
                            //ImGui.InputFloat("MinH", ref minH);
                            //ImGui.InputFloat("MaxH", ref maxH);
                            ImGui.Checkbox("Show Channel", ref bubblesChannel); ImGui.NextColumn();
                            if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Show Channel in the bubble."); }
                            ImGui.PushItemWidth(80);
                            ImGui.InputInt("Duration", ref bubbleTime);
                            ImGui.PopItemWidth(); ImGui.NextColumn();
                            if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Seconds the bubbles exist for."); }
                            //ImGui.InputInt("X Disp", ref xDisp);
                            //ImGui.InputInt("Y Disp", ref yDisp);
                            //ImGui.InputInt("X Cut", ref xCut);
                            //ImGui.InputInt("Y Cut", ref yCut);
                            //ImGui.ColorEdit4("Bubble Colour", ref bubbleColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel);
                            //ImGui.InputFloat("Rounding", ref bubbleRounding);
                            ImGui.Separator();
                            ImGui.Text("Channel"); ImGui.NextColumn();
                            ImGui.Text("Enabled"); ImGui.NextColumn();
                            ImGui.Text("Colour"); ImGui.NextColumn();
                            for (int i = 0; i < (Channels.Length); i++)
                            {
                                ImGui.Text(Channels[i]); ImGui.SameLine(); ImGui.NextColumn();
                                ImGui.Checkbox("##" + Channels[i], ref bubbleEnable[i]); ImGui.NextColumn();
                                ImGui.ColorEdit4(Channels[i] + " ColourBubble", ref bubbleColour[i], ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel); ImGui.NextColumn();
                            }
                            ImGui.Columns(1);
                            ImGui.EndTabItem();
                        }
                    }
                    
                }

                ImGui.EndTabBar();
                ImGui.EndChild();

                if (ImGui.Button("Save and Close Config"))
                {
                    SaveConfig();

                    configWindow = false;
                }
                if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Changes will only be saved for the current session unless you do this!"); }
                ImGui.End();
            }
        }

        private void ChatBubbles()
        {
            if (!skipfont)
            {
                ImGui.PushFont(font);
                if (bubblesWindow)
                {

                    Dalamud.Game.ClientState.Actors.ActorTable actorTable = pluginInterface.ClientState.Actors;
                    List<Dalamud.Game.ClientState.Actors.Types.Chara> charaTable = new List<Dalamud.Game.ClientState.Actors.Types.Chara>();


                    for (var k = 0; k < this.pluginInterface.ClientState.Actors.Length; k++)
                    {
                        var actor = this.pluginInterface.ClientState.Actors[k];

                        if (actor == null)
                            continue;

                        if (actor is Dalamud.Game.ClientState.Actors.Types.NonPlayer.Npc npc)
                        {

                        }

                        if (actor is Dalamud.Game.ClientState.Actors.Types.Chara chara)
                        {

                            if (drawDebug)
                            {
                                if (pluginInterface.Framework.Gui.WorldToScreen(new SharpDX.Vector3(actor.Position.X, actor.Position.Z + AddHeight(chara), actor.Position.Y), out SharpDX.Vector2 pos2))
                                {

                                    ImGui.SetNextWindowPos(new Num.Vector2(pos2.X + 30, pos2.Y));
                                    ImGui.Begin(chara.Name + "Info", ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysAutoResize);
                                    ImGui.Text(chara.Name);
                                    //ImGui.Text("G: " + chara.Customize[(int)Dalamud.Game.ClientState.Actors.CustomizeIndex.Gender].ToString());
                                    //ImGui.Text("R: " + chara.Customize[(int)Dalamud.Game.ClientState.Actors.CustomizeIndex.Race].ToString());
                                    //ImGui.Text("H: " + chara.Customize[(int)Dalamud.Game.ClientState.Actors.CustomizeIndex.Height].ToString());
                                    //ImGui.Text("A: " + AddHeight(chara).ToString());
                                    //ImGui.Text(k.ToString());
                                    ImGui.Text(actor.ActorId.ToString());
                                    ImGui.Text(actor.Address.ToString("X"));
                                    ImGui.End();
                                }

                                ImGui.Begin("XXX", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysAutoResize);
                                ImGui.Text(pluginInterface.ClientState.LocalPlayer.Address.ToString("X"));
                                ImGui.End();
                            }


                        }

                        if (actor is Dalamud.Game.ClientState.Actors.Types.PlayerCharacter pc)
                        {
                            charaTable.Add(pc);
                        }

                    }


                    foreach (Dalamud.Game.ClientState.Actors.Types.PlayerCharacter actor in charaTable)
                    {
                        try
                        {
                            foreach (ChatText chat in chatBubble)
                            {

                                if (chat.Sender == actor.Name && chat.Sender.Length > 0)
                                {
                                    DrawChatBubble(actor, chat);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            //Do nothing
                        }
                        CleanupBubbles();
                    }

                    //ImGui.End();
                }
                ImGui.PopFont();
            }
            else skipfont = false;
        }
    }
}
