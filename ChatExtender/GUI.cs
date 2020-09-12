using Dalamud.Game.Chat.SeStringHandling;
using Dalamud.Game.Chat.SeStringHandling.Payloads;
using Dalamud.Plugin;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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
                if (getUI2ObjByName(Marshal.ReadIntPtr(getBaseUIObj(), 0x20), "ChatLog", 1) == IntPtr.Zero)
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

            RenderUI();
        }

        private void RenderUI()
        {
            ImGuiWindowFlags chat_window_flags = 0;
            ImGuiWindowFlags chat_sub_window_flags = 0;

            if (no_titlebar) chat_window_flags |= ImGuiWindowFlags.NoTitleBar;
            if (no_collapse) chat_window_flags |= ImGuiWindowFlags.NoCollapse;
            if (!no_menu) chat_window_flags |= ImGuiWindowFlags.MenuBar;
            if (no_nav) chat_window_flags |= ImGuiWindowFlags.NoNav;

            if (config.NoScrollBar) chat_window_flags |= ImGuiWindowFlags.NoScrollbar;
            if (config.NoScrollBar) chat_sub_window_flags |= ImGuiWindowFlags.NoScrollbar;
            if (config.NoMove) chat_window_flags |= ImGuiWindowFlags.NoMove;
            if (config.NoResize) chat_window_flags |= ImGuiWindowFlags.NoResize;
            if (config.NoMouse) { chat_window_flags |= ImGuiWindowFlags.NoMouseInputs; }
            if (config.NoMouse2) { chat_sub_window_flags |= ImGuiWindowFlags.NoMouseInputs; }

            if (fontsLoaded)
            {
                if (hideWithChat && Alpha != 0)
                {
                    if (config.ShowChatWindow)
                    {
                        if (flickback)
                        {
                            config.NoMouse = false;
                            flickback = false;
                        }
                        ImGui.SetNextWindowSize(new Vector2(200, 100), ImGuiCond.FirstUseEver);
                        ImGui.SetNextWindowBgAlpha(config.Alpha);
                        ImGui.Begin("Another Window", ref config.ShowChatWindow, chat_window_flags);
                        ImGuiTabBarFlags tab_bar_flags = ImGuiTabBarFlags.None;

                        if (ImGui.BeginTabBar("Tabs", tab_bar_flags))
                        {
                            foreach (var tab in tabs)
                            {
                                if (ImGui.BeginTabItem(tab.Title))
                                {
                                    if (tab != activeTab)
                                    {
                                        activeTab = tab;
                                    }

                                    var prevWindowSize = windowSize;
                                    windowSize = ImGui.GetContentRegionMax();

                                    if (windowSize != prevWindowSize)
                                    {
                                        tab.needsRecomputeCumulativeLengths = true;
                                    }

                                    ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));

                                    RenderChatArea(tab, chat_sub_window_flags);

                                    ImGui.PopStyleVar();

                                    if (config.NoMouse2 && !config.NoMouse)
                                    {
                                        Vector2 vMin = ImGui.GetWindowContentRegionMin();
                                        Vector2 vMax = ImGui.GetWindowContentRegionMax();

                                        vMin.X += ImGui.GetWindowPos().X;
                                        vMin.Y += ImGui.GetWindowPos().Y + 22;
                                        vMax.X += ImGui.GetWindowPos().X - 22;
                                        vMax.Y += ImGui.GetWindowPos().Y;

                                        if (ImGui.IsMouseHoveringRect(vMin, vMax)) { config.NoMouse = true; flickback = true; }
                                    }
                                    tab.msg = false;
                                    ImGui.EndTabItem();
                                }
                            }
                            ImGui.EndTabBar();
                            ImGui.End();
                        }
                    }
                }
            }

            RenderConfigWindow();
        }

        private void RenderChatArea(TabBase tab, ImGuiWindowFlags chat_sub_window_flags)
        {
            float footer = (ImGui.GetStyle().ItemSpacing.Y) / 2 + ImGui.GetFrameHeightWithSpacing();
            if (!tab.FilterOn)
            {
                footer = 0;
            }

            ImGui.BeginChild("scrolling", new Vector2(0, -footer), false, chat_sub_window_flags);

            tab.ComputeNewLineSum();

            var lines = tab.FilteredLogs;
            var cumulativeSum = tab.GetCumulativeLineSum();
            var lineHeight = ImGui.CalcTextSize("M").Y;
            var linesInWindow = (int)(ImGui.GetContentRegionAvail().Y / lineHeight);

            var totalLines = cumulativeSum.LastOrDefault() - linesInWindow; // offset to adjust scrolling area height

            var totalLineHeight = totalLines * lineHeight;
            var scrollPercent = ImGui.GetScrollMaxY() == 0 ? 0 : ImGui.GetScrollY() / ImGui.GetScrollMaxY();
            var lineStartIndex = (int)(Math.Max(0, (totalLines - linesInWindow)) * scrollPercent);


            var initialY = ImGui.GetCursorPosY();
            var initialX = ImGui.GetCursorPosX();
            ImGui.SetCursorPosY(totalLineHeight);
            ImGui.SetCursorPosY(initialY);

            var startIndex = BinaryFind(cumulativeSum, lineStartIndex, 0, cumulativeSum.Count());

            startIndex = Math.Max(0, startIndex - linesInWindow);

            var startY = initialY + (startIndex == 0 ? 0 : cumulativeSum[startIndex - 1]) * lineHeight;
            ImGui.SetCursorPosY(startY);

            for (int i = startIndex; i < startIndex + 3 * linesInWindow && i < lines.Count(); i++)
            {
                var textLogEntry = lines[i];
                var textLines = textLogEntry.GetLines();
                var currentY = ImGui.GetCursorPosY();
                
                if (textLogEntry.line.Sender != null)
                {
                    textLogEntry.line.Sender.IsHovered = false;
                }
                foreach (var text in textLogEntry.Text)
                {
                    text.IsHovered = false;
                }

                // Pre-pass for highlight boxes
                ImGui.PushFont(outlineFont);
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0, 0, 0, 0));
                foreach (var line in textLines)
                {
                    foreach (var str in line)
                    {
                        ImGui.Text(str.Text);
                        ImGui.SameLine();

                        if (str.SourcePayloadContainer != null)
                        {
                            str.SourcePayloadContainer.IsHovered |= ImGui.IsItemHovered(ImGuiHoveredFlags.RectOnly);
                        }
                    }
                    ImGui.SetCursorPosX(initialX);
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + lineHeight);
                }

                ImGui.PopStyleColor();
                ImGui.PopFont();

                ImGui.SetCursorPosY(currentY);
                var buttonId = 0;
                foreach (var line in textLines)
                {
                    ImGui.PushFont(outlineFont);
                    ImGui.SetCursorPosX(initialX);
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0, 0, 0, 0));

                    // Draw highlight boxes
                    foreach (var str in line)
                    {
                        var cursorPos = ImGui.GetCursorPos();
                        var textSize = ImGui.CalcTextSize(str.Text);

                        if (str.SourcePayloadContainer != null && str.SourcePayloadContainer.IsHovered)
                        {
                            ImGui.PushStyleColor(ImGuiCol.Button, str.SourcePayloadContainer.highlightColor);
                            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, str.SourcePayloadContainer.highlightColor);
                            ImGui.PushStyleColor(ImGuiCol.ButtonActive, str.SourcePayloadContainer.highlightColor);
                            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0);
                            if (ImGui.Button($"##{buttonId++}", textSize))
                            {
                                Task.Run(() =>
                                {
                                    ProcessLinkClick(str.SourcePayloadContainer.Payload);
                                });
                            }
                            ImGui.PopStyleVar();
                            ImGui.PopStyleColor();
                            ImGui.PopStyleColor();
                            ImGui.PopStyleColor();
                        }

                        ImGui.SetCursorPos(cursorPos);
                        ImGui.Text(str.Text);
                        ImGui.SameLine();
                    }
                    ImGui.PopStyleColor();
                
                    // Draw outline
                    ImGui.SetCursorPosX(initialX);
                    foreach (var str in line)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, str.ReferenceShadowColor?.Color ?? str.ShadowColor);
                        ImGui.Text(str.Text);
                        ImGui.PopStyleColor();
                        ImGui.SameLine();
                    }
                    ImGui.PopFont();

                    ImGui.SetCursorPosX(initialX);
                    ImGui.PushFont(font);
                    foreach (var str in line)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, str.ReferenceColor?.Color ?? str.Color);
                        ImGui.Text(str.Text);
                        ImGui.PopStyleColor();
                        ImGui.SameLine();
                    }
                    ImGui.PopFont();
                    ImGui.SetCursorPosX(initialX);
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + lineHeight);
                }
            }

            if (tab.ScrollOnce == true)
            {
                ImGui.SetScrollHereY();
                tab.ScrollOnce = false;
            }

            ImGui.EndChild();

            if (tab.FilterOn)
            {
                if (ImGui.InputText("Filter Text", ref tab.Filter, 999))
                {
                    tab.UpdateFilteredLines();
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Only show lines with this text.");
                }
            }
        }

        private int BinaryFind(List<int> numbers, int number, int start, int end)
        {
            if (start >= end - 1)
            {
                return start;
            }

            int half = (start + end) / 2;

            if (number == numbers[half])
            {
                return half;
            }
            else if (number <= numbers[half])
            {
                return BinaryFind(numbers, number, start, half);
            }
            else
            {
                return BinaryFind(numbers, number, half, end);
            }
        }

        private void RenderConfigWindow()
        {
            if (configWindow)
            {
                ImGui.SetNextWindowSize(new Vector2(300, 500), ImGuiCond.FirstUseEver);
                ImGui.Begin("Chat Config", ref configWindow);
                ImGuiTabBarFlags tab_bar_flags = ImGuiTabBarFlags.None;

                float footer = (ImGui.GetStyle().ItemSpacing.Y) / 2 + ImGui.GetFrameHeightWithSpacing();

                if (ImGui.BeginTabBar("Tabs", tab_bar_flags))
                {

                    if (ImGui.BeginTabItem("Config"))
                    {
                        ImGui.BeginChild("scrolling", new Vector2(0, -footer), false);

                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 10);

                        ImGui.Columns(3);

                        ImGui.Checkbox("Show Chat Extender", ref config.ShowChatWindow);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Enable/Disable the Chat Extender"); }
                        ImGui.NextColumn();
                        ImGui.Checkbox("24 Hour Time", ref config.HourTime);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Switch to 24 Hour (Military) time."); }
                        ImGui.NextColumn();
                        ImGui.Checkbox("Hide Scrollbar", ref config.NoScrollBar);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Shows ScrollBar"); }
                        ImGui.NextColumn();

                        ImGui.Checkbox("Lock Window Position", ref config.NoMove);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Lock/Unlock the position of the Chat Extender"); }
                        ImGui.NextColumn();
                        ImGui.Checkbox("Lock Window Size", ref config.NoResize);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Lock/Unlock the size of the Chat Extender"); }
                        ImGui.NextColumn();
                        ImGui.NextColumn();
                        ImGui.Checkbox("ClickThrough Tab Bar", ref config.NoMouse);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Enable/Disable being able to clickthrough the Tab Bar of the Chat Extender"); }
                        ImGui.NextColumn();
                        ImGui.Checkbox("ClickThrough Chat", ref config.NoMouse2);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Enable/Disable being able to clickthrough the Chat Extension chatbox"); }


                        ImGui.Columns(1);

                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 30);
                        ImGui.SliderFloat("Chat Extender Alpha", ref config.Alpha, 0.001f, 0.999f);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Alter the Alpha of the Chat Extender"); }

                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 50);
                        ImGui.Checkbox("Debug", ref debug);
                        if (debug)
                        {
                            ImGui.Checkbox("Output Error Json", ref outputErrorJsons);
                            ImGui.Checkbox("Output All Json", ref outputAllJsons);
                            ImGui.Text($"Lines in chat buffer: {chatBuffer.Count()}");
                            ImGui.Text($"Lines in current filter: {activeTab.FilteredLogs.Count()}");
                        }

                        ImGui.EndChild();
                        ImGui.EndTabItem();
                    }
                    
                    if (ImGui.BeginTabItem("Channels"))
                    {
                        ImGui.BeginChild("scrolling", new Vector2(0, -footer), false);
                        ImGui.Columns(4);
                        ImGui.Text("Setting"); ImGui.NextColumn();
                        ImGui.Text("Example"); ImGui.NextColumn();
                        ImGui.Text("Color 1"); ImGui.NextColumn();
                        ImGui.Text("Color 2"); ImGui.NextColumn();

                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 10); ImGui.NextColumn();
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 10); ImGui.NextColumn();
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 10); ImGui.NextColumn();
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 10); ImGui.NextColumn();

                        ImGui.Text("Time"); ImGui.NextColumn();

                        var cursorPos = ImGui.GetCursorPos();
                        ImGui.PushFont(outlineFont);
                        ImGui.TextColored(config.TimeShadowColor, "[12:00]");
                        ImGui.PopFont();

                        ImGui.SetCursorPos(cursorPos);
                        ImGui.PushFont(font);
                        ImGui.TextColored(config.TimeColor, "[12:00]");
                        ImGui.PopFont();

                        ImGui.NextColumn();


                        ImGui.ColorEdit4("Time Color", ref config.TimeColorRef.Color, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel); ImGui.NextColumn();

                        ImGui.ColorEdit4("Time Outline Color", ref config.TimeColorShadowRef.Color, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel); ImGui.NextColumn();

                        foreach (var key in ChannelSettingsTable.Keys)
                        {
                            var channelSettings = ChannelSettingsTable[key];
                            ImGui.PushItemWidth(50);
                            ImGui.InputText(channelSettings.Name, ref channelSettings.ShortName, 8); ImGui.NextColumn();
                            ImGui.PopItemWidth();
                            
                            cursorPos = ImGui.GetCursorPos();
                            ImGui.PushFont(outlineFont);
                            ImGui.TextColored(channelSettings.OutlineColor, "[" + channelSettings.Name + "]");
                            ImGui.PopFont();
                            ImGui.SetCursorPos(cursorPos);

                            ImGui.PushFont(font);
                            ImGui.TextColored(channelSettings.FontColor, "[" + channelSettings.Name + "]"); ImGui.NextColumn();
                            ImGui.PopFont();

                            ImGui.ColorEdit4(channelSettings.Name + " Font Color", ref channelSettings.FontColorRef.Color, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel); ImGui.NextColumn();
                            ImGui.ColorEdit4(channelSettings.Name + " Outline Color", ref channelSettings.OutlineColorRef.Color, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel); ImGui.NextColumn();
                        }

                        ImGui.Columns(1);
                        ImGui.EndChild();
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Tabs"))
                    {
                        ImGui.BeginChild("scrolling", new Vector2(0, -footer), false);
                        if (ImGui.Button("Add New Tab"))
                        {
                            int i = 1;
                            while (tabs.Any(x => x.Title == $"New Tab ({i})"))
                            {
                                i++;
                            }

                            tabs.Add(new TabBase
                            {
                                Title = $"New Tab ({i})",
                                Enabled = true
                            });
                            tempTitle = "Title";
                        }
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Add a new Tab to the Chat Extender"); }

                        if (ImGui.TreeNode("Tab Order"))
                        {

                            ImGui.Columns(3);
                            ImGui.Text("Tab"); ImGui.NextColumn();
                            ImGui.Text(""); ImGui.NextColumn();
                            ImGui.Text(""); ImGui.NextColumn();

                            for (int i = 0; i < tabs.Count; i++)
                            {
                                ImGui.Text(tabs[i].Title); ImGui.NextColumn();
                                if (i > 0)
                                {
                                    if (ImGui.Button("^##" + i.ToString()))
                                    {
                                        TabBase temp = tabs[i];
                                        tabs.RemoveAt(i);
                                        tabs.Insert(i - 1, temp);
                                    }
                                }
                                ImGui.NextColumn();
                                if (i < tabs.Count - 1)
                                {
                                    if (ImGui.Button("v##" + i.ToString()))
                                    {
                                        TabBase temp = tabs[i];
                                        tabs.RemoveAt(i);
                                        tabs.Insert(i + 1, temp);
                                    }
                                }
                                ImGui.NextColumn();
                            }
                            ImGui.Columns(1);
                            ImGui.TreePop();
                        }

                        ImGui.Separator();
                        foreach (var tab in tabs)
                        {
                            if (ImGui.TreeNode(tab.Title))
                            {
                                float footer2 = (ImGui.GetStyle().ItemSpacing.Y) / 2 + ImGui.GetFrameHeightWithSpacing();
                                ImGui.BeginChild("scrolling", new Vector2(0, -footer2), false);
                                ImGui.InputText("##Tab Name", ref tempTitle, bufSize);
                                ImGui.SameLine();
                                if (ImGui.Button("Set Tab Title"))
                                {
                                    if (tempTitle.Length == 0) {
                                        tempTitle += ".";
                                    }

                                    while (CheckDupe(tabs, tempTitle)) {
                                        tempTitle += ".";
                                    }

                                    tab.Title = tempTitle;
                                    tempTitle = "Title";
                                }
                                if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Change the title of the Tab"); }

                                ImGui.Columns(4);

                                ImGui.Checkbox("Time Stamp", ref tab.Timestamps);
                                if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Show Timestamps in this Tab"); }
                                ImGui.NextColumn();
                                ImGui.Checkbox("Channel", ref tab.ShowChannelTagAll);
                                if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Show the Channel the message came from"); }
                                ImGui.NextColumn();
                                ImGui.Checkbox("AutoScroll", ref tab.AutoScroll);
                                if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Enable the Chat to scroll automatically on a new message"); }
                                ImGui.NextColumn();
                                if (ImGui.Checkbox("Enable Filter", ref tab.FilterOn))
                                {
                                    tab.UpdateFilteredLines();
                                }
                                if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Enable Filtering of text"); }
                                ImGui.NextColumn();
                                ImGui.Columns(1);


                                //TODO: Add a confirm prompt

                                if (tabs.Count > 1)
                                {
                                    if (ImGui.Button("Delete Tab"))
                                    {
                                        tab.Enabled = false;
                                        tabs = tabs.Where(x => x.Enabled).ToList();
                                        if (!activeTab.Enabled)
                                        {
                                            activeTab = tabs[0];
                                        }
                                    }
                                }
                                if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Removes Tab"); }


                                ImGui.Columns(2);
                                ImGui.Text("Channel"); ImGui.NextColumn();
                                if (tab.ShowChannelTagAll)
                                {
                                    ImGui.Text("Show Short");
                                }
                                else
                                {
                                    ImGui.Text("");
                                }
                                ImGui.NextColumn();

                                foreach(var channelId in ChannelSettingsTable.Keys)
                                {
                                    var channelSettings = ChannelSettingsTable[channelId];
                                    var channelName = channelSettings.Name;

                                    ImGui.PushStyleColor(ImGuiCol.Text, channelSettings.FontColor);

                                    if (ImGui.Checkbox("[" + channelSettings.Name + "]", ref tab.EnabledChannels[channelName].Value) && tab == activeTab)
                                    {
                                        tab.UpdateFilteredLines();
                                    }
                                    ImGui.NextColumn();

                                    if (tab.ShowChannelTagAll)
                                    {
                                        if (ImGui.Checkbox(channelSettings.ShortName, ref tab.ShowChannelTag[channelSettings.Name].Value))
                                        {
                                            tab.needsRecomputeCumulativeLengths = true;
                                        }
                                    }
                                    else
                                    {
                                        ImGui.Text("");
                                    }
                                    ImGui.NextColumn();

                                    ImGui.PopStyleColor();
                                }

                                ImGui.Columns(1);
                                ImGui.EndChild();
                                ImGui.TreePop();
                            }
                        }
                        ImGui.EndChild();
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Font"))
                    {
                        ImGui.BeginChild("scrolling", new Vector2(0, -footer), false);
                        ImGui.Columns(1);
                        ImGui.PushItemWidth(124);
                        ImGui.InputFloat("Font Size", ref config.FontSize); ImGui.SameLine();
                        ImGui.PopItemWidth();
                        if (ImGui.SmallButton("Apply"))
                        {
                            UpdateFonts();
                        }

                        ImGui.EndChild();
                        ImGui.EndTabItem();
                    }

                    if (debug)
                    {
                        if (ImGui.BeginTabItem("Style Editor"))
                        {
                            ImGui.BeginChild("scrolling", new Vector2(0, -footer), false);
                            ImGui.ShowStyleEditor();
                            ImGui.EndChild();
                            ImGui.EndTabItem();
                        }
                    }
                }

                ImGui.EndTabBar();

                if (ImGui.Button("Save and Close Config"))
                {
                    SaveConfig();

                    configWindow = false;
                }
                if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Changes will only be saved for the current session unless you do this!"); }
                ImGui.End();
            }
        }

        private void ProcessLinkClick(Payload sourcePayload)
        {
            switch (sourcePayload.Type)
            {
                case PayloadType.MapLink:
                    var mapPayload = (MapLinkPayload)sourcePayload;
                    pluginInterface.Framework.Gui.OpenMapWithMapLink(mapPayload);
                    break;
                default:
                    break;
            }
        }
    }
}
