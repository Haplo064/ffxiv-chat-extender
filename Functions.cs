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

namespace DalamudPlugin
{
    public partial class ChatExtenderPlugin : IDalamudPlugin
    {

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
                    babyClone.Chat = new List<ChatText>();
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
            this.pluginInterface.SavePluginConfig(Configuration);
        }

        public void HighlightText()
        {
            ImGui.GetWindowDrawList().AddRectFilled(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), UintCol(high.htA, high.htB, high.htG, high.htR), 2.0f);
        }

        public void Wrap(String input)
        {
            input = input.Replace("\n", " XXX ");
            input = input.Replace("\r", " XXX ");

            String[] inputArray = input.Split(' ');

            int count = 0;
            foreach (String splits in inputArray)
            {
                bool newline = false;
                if (ImGui.GetContentRegionAvail().X - 5 - ImGui.CalcTextSize(splits).X < 0)
                { ImGui.Text(""); }

                if (splits == "XXX") { newline = true; PluginLog.Log("newline set to true"); }
                else { ImGui.Text(splits.Trim()); }
                
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
                PluginLog.Log(e.ToString());
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


                    foreach (var tab in items)
                    {
                        int chan = ConvertForArray(type.ToString());
                        if (chan < Channels.Length)
                        {
                            if (tab.Logs[chan])
                            {
                                ChatText tmp = new ChatText();

                                tmp.Time = GetTime();
                                tmp.ChannelShort = GetChannelName(type.ToString());
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
                                if(tmp.Channel== "StandardEmote")
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

                                foreach (var payload in payloads)
                                {
                                    //if (payload.Type == PayloadType.AutoTranslateText) { texttype = 0; }
                                    //if (payload.Type == PayloadType.Item) { texttype = 1; }
                                    if (payload.Type == PayloadType.MapLink)
                                    {
                                        replace = 2;
                                        payloadType = PayloadType.MapLink;
                                        payloader = payload;
                                    }
                                    //if (payload.Type == PayloadType.Player) { texttype = 3; }
                                    //if (payload.Type == PayloadType.RawText) { texttype = 4; }
                                    //if (payload.Type == PayloadType.Status) { texttype = 5; }
                                    //if (payload.Type == PayloadType.UIForeground) { texttype = 6; }
                                    //if (payload.Type == PayloadType.UIGlow) { texttype = 7; }

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

                                    //PluginLog.Log(payload.ToString());

                                }

                                tmp.Text = rawtext;



                                tab.Chat.Add(tmp);

                                if (tab.Chat.Count > 256)
                                {
                                    tab.Chat.RemoveAt(0);
                                }

                                if (tab.Config[3])
                                {
                                    //Writing to file
                                    string filename = GetDate() + "_" + tab.Title + ".txt";
                                    if (!System.IO.Directory.Exists(pathString))
                                    {
                                        System.IO.Directory.CreateDirectory(pathString);
                                    }

                                    if (!System.IO.File.Exists(pathString + filename))
                                    {
                                        System.IO.File.WriteAllText(pathString + filename, tab.Title + "\n");
                                    }

                                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(pathString + filename, true))
                                    {
                                        file.WriteLine(tmp.Time + "[" + tmp.Channel + "]" + "<" + tmp.Sender + ">:" + TextTypesToString(rawtext));
                                    }
                                }

                                if (tab.AutoScroll == true)
                                {
                                    tab.Scroll = true;
                                }
                                tab.msg = true;
                            }
                        }
                        else PluginLog.Log("[" + chan.ToString() + "] " + message.TextValue);

                    }

                    String messageString = message.TextValue;
                    String predictedLanguage = Lang(messageString);
                    if (predictedLanguage == language)
                    {
                        Task.Run(() => Tran(type, messageString, senderName));
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
                tab.Chat.Add(tmp);

                if (tab.Chat.Count > 256)
                {
                    tab.Chat.RemoveAt(0);
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
            if (DateTime.Now.ToString("%h").Length == 1) { temp += "0"; }
            temp += DateTime.Now.ToString("%h" + ":");
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

                    tab.Chat.Add(tmp);

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
            pluginInterface.UiBuilder.OnOpenConfigUi -= Chat_ConfigWindow;
        }

        bool CheckDupe(List<TabBase> items, string title)
        {
            foreach (var tab in items)
            {
                if (title == tab.Title) { return true; }
            }
            return false;
        }

    }
}
