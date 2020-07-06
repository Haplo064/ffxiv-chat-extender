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
using System.Runtime.InteropServices;
using Dalamud.Configuration;
using Num = System.Numerics;
using System.Reflection;
using System.Collections.Concurrent;

namespace DalamudPlugin
{
    public partial class ChatExtenderPlugin : IDalamudPlugin
    {
        private unsafe void AddFont()
        {
            string fontFile = "XIVfree.ttf";
            string fontPath = Path.Combine(dllPath, fontFile);

            ImFontConfigPtr fontConfig = ImGuiNative.ImFontConfig_ImFontConfig();
            fontConfig.MergeMode = true;
            fontConfig.PixelSnapH = false;

            var gameRangeHandle = GCHandle.Alloc(new ushort[]
            {
                0xE016,
                0xf739,
                0
            }, GCHandleType.Pinned);

            var gameRangeHandle2 = GCHandle.Alloc(new ushort[]
            {
                0x2013,
                0x303D,
                0
            }, GCHandleType.Pinned);

            var fontPathJp = Path.Combine( Directory.GetParent(Directory.GetParent(Directory.GetParent(dllPath).ToString()).ToString()).ToString(), "addon", "Hooks", "UIRes", "NotoSansCJKjp-Medium.otf");
            
            font = ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPath, (float)fontsize, fontConfig);
            ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPath, (float)fontsize, fontConfig, gameRangeHandle.AddrOfPinnedObject());
            ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPath, (float)fontsize, fontConfig, gameRangeHandle2.AddrOfPinnedObject());
            ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPathJp, (float)fontsize, fontConfig, ImGui.GetIO().Fonts.GetGlyphRangesJapanese());


            fontConfig.Destroy();
            gameRangeHandle.Free();
            gameRangeHandle2.Free();
        }

        private unsafe void UpdateFont()
        {
            string fontFile = "XIVfree.ttf";
            string fontPath = Path.Combine(dllPath, fontFile);

            ImFontConfigPtr fontConfig = ImGuiNative.ImFontConfig_ImFontConfig();
            fontConfig.MergeMode = true;
            fontConfig.PixelSnapH = false;

            var gameRangeHandle = GCHandle.Alloc(new ushort[]
            {
                0xE016,
                0xf739,
                0
            }, GCHandleType.Pinned);

            var gameRangeHandle2 = GCHandle.Alloc(new ushort[]
            {
                0x2013,
                0x303D,
                0
            }, GCHandleType.Pinned);

            var fontPathJp = Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(dllPath).ToString()).ToString()).ToString(), "addon", "Hooks", "UIRes", "NotoSansCJKjp-Medium.otf");

            font = ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPath, (float)fontsize, fontConfig);
            ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPath, (float)fontsize, fontConfig, gameRangeHandle.AddrOfPinnedObject());
            ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPath, (float)fontsize, fontConfig, gameRangeHandle2.AddrOfPinnedObject());
            ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPathJp, (float)fontsize, fontConfig, ImGui.GetIO().Fonts.GetGlyphRangesJapanese());


            fontConfig.Destroy();
            gameRangeHandle.Free();
            gameRangeHandle2.Free();

            pluginInterface.UiBuilder.RebuildFonts();
            skipfont=true;
        }
        

        private void Chat_ConfigWindow(object Sender, EventArgs args)
        {
            configWindow = true;
        }

        private List<TabBase> CopyAndStripItems(List<TabBase> items)
        {
            List<TabBase> clone = new List<TabBase>();
            foreach (TabBase tabs in items)
            {
                if (tabs.Enabled)
                {
                    TabBase babyClone = new TabBase();
                    babyClone.AutoScroll = tabs.AutoScroll;
                    babyClone.Chat = new ConcurrentQueue<ChatText>();
                    babyClone.Config = tabs.Config.ToArray();
                    babyClone.Enabled = tabs.Enabled;
                    babyClone.Logs = tabs.Logs.ToArray();
                    babyClone.Scroll = tabs.Scroll;
                    babyClone.Title = tabs.Title.ToString();
                    babyClone.Filter = tabs.Filter.ToString();
                    babyClone.FilterOn = tabs.FilterOn;
                    clone.Add(babyClone);
                }
            }
            return clone;
        }

        private List<TabBase> CopyItems(List<TabBase> items)
        {
            List<TabBase> clone = new List<TabBase>();
            foreach (TabBase tabs in items)
            {
                if (tabs.Enabled)
                {
                    TabBase babyClone = new TabBase();
                    babyClone.AutoScroll = tabs.AutoScroll;
                    babyClone.Chat = tabs.Chat;
                    babyClone.Config = tabs.Config.ToArray();
                    babyClone.Enabled = tabs.Enabled;
                    babyClone.Logs = tabs.Logs.ToArray();
                    babyClone.Scroll = tabs.Scroll;
                    babyClone.Title = tabs.Title.ToString();
                    babyClone.Filter = tabs.Filter.ToString();
                    babyClone.FilterOn = tabs.FilterOn;
                    clone.Add(babyClone);
                }
            }
            return clone;
        }

        public uint UintCol(int A, int B, int G, int R)
        {
            //PluginLog.Log();
            return Convert.ToUInt32("0x" + A.ToString("X2") + B.ToString("X2") + G.ToString("X2") + R.ToString("X2"), 16);
            //return UInt32.Parse("0x" + R.ToString("X2") + G.ToString("X2") + B.ToString("X2") + A.ToString("X2"));
        }

        public bool ContainsText(List<TextTypes> text, string find)
        {
            String concat = "";
            foreach (TextTypes texts in text)
            {
                concat += texts.Text + " ";
            }

            return concat.Contains(find);
        }



        public void SaveConfig()
        {
            Configuration.Items = CopyAndStripItems(items);
            Configuration.Inject = injectChat;
            Configuration.Extender = chatWindow;
            Configuration.Alpha = alpha;
            Configuration.ChanColour = chanColour.ToArray();
            Configuration.LogColour = logColour.ToArray();
            Configuration.RTr = rTr.ToString();
            Configuration.LTr = lTr.ToString();
            Configuration.NoMouse = no_mouse;
            Configuration.NoMouse2 = no_mouse2;
            Configuration.NoMove = no_move;
            Configuration.NoResize = no_resize;
            Configuration.Space_Hor = space_hor;
            Configuration.Space_Ver = space_ver;
            Configuration.TimeColour = timeColour;
            Configuration.NameColour = nameColour;
            Configuration.High = high;
            Configuration.Chan = Chan.ToArray();
            Configuration.YandexKey = yandex.ToString();
            Configuration.Translator = translator;
            Configuration.AllowTranslation = allowTranslation;
            Configuration.BubbleColour = bubbleColour.ToArray();
            Configuration.BubbleEnable = bubbleEnable.ToArray();
            Configuration.BubblesWindow = bubblesWindow;
            Configuration.FontSize = fontsize;
            Configuration.HourTime = hourTime;
            Configuration.FontShadow = fontShadow;
            Configuration.BubbleTime = bubbleTime;
            this.pluginInterface.SavePluginConfig(Configuration);
        }

        public void HighlightText()
        {
            ImGui.GetWindowDrawList().AddRectFilled(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), UintCol(high.htA, high.htB, high.htG, high.htR), 2.0f);
        }

        public void ShadowFont(string input)
        {

            var cur_pos = ImGui.GetCursorPos();
            ImGui.PushStyleColor(ImGuiCol.Text, UintCol(255, 0, 0, 0));
            ImGui.SetCursorPos(new Num.Vector2(cur_pos.X - 1, cur_pos.Y - 1)); ImGui.Text(input);
            ImGui.SetCursorPos(new Num.Vector2(cur_pos.X - 1, cur_pos.Y + 1)); ImGui.Text(input);
            ImGui.SetCursorPos(new Num.Vector2(cur_pos.X + 1, cur_pos.Y + 1)); ImGui.Text(input);
            ImGui.SetCursorPos(new Num.Vector2(cur_pos.X + 1, cur_pos.Y - 1)); ImGui.Text(input);
            ImGui.PopStyleColor();
            ImGui.SetCursorPos(cur_pos);

        }

        public void Wrap(String input)
        {
            input = input.Replace("\n", " XXX ");

            String[] inputArray = input.Split(' ');

            int count = 0;
            foreach (String splits in inputArray)
            {
                bool newline = false;
                if (ImGui.GetContentRegionAvail().X - 5 - ImGui.CalcTextSize(splits).X < 0)
                { ImGui.Text(""); }

                if (splits == "XXX")
                {   
                    newline = true;
                }
                else
                {
                    if (fontShadow)
                    {
                        var cur_pos = ImGui.GetCursorPos();
                        ImGui.PushStyleColor(ImGuiCol.Text, UintCol(255, 0, 0, 0));
                        ImGui.SetCursorPos(new Num.Vector2(cur_pos.X - 1, cur_pos.Y - 1)); ImGui.Text(splits.Trim());
                        ImGui.SetCursorPos(new Num.Vector2(cur_pos.X - 1, cur_pos.Y + 1)); ImGui.Text(splits.Trim());
                        ImGui.SetCursorPos(new Num.Vector2(cur_pos.X + 1, cur_pos.Y + 1)); ImGui.Text(splits.Trim());
                        ImGui.SetCursorPos(new Num.Vector2(cur_pos.X + 1, cur_pos.Y - 1)); ImGui.Text(splits.Trim());
                        ImGui.PopStyleColor();
                        ImGui.SetCursorPos(cur_pos);
                    }
                    ImGui.Text(splits.Trim());
                    
                }

                foreach (String word in high.highlights)
                {
                    if (StripPunctuation(splits.ToLower()) == StripPunctuation(word.ToLower())) HighlightText();
                }

                if (count < (inputArray.Length - 1))
                {
                    if (!newline)
                    { ImGui.SameLine(); }
                    count++;
                }
            }
        }

        public static string StripPunctuation(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s)
            {
                if (!char.IsPunctuation(c))
                    sb.Append(c);
            }
            return sb.ToString();
        }


        public int EnabledTabs(List<TabBase> countMe)
        {
            int count = 0;

            foreach (var tab in countMe)
            {
                if (tab.Enabled) { count++; }
            }
            return count;
        }

        public int ConvertForArray(string type)
        {
            try
            {
                int value = Array.IndexOf(Channels, type);
                if (value >= 0) { return Array.IndexOf(Channels, type); }
                else
                {
                    //PluginLog.Log(type);
                    type = AddOnChannels(Int32.Parse(type.ToString()) & 127);
                    value = Array.IndexOf(Channels, type);
                    if (value >= 0) { return Array.IndexOf(Channels, type); }
                    else { return Int32.Parse(type.ToString()) & 127; }
                }
            }
            catch (Exception e)
            {
                //PluginLog.Log(e.ToString());
                return 0;
            }
        }

        public string AddOnChannels(int channel)
        {
            if (channel == 57) return "SystemMessage";
            if (channel == 43) return "Actions";
            if (channel == 41) return "Damage";
            if (channel == 42) return "FailedAttacks";
            if (channel == 44) return "ItemsUsed";
            if (channel == 45) return "Healing";
            if (channel == 46) return "BenefictsStart";
            if (channel == 47) return "DetrimentsStart";
            if (channel == 48) return "BenefictsEnd";
            if (channel == 49) return "DetrimentsEnd";
            if (channel == 55) return "Alarms";
            if (channel == 58) return "BattleSystemMessage";
            if (channel == 61) return "NPC";
            if (channel == 62) return "Loot";
            if (channel == 64) return "Progression";
            if (channel == 65) return "LootRolls";
            if (channel == 66) return "Synthesis";
            if (channel == 68) return "NPCAnnouncement";
            if (channel == 69) return "FCAnnouncement";
            if (channel == 70) return "FCLogin";
            if (channel == 72) return "RecruitmentNotice";
            if (channel == 73) return "SignMarking";
            if (channel == 74) return "Randoms";
            if (channel == 76) return "OrchestronTrack";
            if (channel == 77) return "PVPTeamAnnouncement";
            if (channel == 78) return "PVPTeamLogin";
            if (channel == 79) return "MessageBookAlert";
            else return channel.ToString();
        }

        public string GetChannelName(string type)
        {
            try
            { return Chan[ConvertForArray(type)]; }
            catch (Exception)
            { return type; }
        }

        private void Chat_OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            try
            {
                if (!isHandled)
                {
                    var senderName = sender.TextValue;
                    List<Payload> payloads = message.Payloads;
                    int chan = ConvertForArray(type.ToString());
                    ChatText tmp = new ChatText();

                    tmp.Time = GetTime();
                    tmp.DateTime = DateTime.Now;
                    tmp.ChannelShort = GetChannelName(type.ToString());
                    tmp.SenderId = senderId;

                    //PluginLog.Log(senderId.ToString());
                    //PluginLog.Log(senderName);

                    try
                    {
                        tmp.Channel = Channels[chan].Trim().Replace(" ", "");
                    }
                    catch (Exception)
                    {
                        tmp.Channel = chan.ToString();
                    }

                    tmp.Sender = senderName;
                    tmp.ChannelColour = ConvertForArray(type.ToString());

                    List<TextTypes> rawtext = new List<TextTypes>();

                    int replace = 0;
                    Payload payloader = null;
                    PayloadType payloadType = PayloadType.RawText;

                    //Handling Emotes
                    if (tmp.Channel == "StandardEmote")
                    {
                        tmp.Sender = "";
                    }

                    if (tmp.Channel == "CustomEmote")
                    {
                        tmp.Sender = "";
                        TextTypes wrangle = new TextTypes();
                        wrangle.Type = PayloadType.RawText;
                        wrangle.Text = senderName;
                        rawtext.Add(wrangle);
                    }
                    //Handling Tells
                    if (tmp.Channel == "TellOutgoing")
                    {
                        TextTypes wrangle = new TextTypes();
                        wrangle.Type = PayloadType.RawText;
                        wrangle.Text = ">>" + tmp.Sender + ":";
                        rawtext.Add(wrangle);
                        tmp.Sender = pluginInterface.ClientState.LocalPlayer.Name.ToString();

                    }
                    if (tmp.Channel == "TellIncoming")
                    {
                        TextTypes wrangle = new TextTypes();
                        wrangle.Type = PayloadType.RawText;
                        wrangle.Text = ">>";
                        rawtext.Add(wrangle);
                    }


                    foreach (var payload in payloads)
                    {
                        if (payload.Type == PayloadType.MapLink)
                        {
                            replace = 2;
                            payloadType = PayloadType.MapLink;
                            payloader = payload;
                        }

                        if (payload.Type == PayloadType.RawText)
                        {
                            TextTypes wrangler = new TextTypes();
                            wrangler.Text = payload.ToString().Split(new[] { ' ' }, 4)[3];

                            if (replace == 1)
                            {
                                if (payloadType == PayloadType.MapLink)
                                {
                                    rawtext.RemoveAt(rawtext.Count - 1);
                                    wrangler.Payload = payloader;
                                }
                            }

                            if (replace == 0)
                            {
                                payloadType = PayloadType.RawText;
                            }

                            wrangler.Type = payloadType;
                            rawtext.Add(wrangler);

                            if (replace > 0)
                            {
                                replace--;
                            }
                        }
                    }

                    tmp.Text = rawtext;
                    
                    if(System.Text.RegularExpressions.Regex.Match(tmp.Sender, "^[-]").Success)
                    {
                        tmp.Sender = tmp.Sender.Substring(1);
                    }
                    
                    if (bubbleEnable[chan])
                    {
                        ChatBubbleAdd(tmp);
                    }
                    


                    foreach (var tab in items)
                    {
                        if (chan < Channels.Length && tab.Logs[chan])
                        {
                            tab.Chat.Enqueue(tmp);
                            tab.msg = true;

                            if (tab.Chat.Count > 256)
                            { tab.Chat.TryDequeue(out ChatText pop); }

                            if (tab.Config[3])
                            {
                                //Writing to file
                                string filename = GetDate() + "_" + tab.Title + ".txt";
                                if (!System.IO.Directory.Exists(pathString))
                                { System.IO.Directory.CreateDirectory(pathString); }

                                if (!System.IO.File.Exists(pathString + filename))
                                { System.IO.File.WriteAllText(pathString + filename, tab.Title + "\n"); }

                                using (System.IO.StreamWriter file = new System.IO.StreamWriter(pathString + filename, true))
                                { file.WriteLine(tmp.Time + "[" + tmp.Channel + "]" + "<" + tmp.Sender + ">:" + TextTypesToString(rawtext)); }
                            }

                            if (tab.AutoScroll == true)
                            { tab.Scroll = true; }
                        }
                        else { }//PluginLog.Log("[" + chan.ToString() + "] " + message.TextValue);
                    }

                    if (allowTranslation)
                    {
                        String messageString = message.TextValue;
                        String predictedLanguage = Lang(messageString);
                        if (predictedLanguage == language)
                        {
                            Task.Run(() => Tran(type, messageString, senderName));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                PluginLog.LogError(e.ToString());
            }

        }

        public void Broadcast(string message)
        {

            foreach (var tab in items)
            {
                ChatText tmp = new ChatText();

                tmp.Time = GetTime();
                tmp.ChannelShort = "[N]";
                tmp.Channel = "Notice";
                tmp.Sender = "";
                tmp.ChannelColour = ConvertForArray("Notice");
                List<TextTypes> rawtext = new List<TextTypes>();

                PayloadType payloadType = PayloadType.RawText;
                TextTypes wrangler = new TextTypes();
                wrangler.Text = message;
                wrangler.Type = payloadType;
                rawtext.Add(wrangler);

                tmp.Text = rawtext;
                tab.Chat.Enqueue(tmp);

                if (tab.Chat.Count > 256)
                {
                    tab.Chat.TryDequeue(out ChatText pop);
                }

                if (tab.AutoScroll == true)
                {
                    tab.Scroll = true;
                }
            }
        }

        public string TextTypesToString(List<TextTypes> textTypes)
        {
            string str = "";
            foreach (TextTypes texts in textTypes)
            {
                str += texts.Text + " ";
            }
            return str;
        }

        public string GetDate()
        {
            return DateTime.Now.ToString("yyyyMMdd");
        }

        public string GetTime()
        {
            string temp = "[";
            if (hourTime)
            {
                if (DateTime.Now.ToString("%H").Length == 1) { temp += "0"; }
                temp += DateTime.Now.ToString("%H" + ":");
            }    
            else
            {
                if (DateTime.Now.ToString("%h").Length == 1) { temp += "0"; }
                temp += DateTime.Now.ToString("%h" + ":");
            }
            if (DateTime.Now.ToString("%m").Length == 1) { temp += "0"; }
            temp += DateTime.Now.ToString("%m" + "]");
            return temp;
        }

        public void PrintChat(XivChatType type, string senderName, string messageString)
        {
            var chat = new XivChatEntry
            { Type = type, Name = senderName, MessageBytes = Encoding.UTF8.GetBytes(messageString) };
            pluginInterface.Framework.Gui.Chat.PrintChat(chat);
        }

        public void Tran(XivChatType type, string messageString, string senderName)
        {

            string output = Translate(messageString);

            if (injectChat == true)
            {
                PrintChat(type, senderName, lTr + output + rTr);
            }

            foreach (var tab in items)
            {

                if (tab.Logs[ConvertForArray(type.ToString())] && tab.Config[2])
                {
                    ChatText tmp = new ChatText();

                    tmp.Time = GetTime();
                    tmp.ChannelShort = GetChannelName(type.ToString());
                    tmp.Channel = type.ToString();
                    tmp.Sender = senderName;

                    TextTypes translate = new TextTypes();
                    translate.Text = lTr + output + rTr;
                    translate.Type = PayloadType.RawText;
                    tmp.Text.Add(translate);

                    tab.Chat.Enqueue(tmp);

                    if (tab.AutoScroll == true)
                    {
                        tab.Scroll = true;
                    }
                    tab.msg = true;
                }

            }

        }


        public string Translate(string apple)
        {
            if (translator == 1)
            {
                var text = TransG.TranslateLiteAsync(apple, Language.Auto, Language.English).GetAwaiter().GetResult();
                return text.MergedTranslation;
            }
            else
            {
                var text = TransY.Translate(apple, "en");
                return text.Text[0];
            }

        }

        public static string Lang(string banana)
        {

            var languages = identifier.Identify(banana);
            var mostCertainLanguage = languages.FirstOrDefault();
            if (mostCertainLanguage != null)
            {
                //Console.WriteLine("The language of the text is:" + mostCertainLanguage.Item1.Iso639_3);
                return mostCertainLanguage.Item1.Iso639_3;
            }
            else
                return ("The language couldn’t be identified with an acceptable degree of certainty");
        }
        public void Dispose()
        {
            pluginInterface.Framework.Gui.Chat.OnChatMessage -= Chat_OnChatMessage;
            pluginInterface.CommandManager.RemoveHandler("/cht");
            this.pluginInterface.UiBuilder.OnBuildUi -= ChatUI;
            this.pluginInterface.UiBuilder.OnBuildUi -= ChatBubbles;
            pluginInterface.UiBuilder.OnOpenConfigUi -= Chat_ConfigWindow;
            goatImage.Dispose();
            this.pluginInterface.UiBuilder.OnBuildFonts -= AddFont;
        }

        bool CheckDupe(List<TabBase> items, string title)
        {
            foreach (var tab in items)
            {
                if (title == tab.Title) { return true; }
            }
            return false;
        }

        public void BuildImGuiFont()
        {            PluginLog.Log("BEFORE CALL: " + font.IsLoaded().ToString());
            //IntPtr x = ImGui.GetCurrentContext();
            //ImGui.GetIO().Fonts.Build();
            //ImGui.SetCurrentContext(x);
            PluginLog.Log("AFTER CALL: " + font.IsLoaded().ToString());
        }

        public void ChatBubbleAdd(ChatText chatText)
        {

            for (int i = 0; i < chatBubble.Count; i++)
            {
                if (chatBubble[i].Sender == chatText.Sender)
                {
                    chatBubble[i] = chatText;
                    bubbleOffsets[i].x = 0;
                    bubbleOffsets[i].y = 0;
                    bubbleOffsets[i].Width = 0;
                    bubbleOffsets[i].Height = 0;
                    bubbleOffsets[i].extra= noRepeats;
                    noRepeats++;
                    return;
                }
            }
            BubbleOffset bubbleOffset = new BubbleOffset();
            bubbleOffset.name = chatText.Sender.ToString();
            bubbleOffset.extra = noRepeats;
            noRepeats++;

            chatBubble.Add(chatText);
            bubbleOffsets.Add(bubbleOffset);
        }

        public void DrawChatBubble(Dalamud.Game.ClientState.Actors.Types.Chara actor, ChatText chat)
        {
            if (pluginInterface.Framework.Gui.WorldToScreen(new SharpDX.Vector3(actor.Position.X, actor.Position.Z + AddHeight(actor), actor.Position.Y), out SharpDX.Vector2 pos))
            {

                int lookup = GetChatPos(actor.Name);

                String[] senderSplit = chat.Sender.Split(' ');
                string senderName = senderSplit[0] + " " + senderSplit[1];
                
                ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new Num.Vector2(ImGui.CalcTextSize(senderName).X + 50 ,20));
                ImGui.Begin(chat.Sender + bubbleOffsets[lookup].extra.ToString(), ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoBackground);
                ImGui.SetWindowPos(new Num.Vector2(pos.X + 30 + bubbleOffsets[lookup].x, pos.Y + bubbleOffsets[lookup].y));
                
                ImGui.PushStyleColor(ImGuiCol.Text, UintCol(0, 0, 0, 0));
                if (bubblesChannel)
                {
                    ImGui.Text(chat.ChannelShort + ": "); ImGui.SameLine();
                }
                string message = TextTypesToString(chat.Text);

                
                ImGui.PushTextWrapPos(maxBubbleWidth);
                ImGui.TextWrapped(message);
                ImGui.PopTextWrapPos();
                ImGui.PopStyleColor();
                ImGui.PopStyleVar();
                              



                bubbleOffsets[lookup].Pos = ImGui.GetWindowPos();
                bubbleOffsets[lookup].Width = ImGui.GetWindowWidth();
                bubbleOffsets[lookup].Height = ImGui.GetWindowHeight();

                resolveCollision(lookup, pos, true);
                Num.Vector2 finalpos = ImGui.GetWindowPos();

                ImGui.End();



                //WIP
                float ImageWidth  = 12.5f * ((bubbleOffsets[lookup].Width  - 25f) / 12.5f) - xCut;
                float ImageHeight = 12.5f * ((bubbleOffsets[lookup].Height - 25f) / 12.5f) - yCut;
                //uint colour = ImGui.GetColorU32(new Num.Vector4(255, 255, 255, 255));


                ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new Num.Vector2(ImGui.CalcTextSize(senderName).X + 50, 20));
                ImGui.Begin(chat.Sender + "visible" + bubbleOffsets[lookup].extra.ToString(), ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoBackground);
                ImGui.SetWindowPos(new Num.Vector2(pos.X + 30 + bubbleOffsets[lookup].x, pos.Y + bubbleOffsets[lookup].y));

                //Draw Box
                ImGui.SetCursorScreenPos(new Num.Vector2(finalpos.X + xDisp, finalpos.Y + yDisp));
                //ImGui.GetWindowDrawList().AddRectFilled(new Num.Vector2(finalpos.X + xDisp, finalpos.Y + yDisp), new Num.Vector2(finalpos.X + xDisp + 25f + ImageWidth, finalpos.Y + 25f + ImageHeight + yDisp), UintCol(high.htA, high.htB, high.htG, high.htR), bubbleRounding);
                ImGui.GetWindowDrawList().AddRectFilled(new Num.Vector2(finalpos.X + xDisp, finalpos.Y + yDisp), new Num.Vector2(finalpos.X + xDisp + 25f + ImageWidth, finalpos.Y + 25f + ImageHeight + yDisp), ImGui.GetColorU32(bubbleColour[chat.ChannelColour]), bubbleRounding);
                //Draw Outside
                //Top Left of Box (Set size)
                ImGui.SetCursorScreenPos(new Num.Vector2(finalpos.X + xDisp, finalpos.Y + yDisp));
                ImGui.Image(goatImage.ImGuiHandle, new Num.Vector2(12.5f, 12.5f), bubble_TL1, bubble_TL2);
                //Top Middle of Box (Variable Width)
                ImGui.SetCursorScreenPos(new Num.Vector2(finalpos.X + xDisp + 12.5f, finalpos.Y + yDisp));
                ImGui.Image(goatImage.ImGuiHandle, new Num.Vector2(ImageWidth, 12.5f), bubble_TM1, bubble_TM2);
                //Top Right of Box (Set Size)
                ImGui.SetCursorScreenPos(new Num.Vector2(finalpos.X + xDisp + 12.5f + ImageWidth, finalpos.Y + yDisp));
                ImGui.Image(goatImage.ImGuiHandle, new Num.Vector2(12.5f, 12.5f), bubble_TR1, bubble_TR2);
                //Mid Left of Box (Variable Hight)
                ImGui.SetCursorScreenPos(new Num.Vector2(finalpos.X + xDisp, finalpos.Y + 12.5f + yDisp));
                ImGui.Image(goatImage.ImGuiHandle, new Num.Vector2(12.5f, ImageHeight), bubble_ML1, bubble_ML2);
                //Mid Middle of Box (Variable Width and Height)
                ImGui.SetCursorScreenPos(new Num.Vector2(finalpos.X + xDisp + 12.5f, finalpos.Y +12.5f + yDisp));
                ImGui.Image(goatImage.ImGuiHandle, new Num.Vector2(ImageWidth, ImageHeight), bubble_MM1, bubble_MM2);
                //Mid Right of Box (Variable Height)
                ImGui.SetCursorScreenPos(new Num.Vector2(finalpos.X + xDisp + 12.5f + ImageWidth, finalpos.Y + 12.5f + yDisp));
                ImGui.Image(goatImage.ImGuiHandle, new Num.Vector2(12.5f, ImageHeight), bubble_MR1, bubble_MR2);
                //Bot Left of Box (Set Size)
                ImGui.SetCursorScreenPos(new Num.Vector2(finalpos.X + xDisp, finalpos.Y + 12.5f + ImageHeight + yDisp));
                ImGui.Image(goatImage.ImGuiHandle, new Num.Vector2(12.5f, 12.5f), bubble_BL1, bubble_BL2);
                //Bot Middle of Box (Variable Width)
                ImGui.SetCursorScreenPos(new Num.Vector2(finalpos.X + xDisp + 12.5f, finalpos.Y + 12.5f + ImageHeight + yDisp));
                ImGui.Image(goatImage.ImGuiHandle, new Num.Vector2(ImageWidth, 12.5f), bubble_BM1, bubble_BM2);
                //Bot Right of Box (Set Size)
                ImGui.SetCursorScreenPos(new Num.Vector2(finalpos.X + xDisp + 12.5f + ImageWidth, finalpos.Y + 12.5f + ImageHeight + yDisp));
                ImGui.Image(goatImage.ImGuiHandle, new Num.Vector2(12.5f, 12.5f), bubble_BR1, bubble_BR2);


                ImGui.SetCursorScreenPos(new Num.Vector2(finalpos.X + 12, finalpos.Y+ 6 + ((ImGui.GetFontSize() - 15) / 3)));

                ImGui.PushStyleColor(ImGuiCol.Text, UintCol(255, 0, 0, 0));
                if (bubblesChannel)
                {
                    ImGui.Text(chat.ChannelShort + ": "); ImGui.SameLine();
                }
                
                
                ImGui.PushTextWrapPos(maxBubbleWidth);
                ImGui.TextWrapped(message); ImGui.SameLine(); ImGui.Text(" ");
                ImGui.PopTextWrapPos();
                ImGui.PopStyleColor();


                ImGui.PopStyleVar();
                ImGui.End();

                ImGui.Begin(chat.Sender + "Name" + bubbleOffsets[lookup].extra.ToString(), ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoBackground);
                
                ImGui.SetWindowPos(new Num.Vector2(finalpos.X+5,finalpos.Y-18));

                ImGui.PushStyleColor(ImGuiCol.Text, UintCol(255,0,0,0));
                Num.Vector2 finalpos2 = ImGui.GetCursorPos();
                ImGui.SetCursorPos(new Num.Vector2(finalpos2.X + 1, finalpos2.Y + 1 - ((ImGui.GetFontSize() - 15) / 3)));
                ImGui.Text(chat.Sender);
                ImGui.PopStyleColor();
                ImGui.SetCursorPos(new Num.Vector2(finalpos2.X, finalpos2.Y - ((ImGui.GetFontSize() - 15) / 3)));
                ImGui.Text(chat.Sender);
                ImGui.End();

                /* Working on indicator
                ImGui.Begin(chat.Sender + "Arrow" + bubbleOffsets[lookup].extra.ToString(), ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysAutoResize);
                ImGui.GetWindowDrawList().AddTriangle(
                    new Num.Vector2(10, 10),
                    new Num.Vector2(10, 50),
                    new Num.Vector2(50, 50),
                    ImGui.GetColorU32(bubbleColour[chat.ChannelColour]),
                    2.0f
                    );
                ImGui.End();
                */
            }


        }

        public void resolveCollision(int lookup, SharpDX.Vector2 pos, bool reset)
        {
            for (int i = 0; i < bubbleOffsets.Count; i++)
            {
                BoundingBox boundingBoxA = new BoundingBox();
                BoundingBox boundingBoxB = new BoundingBox();

                bool collision = false;
                if (bubbleOffsets[lookup].y != 0 && reset)
                {
                    for (int j = 0; j < bubbleOffsets.Count; j++)
                    {

                        if (j != lookup)
                        {
                            boundingBoxA.min = new Num.Vector2(pos.X + 25, pos.Y - 5);
                            boundingBoxA.max = new Num.Vector2(pos.X + 35 + ImGui.GetWindowWidth(), pos.Y + ImGui.GetWindowHeight() + 5);

                            boundingBoxB.min = bubbleOffsets[j].Pos;
                            boundingBoxB.max = new Num.Vector2(bubbleOffsets[j].Pos.X + bubbleOffsets[j].Width, bubbleOffsets[j].Pos.Y + bubbleOffsets[j].Height);

                            if (isCollision(boundingBoxA, boundingBoxB))
                            {
                                collision = true;
                            }
                        }
                    }
                    if (!collision)
                    {
                        bubbleOffsets[lookup].y = 0;
                        //PluginLog.Log("Resetting position of: " + lookup.ToString());
                        ImGui.SetWindowPos(new Num.Vector2(pos.X + 30 + bubbleOffsets[lookup].x, pos.Y + bubbleOffsets[lookup].y));
                    }
                }



                if (i != lookup)
                {
                    
                    boundingBoxA.min = ImGui.GetWindowPos();
                    boundingBoxA.max = new Num.Vector2(ImGui.GetWindowPos().X + ImGui.GetWindowWidth(), ImGui.GetWindowPos().Y + ImGui.GetWindowHeight());

                    
                    boundingBoxB.min = bubbleOffsets[i].Pos;
                    boundingBoxB.max = new Num.Vector2(bubbleOffsets[i].Pos.X + bubbleOffsets[i].Width, bubbleOffsets[i].Pos.Y + bubbleOffsets[i].Height);

                    if (isCollision(boundingBoxA, boundingBoxB))
                    {
                        if (boolUp) { bubbleOffsets[lookup].y = bubbleOffsets[lookup].y + (int)(boundingBoxB.min.Y - boundingBoxA.min.Y - (bubbleOffsets[lookup].Height)) - 10; }
                        else { bubbleOffsets[lookup].y = bubbleOffsets[lookup].y + Math.Abs((int)(boundingBoxA.min.Y - boundingBoxB.min.Y - (bubbleOffsets[lookup].Height))) + 10; }
                        
                        ImGui.SetWindowPos(new Num.Vector2(pos.X + 30 + bubbleOffsets[lookup].x, pos.Y + bubbleOffsets[lookup].y));
                        //PluginLog.Log("Resolving crash\n" +lookup.ToString()+" hit "+i.ToString()+"\n"+"Y: " + bubbleOffsets[lookup].y.ToString());
                        resolveCollision(lookup, pos, false);
                    }



                }
            }
        }

        bool isCollision(BoundingBox a, BoundingBox b)
        {
            // Exit with no intersection if found separated along an axis
            if (a.min.X < b.min.X || a.min.X > b.max.X) return false;
            if (a.max.Y < b.min.Y || a.min.Y > b.max.Y) return false;

            // No separating axis found, therefor there is at least one overlapping axis
            return true;
        }

        public int GetChatPos(string name)
        {
            int x = 0;
            foreach(BubbleOffset bubble in bubbleOffsets)
            {
                if(bubble.name == name) { break; }
                x++;
            }
            return x;
        }

        public void CleanupBubbles()
        {
            for (int i = 0; i < chatBubble.Count; i++)
            {
                if ((DateTime.Now - chatBubble[i].DateTime).TotalSeconds > bubbleTime)
                {
                    chatBubble.RemoveAt(i);
                    bubbleOffsets.RemoveAt(i);
                    i--;
                }
            }
        }

        public float AddHeight(Dalamud.Game.ClientState.Actors.Types.Chara chara)
        {
            float Height = chara.Customize[(int)Dalamud.Game.ClientState.Actors.CustomizeIndex.Height];
            int race = chara.Customize[(int)Dalamud.Game.ClientState.Actors.CustomizeIndex.Race];
            int gender = chara.Customize[(int)Dalamud.Game.ClientState.Actors.CustomizeIndex.Gender];

            switch ((Race)race)
            {
                case Race.Hyur:     return 1.50f + 0.002f * Height;
                case Race.Elezen:   return 1.80f + 0.002f * Height;
                case Race.Lalafell: return 1.00f + 0.001f * Height;
                case Race.Miqote:   return 1.45f + 0.003f * Height;
                case Race.Roegadyn: return 2.00f + 0.001f * Height;
                case Race.AuRa:
                    if(gender == (int)Gender.Male)
                                    return 2.00f + 0.001f * Height; 
                    else 
                                    return 1.40f + 0.001f * Height;
                case Race.Hrothgar: return 1.85f + 0.002f * Height;
                case Race.Viera:    return 1.75f + 0.002f * Height;
            }
            return minH + maxH * Height;


        }
    }
}
