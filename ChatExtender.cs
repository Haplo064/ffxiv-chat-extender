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
using Newtonsoft.Json;
using System.IO;
using System.Runtime.CompilerServices;
using Dalamud.Configuration;
using Num = System.Numerics;

//TODO
//Add colour config - Split into 3?
//Add Custom wrapper

//Add select+copy?
//Add locking in place
//Add clickthrough
//Add write to file

//Add spacing config
//Font? - Likely change to gothic
//Fix up missing chat text
//Add in handling for quick-translate stuff
//Add in text finding
//Add in text higlighting?
//Add in Yandex Key via config
//Add in support for more than japanese?
//Add in config for language selections?


namespace DalamudPlugin
{
    public class ChatExtenderPlugin : IDalamudPlugin
    {
        // Dalamud Plugin

        public string Name => "Translator Plugin";
        private DalamudPluginInterface pluginInterface;
        private bool chatWindow = false;
        private bool configWindow = false;
        //Globals
        public bool injectChat = false;
        public int translator = 1;         //1=Google,2=Yandex
        public string language = "jpn";
        public string yandex = "";
        public List<TabBase> items = new List<TabBase>();
        public List<TabBase> itemsTemp = new List<TabBase>();
        public uint bufSize = 24;
        public string tempTitle = "Title";
        public float alpha = 0.2f;

        //Google Translate
        private static readonly GoogleTranslator TransG = new GoogleTranslator();
        // Yandex Translate
        private YandexTranslate.Translator TransY = new YandexTranslate.Translator();
        // NCat
        public static RankedLanguageIdentifierFactory factory = new RankedLanguageIdentifierFactory();
        public static RankedLanguageIdentifier identifier = factory.Load(Path.Combine(AssemblyDirectory, "Core14.profile.xml"));

        public ChatExtenderPluginConfiguration Configuration;

        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = typeof(ChatExtenderPlugin).Assembly.CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            // Initializing plugin, hooking into chat.
            this.pluginInterface = pluginInterface;
            Configuration = pluginInterface.GetPluginConfig() as ChatExtenderPluginConfiguration ?? new ChatExtenderPluginConfiguration();

            if (Configuration.Items == null)
            {
                //Serilog.Log.Information("Null DynTab List");
                items.Add(new DynTab("XXX", new List<ChatText>(), true));
            }
            else
            {
                //Serilog.Log.Information("Not Null DynTab List");
                if (Configuration.Items.Count == 0)
                {
                    //Serilog.Log.Information("Empty DynTab List");
                    items.Add(new DynTab("YYY", new List<ChatText>(), true));
                }
                else
                {
                    foreach (var obj in Configuration.Items)
                    {
                        if (obj.Enabled == true)
                        {
                            itemsTemp.Add(obj);
                        }
                    }
                    Configuration.Items = itemsTemp.ToList();

                    //Serilog.Log.Information("Normal DynTab List");
                    items = Configuration.Items.ToList();
                }
            }

            if (Configuration.Inject == true)
            {
                injectChat = true;
            }

            if (Configuration.Translator == 2)
            {
                translator = 2;
            }

            if (Configuration.YandexKey != null)
            {
                yandex = Configuration.YandexKey.ToString();
            }

            if (Configuration.Extender == true)
            {
                chatWindow = true;
            }

            if (Configuration.Alpha != 0.2f)
            {
                alpha = Configuration.Alpha;
            }

            TransY.Make("https://translate.yandex.net/api/v1.5/tr.json/translate", Configuration.YandexKey);

            // Set up command handlers
            this.pluginInterface.CommandManager.AddHandler("/cht", new CommandInfo(OnTranslateCommand)
            {
                HelpMessage = "Configure Translator Engine of Translator. Usage: /cht t <#> (1=Google, 2=Yandex)"
            });

            this.pluginInterface.Framework.Gui.Chat.OnChatMessage += Chat_OnChatMessage;
            this.pluginInterface.UiBuilder.OnBuildUi += ChatUI;
            this.pluginInterface.UiBuilder.OnOpenConfigUi += Chat_ConfigWindow;
        }

        private void Chat_ConfigWindow(object Sender, EventArgs args)
        {
            configWindow = true;
        }


        private void OnTranslateCommand(string command, string arguments)
        {
            if (string.IsNullOrEmpty(arguments))
            {
                this.pluginInterface.Framework.Gui.Chat.PrintError("No setting specified.");
                return;
            }

            else
            {
                //TODO: Replace this via gui config
                String[] bits = arguments.Split(' ');
                if (bits[0] == "e" | bits[0] == "engine")
                {
                    if (bits[1] == "1" | bits[1] == "google") {translator = 1; PrintChat(XivChatType.Notice, "[CHT]", "Translator set to Google"); return; }
                    else if (bits[1] == "2"| bits[1] == "yandex") {translator = 2; PrintChat(XivChatType.Notice, "[CHT]", "Translator set to Yandex"); return; }
                    else {this.pluginInterface.Framework.Gui.Chat.PrintError("No valid setting supplied. 1=Google, 2=Yandex"); return; }
                }

                else if (bits[0] == "i" | bits[0] == "inject")
                {
                    if (bits[1] == "1" | bits[1] == "true" | bits[1] == "on") { injectChat = true; PrintChat(XivChatType.Notice, "[CHT]", "Chat injection on"); return; }
                    else if (bits[1] == "0" | bits[1] == "false" | bits[1] == "off") { injectChat = false; PrintChat(XivChatType.Notice, "[CHT]", "Chat injection off"); return; }
                    else { this.pluginInterface.Framework.Gui.Chat.PrintError("No valid setting supplied. Try: 1/0, on/off, true/false"); return; }
                }

                else if  (bits[0] == "w" | bits[0] == "window")
                {
                    if (this.chatWindow)
                    {
                        this.chatWindow = false;
                        PrintChat(XivChatType.Notice, "<CHT>", "Closed Chat Window");
                    }
                    else
                    {
                        this.chatWindow = true;
                        PrintChat(XivChatType.Notice, "<CHT>", "Opened Chat Window");
                    }
                    return;
                }

                else if (bits[0] == "c" | bits[0] == "config")
                {
                    if (this.configWindow)
                    {
                        this.configWindow = false;
                        PrintChat(XivChatType.Notice, "<CHT>", "Closed config Window");
                    }
                    else
                    {
                        this.configWindow = true;
                        PrintChat(XivChatType.Notice, "<CHT>", "Opened config Window");
                    }
                    return;
                }

                else if (bits[0] == "h" | bits[0] == "help")
                {PrintChat(XivChatType.Notice, "[CHT]", "w = chat window, c = config"); return; }
                
                else
                { this.pluginInterface.Framework.Gui.Chat.PrintError("No valid command supplied. Try 'h/help'"); return; }
            }



        }

        public class ChatExtenderPluginConfiguration : IPluginConfiguration
        {
            public int Version { get; set; } = 0;
            public string YandexKey { get; set; }
            public List<TabBase> Items { get; set; }
            public bool Inject { get; set; }
            public int Translator { get; set; }
            public bool Extender { get; set; }
            public float Alpha { get; set; }
        }

        private void ChatUI()
        {
            if (chatWindow)
            {
                ImGui.SetNextWindowSize(new Num.Vector2(200, 100), ImGuiCond.FirstUseEver);
                ImGui.SetNextWindowBgAlpha(alpha);
                ImGui.Begin("Another Window", ref chatWindow, ImGuiWindowFlags.NoTitleBar);
                ImGuiTabBarFlags tab_bar_flags = ImGuiTabBarFlags.None;
                if (ImGui.BeginTabBar("Tabs", tab_bar_flags))
                {
                    foreach (var tab in items)
                    {
                        if (tab.Enabled)
                        {
                            if (ImGui.BeginTabItem(tab.Title))
                            {
                                float footer = (ImGui.GetStyle().ItemSpacing.Y) / 2 + ImGui.GetFrameHeightWithSpacing();
                                ImGui.BeginChild("scrolling", new Num.Vector2(0, -footer), false);
                                foreach (ChatText line in tab.Chat)
                                {

                                    if (tab.Config[0]) { ImGui.TextWrapped(line.Time + " "); ImGui.SameLine(); }
                                    if (tab.Config[1]) { ImGui.TextWrapped(line.Channel + " "); ImGui.SameLine(); }
                                    if (line.Sender.Length > 0) { ImGui.TextWrapped(line.Sender + ":"); ImGui.SameLine(); }
                                    /*
                                    if (tab.Config[1]) { tmp += line.Channel + " "; }
                                    if (line.Sender.Length > 0) { tmp += line.Sender + ":"; }
                                    */
                                    ImGui.TextWrapped(line.Text);
                                }
                                if (tab.Scroll == true)
                                {
                                    ImGui.SetScrollHereY();
                                    tab.Scroll = false;
                                }
                                ImGui.EndChild();
                                ImGui.EndTabItem();
                            }
                        }
                    }
                    ImGui.EndTabBar();
                    ImGui.End();
                }
            }

            if (configWindow)
            {

                ImGui.SetNextWindowSize(new Num.Vector2(300, 500), ImGuiCond.FirstUseEver);
                ImGui.Begin("Chat Config", ref configWindow);

                float footer1 = (ImGui.GetStyle().ItemSpacing.Y) / 2 + ImGui.GetFrameHeightWithSpacing();
                ImGui.BeginChild("scrolling", new Num.Vector2(0, -footer1), false);
                ImGui.Text("Config");
                ImGui.Checkbox("Inject Translate into chat", ref injectChat);
                ImGui.Checkbox("Display Chat Extender", ref chatWindow);
                ImGui.SliderFloat("Alpha", ref alpha, 0.1f, 0.999f);

                foreach (var tab in items)
                {
                    if (tab.Enabled)
                    {

                        if (ImGui.TreeNode(tab.Title))
                        {
                            float footer2 = (ImGui.GetStyle().ItemSpacing.Y) / 2 + ImGui.GetFrameHeightWithSpacing();
                            ImGui.BeginChild("scrolling", new Num.Vector2(0, -footer2), false);
                            ImGui.InputText("Tab Name", ref tempTitle, bufSize);
                            ImGui.SameLine();
                            if (ImGui.Button("Set Tab Title"))
                            {
                                if (tempTitle.Length == 0) { tempTitle += "."; }

                                while (CheckDupe(items, tempTitle))
                                { tempTitle += "."; }

                                tab.Title = tempTitle;
                                tempTitle = "Title";
                            }
                            ImGui.Checkbox("Time Stamp", ref tab.Config[0]);
                            ImGui.SameLine();
                            ImGui.Checkbox("Channel", ref tab.Config[1]);
                            ImGui.SameLine();
                            ImGui.Checkbox("Translate", ref tab.Config[2]);
                            ImGui.Checkbox("AutoScroll", ref tab.AutoScroll);
                            if (ImGui.TreeNode("Channels"))
                            {
                                ImGui.Checkbox("None/Not Captured", ref tab.Logs[0]);
                                ImGui.Checkbox("Debug", ref tab.Logs[1]);
                                ImGui.Checkbox("Urgent", ref tab.Logs[2]);
                                ImGui.Checkbox("Notice", ref tab.Logs[3]);
                                ImGui.Checkbox("Say", ref tab.Logs[4]);
                                ImGui.Checkbox("Shout", ref tab.Logs[5]);
                                ImGui.Checkbox("Tell Outgoing", ref tab.Logs[6]);
                                ImGui.Checkbox("Tell Incoming", ref tab.Logs[7]);
                                ImGui.Checkbox("Party", ref tab.Logs[8]);
                                ImGui.Checkbox("Alliance", ref tab.Logs[9]);
                                ImGui.Checkbox("Link Shell 1", ref tab.Logs[10]);
                                ImGui.Checkbox("Link Shell 2", ref tab.Logs[11]);
                                ImGui.Checkbox("Link Shell 3", ref tab.Logs[12]);
                                ImGui.Checkbox("Link Shell 4", ref tab.Logs[13]);
                                ImGui.Checkbox("Link Shell 5", ref tab.Logs[14]);
                                ImGui.Checkbox("Link Shell 6", ref tab.Logs[15]);
                                ImGui.Checkbox("Link Shell 7", ref tab.Logs[16]);
                                ImGui.Checkbox("Link Shell 8", ref tab.Logs[17]);
                                ImGui.Checkbox("FreeCompany", ref tab.Logs[18]);
                                ImGui.Checkbox("Novice Network", ref tab.Logs[19]);
                                ImGui.Checkbox("Custom Emote", ref tab.Logs[20]);
                                ImGui.Checkbox("Standard Emote", ref tab.Logs[21]);
                                ImGui.Checkbox("Yell", ref tab.Logs[22]);
                                ImGui.Checkbox("Cross Party", ref tab.Logs[23]);
                                ImGui.Checkbox("PVP Team", ref tab.Logs[24]);
                                ImGui.Checkbox("Echo", ref tab.Logs[25]);
                                ImGui.Checkbox("System Error", ref tab.Logs[26]);
                                ImGui.Checkbox("Gathering System Message", ref tab.Logs[27]);
                                ImGui.Checkbox("Retainor Sale", ref tab.Logs[28]);
                                ImGui.Checkbox("Cross Link Shell 1", ref tab.Logs[29]);
                                ImGui.Checkbox("Cross Link Shell 2", ref tab.Logs[30]);
                                ImGui.Checkbox("Cross Link Shell 3", ref tab.Logs[31]);
                                ImGui.Checkbox("Cross Link Shell 4", ref tab.Logs[32]);
                                ImGui.Checkbox("Cross Link Shell 5", ref tab.Logs[33]);
                                ImGui.Checkbox("Cross Link Shell 6", ref tab.Logs[34]);
                                ImGui.Checkbox("Cross Link Shell 7", ref tab.Logs[35]);
                                ImGui.Checkbox("Cross Link Shell 8", ref tab.Logs[36]);
                            }
                            if (ImGui.Button("Delete Tab"))
                            {
                                if (EnabledTabs(items) > 1)
                                { tab.Enabled = false; }
                            }
                            ImGui.EndChild();
                            ImGui.TreePop();
                        }

                    }

                }

                if (ImGui.Button("New Tab"))
                {
                    tempTitle = "New";

                    while (CheckDupe(items, tempTitle))
                    { tempTitle += "."; }

                    items.Add(new DynTab(tempTitle, new List<ChatText>(), true));
                    tempTitle = "Title";
                }
                ImGui.SameLine();

                //TODO: Make this not clear the fucking log by using the same reference
                if (ImGui.Button("Save and Close Config"))
                {
                    Configuration.Items = items.ToList();
                    foreach (var strip in Configuration.Items)
                    {
                        strip.Chat = new List<ChatText>();
                    }
                    Configuration.Inject = injectChat;
                    Configuration.Extender = chatWindow;
                    Configuration.Alpha = alpha;
                    this.pluginInterface.SavePluginConfig(Configuration);
                    configWindow = false;
                }
                ImGui.EndChild();
                ImGui.EndTabItem();
            }
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
            if (type == "None") return 0;
            if (type == "Debug") return 1;
            if (type == "Urgent") return 2;
            if (type == "Notice") return 3;
            if (type == "Say") return 4;
            if (type == "Shout") return 5;
            if (type == "TellOutgoing") return 6;
            if (type == "TellIncoming") return 7;
            if (type == "Party") return 8;
            if (type == "Alliance") return 9;
            if (type == "Ls1") return 10;
            if (type == "Ls2") return 11;
            if (type == "Ls3") return 12;
            if (type == "Ls4") return 13;
            if (type == "Ls5") return 14;
            if (type == "Ls6") return 15;
            if (type == "Ls7") return 16;
            if (type == "Ls8") return 17;
            if (type == "FreeCompany") return 18;
            if (type == "NoviceNetwork") return 19;
            if (type == "CustomEmote") return 20;
            if (type == "StandardEmote") return 21;
            if (type == "Yell") return 22;
            if (type == "CrossParty") return 23;
            if (type == "PvPTeam") return 24;
            if (type == "CrossLinkShell1") return 29;
            if (type == "Echo") return 25;
            if (type == "SystemError") return 26;
            if (type == "GatheringSystemMessage") return 27;
            if (type == "RetainerSale") return 28;
            if (type == "CrossLinkShell2") return 30;
            if (type == "CrossLinkShell3") return 31;
            if (type == "CrossLinkShell4") return 32;
            if (type == "CrossLinkShell5") return 33;
            if (type == "CrossLinkShell6") return 34;
            if (type == "CrossLinkShell7") return 35;
            if (type == "CrossLinkShell8") return 36;
            else return 0;
        }

        public string GetChannelName(string type)
        {
            if (type == "None") return "[N]";
            if (type == "Debug") return "[DB]";
            if (type == "Urgent") return "[U]";
            if (type == "Notice") return "[NT]";
            if (type == "Say") return "[S]";
            if (type == "Shout") return "[SH]";
            if (type == "TellOutgoing") return "[TO]";
            if (type == "TellIncoming") return "[TI]";
            if (type == "Party") return "[P]";
            if (type == "Alliance") return "[A]";
            if (type == "Ls1") return "[LS1]";
            if (type == "Ls2") return "[LS2]";
            if (type == "Ls3") return "[LS3]";
            if (type == "Ls4") return "[LS4]";
            if (type == "Ls5") return "[LS5]";
            if (type == "Ls6") return "[LS6]";
            if (type == "Ls7") return "[LS7]";
            if (type == "Ls8") return "[LS8]";
            if (type == "FreeCompany") return "[FC]";
            if (type == "NoviceNetwork") return "[NN]";
            if (type == "CustomEmote") return "[EC]";
            if (type == "StandardEmote") return "[ES]";
            if (type == "Yell") return "[Y]";
            if (type == "CrossParty") return "[CP]";
            if (type == "PvPTeam") return "[PVP]";
            if (type == "CrossLinkShell1") return "[CW1]";
            if (type == "Echo") return "[E]";
            if (type == "SystemError") return "[SE]";
            if (type == "GatheringSystemMessage") return "[G]";
            if (type == "RetainerSale") return "[RS]";
            if (type == "CrossLinkShell2") return "[CW2]";
            if (type == "CrossLinkShell3") return "[CW3]";
            if (type == "CrossLinkShell4") return "[CW4]";
            if (type == "CrossLinkShell5") return "[CW5]";
            if (type == "CrossLinkShell6") return "[CW6]";
            if (type == "CrossLinkShell7") return "[CW7]";
            if (type == "CrossLinkShell8") return "[CW8]";
            else return "[?]";
        }

        private void Chat_OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            var senderName = sender.TextValue;
            List<Payload> payloads = message.Payloads;


            foreach (var tab in items)
            {
                
                if (tab.Logs[ConvertForArray(type.ToString())])
                {
                    ChatText tmp = new ChatText();

                    tmp.Time = GetTime();
                    tmp.Channel = GetChannelName(type.ToString());
                    tmp.Sender = senderName;
                    String rawtext = "";





                    foreach (var payload in payloads)
                    {
                        if(payload.Type == PayloadType.RawText)
                        {
                            rawtext += payload.ToString().Split(new[] { ' ' }, 4)[3];
                        }

                        PluginLog.Log(payload.ToString());

                        /*
                        if(payload.Type == PayloadType.MapLink)
                        {
                            tmp.MapPayloads.Add(payload);
                        }
                        */
                    }

                    tmp.Text = rawtext;

                    String messageString = message.TextValue;
                    String predictedLanguage = Lang(messageString);

                    if (predictedLanguage == language)
                    {
                        Task.Run(() => Tran(type, messageString, senderName));
                    }

                    tab.Chat.Add(tmp);

                    /* Taken out until linkage working
                    foreach(Dalamud.Game.Chat.SeStringHandling.Payloads.MapLinkPayload MapLinks in tmp.MapPayloads)
                    {
                        ChatText map = new ChatText();
                        map.Time = "";
                        map.Channel = "";
                        map.Sender = "[M]";

                        map.Text = MapLinks.XCoord.ToString() + "|";
                        map.Text += MapLinks.RawX.ToString() + "|";
                        MapLinks.Resolve();
                        map.Text += MapLinks.XCoord.ToString() + "|";

                        tab.Chat.Add(map);
                    }
                    */
                    if (tab.AutoScroll == true)
                    {
                        tab.Scroll = true;
                    }
                }
                
            }
        }

        public string GetTime()
        {
            string temp = "[";
            if (DateTime.Now.ToString("%h").Length == 1) { temp += "0"; }
            temp += DateTime.Now.ToString("%h"+":");
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
                PrintChat(type, senderName, "<<" + output + ">>");
            }

            foreach (var tab in items)
            {

                if (tab.Logs[ConvertForArray(type.ToString())] && tab.Config[2])
                {
                    ChatText tmp = new ChatText();

                    tmp.Time = GetTime();
                    tmp.Channel= GetChannelName(type.ToString());
                    tmp.Sender = senderName;
                    tmp.Text = "<<" + output + ">>";

                    tab.Chat.Add(tmp);
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

        public class TabBase
        {
            public string Title;
            public List<ChatText> Chat;
            public bool Enabled;

            public bool[] Logs = {
                false, false, false, false, false,
                false, false, false, false, false,
                false, false, false, false, false,
                false, false, false, false, false,
                false, false, false, false, false,
                false, false, false, false, false,
                false, false, false, false, false,
                false, false, false };

            // 0 = Timestamp
            // 1 = Channel
            // 2 = Translate
            public bool[] Config = { false, false, true };
            public bool AutoScroll = true;
            public bool Scroll = false;
        }

        bool CheckDupe(List<TabBase> items, string title)
        {
            foreach (var tab in items)
            {
                if (title == tab.Title) { return true; }
            }
            return false;
        }

        public class ChatText
        {
            public string Time;
            public string Channel;
            public string Sender;
            public string Text;
            public bool Selected;
            public List<Payload> MapPayloads = new List<Payload>();
        }

        public class DynTab : TabBase
        {
            public DynTab(string title, List<ChatText> chat, bool enabled)
            {
                Title = title;
                Chat = chat;
                Enabled = enabled;
            }

        }

    }

}
