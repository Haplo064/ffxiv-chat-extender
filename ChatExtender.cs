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

//TODO
//Add select+copy?
//Add spacing config
//Font? - Likely change to gothic
//Add in handling for quick-translate stuff
//Add in text finding
//Add in text higlighting?
//Add in Yandex Key via config
//Add in support for more than japanese?
//Add in config for language selections?

//Add write to file <<DONE>>
//Add locking in place <<DONE>>
//Add clickthrough <<DONE>>
//Add customizable translate surrounds <<DONE>>
//Add colour configs <<DONE>>
//Fix up missing chat text <<DONE>>
//Add Custom wrapper <<DONE>>


namespace DalamudPlugin
{
    public class ChatExtenderPlugin : IDalamudPlugin
    {
        // Dalamud Plugin

        public string Name => "Chat Extender";
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

        public string lTr = "<<";
        public string rTr = ">>";

        static bool no_titlebar = true;
        static bool no_scrollbar = false;
        static bool no_menu = true;
        static bool no_move = false;
        static bool no_resize = false;
        static bool no_collapse = true;
        static bool no_close = true;
        static bool no_nav = false;
        static bool no_mouse = false;

        static string pathString = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+@"\XIVLauncher\installedPlugins\ChatExtender\Logs\";

        public Num.Vector4[] logColour =
        {
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255)
        };

        public Num.Vector4[] chanColour =
        {
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255)
        };

        public String[] Channels =
        {
            "None","Debug","Urgent","Notice","Say","Shout","Tell Outgoing","Tell Incoming","Party","Alliance","Ls 1","Ls 2","Ls 3","Ls 4","Ls 5","Ls 6","Ls 7","Ls 8",
            "Free Company","Novice Network","Custom Emote","Standard Emote","Yell","Cross Party","PvP Team","Echo","System Error","Gathering System Message","Retainer Sale",
            "Cross LinkShell 1","Cross LinkShell 2","Cross LinkShell 3","Cross LinkShell 4","Cross LinkShell 5","Cross LinkShell 6","Cross LinkShell 7","Cross LinkShell 8"
        };

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

            try
            { rTr = Configuration.RTr.ToString(); }
            catch (Exception)
            {
                PluginLog.LogError("Failed to Load right Translate Surround!");
            }

            try
            { lTr = Configuration.LTr.ToString(); }
            catch (Exception)
            {
                PluginLog.LogError("Failed to Load left Translate Surround!");
            }

            try
            {chanColour = Configuration.ChanColour.ToArray();}
            catch (Exception)
            {PluginLog.LogError("No ChanColour to load!");}

            try
            {logColour = Configuration.LogColour.ToArray(); }
            catch (Exception)
            { PluginLog.LogError("No LogColour to load!"); }

            try
            {injectChat = Configuration.Inject;}
            catch (Exception)
            {
                PluginLog.LogError("Failed to Load Inject Status!");
                injectChat = false;
            }

            try
            { translator = Configuration.Translator; }
            catch (Exception)
            {
                PluginLog.LogError("Failed to Load Translator Choice!");
                translator = 1;
            }

            try
            { yandex = Configuration.YandexKey.ToString(); }
            catch (Exception)
            {
                PluginLog.LogError("Failed to Load Yandex Key!");
                yandex = "";
            }

            try
            { chatWindow = Configuration.Extender; }
            catch (Exception)
            {
                PluginLog.LogError("Failed to Load Extender Choice!");
                chatWindow = false;
            }

            try
            { alpha = Configuration.Alpha; }
            catch (Exception)
            {
                PluginLog.LogError("Failed to Load Alpha!");
                alpha = 0.2f;
            }

            try
            { no_mouse = Configuration.NoMouse; }
            catch (Exception)
            {
                PluginLog.LogError("Failed to Load NoMouse Config!");
                no_mouse = false;
            }

            try
            { no_move = Configuration.NoMove; }
            catch (Exception)
            {
                PluginLog.LogError("Failed to Load NoMove Config!");
                no_move = false;
            }

            try
            { no_resize = Configuration.NoResize; }
            catch (Exception)
            {
                PluginLog.LogError("Failed to Load NoMove Config!");
                no_resize = false;
            }

            try
            { no_mouse = Configuration.NoMouse; }
            catch (Exception)
            {
                PluginLog.LogError("Failed to Load NoMouse Config!");
                no_mouse = false;
            }



            //TODO: try/catch this?
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
                    //Serilog.Log.Information("Normal DynTab List");
                    items = Configuration.Items.ToList();
                }

            }

            if (items[0].Config.Length==3)
            {
                foreach(TabBase item in items)
                {
                    bool[] temp = { false, false, false, false, false, false, false, false, false, false };
                    temp[0] = item.Config[0];
                    temp[1] = item.Config[1];
                    temp[2] = item.Config[2];
                    item.Config = temp;
                }
            }

            SaveConfig();

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

        private List<TabBase> CopyAndStripItems(List<TabBase> items)
        {
            List<TabBase> clone = new List<TabBase>();
            foreach(TabBase tabs in items)
            {
                TabBase babyClone = new TabBase();
                babyClone.AutoScroll = tabs.AutoScroll;
                babyClone.Chat = new List<ChatText>();
                babyClone.Config = tabs.Config.ToArray();
                babyClone.Enabled = tabs.Enabled;
                babyClone.Logs = tabs.Logs.ToArray();
                babyClone.Scroll = tabs.Scroll;
                babyClone.Title = tabs.Title.ToString();
                clone.Add(babyClone);
            }
            return clone;
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
                    if (bits[1] == "1" | bits[1] == "google") { translator = 1; PrintChat(XivChatType.Notice, "[CHT]", "Translator set to Google"); return; }
                    else if (bits[1] == "2" | bits[1] == "yandex") { translator = 2; PrintChat(XivChatType.Notice, "[CHT]", "Translator set to Yandex"); return; }
                    else { this.pluginInterface.Framework.Gui.Chat.PrintError("No valid setting supplied. 1=Google, 2=Yandex"); return; }
                }

                else if (bits[0] == "w" | bits[0] == "window")
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

                else
                { this.pluginInterface.Framework.Gui.Chat.PrintError("No valid command supplied."); return; }
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
            public Num.Vector4[] ChanColour { get; set; }
            public Num.Vector4[] LogColour { get; set; }
            public string LTr { get; set; }
            public string RTr { get; set; }
            public bool NoMouse { get; set; }
            public bool NoMove { get; set; }
            public bool NoResize { get; set; }
        }

        private void ChatUI()
        {
            ImGuiWindowFlags chat_window_flags = 0;
            ImGuiWindowFlags chat_sub_window_flags = 0;

            if (no_titlebar) chat_window_flags |= ImGuiWindowFlags.NoTitleBar;
            if (no_scrollbar) chat_window_flags |= ImGuiWindowFlags.NoScrollbar;
            if (!no_menu) chat_window_flags |= ImGuiWindowFlags.MenuBar;
            if (no_move) chat_window_flags |= ImGuiWindowFlags.NoMove;
            if (no_resize) chat_window_flags |= ImGuiWindowFlags.NoResize;
            if (no_collapse) chat_window_flags |= ImGuiWindowFlags.NoCollapse;
            if (no_nav) chat_window_flags |= ImGuiWindowFlags.NoNav;
            if (no_mouse) { chat_window_flags |= ImGuiWindowFlags.NoMouseInputs; chat_sub_window_flags |= ImGuiWindowFlags.NoMouseInputs; }
 
            if (chatWindow)
            {
                ImGui.SetNextWindowSize(new Num.Vector2(200, 100), ImGuiCond.FirstUseEver);
                ImGui.SetNextWindowBgAlpha(alpha);
                ImGui.Begin("Another Window", ref chatWindow, chat_window_flags);
                ImGuiTabBarFlags tab_bar_flags = ImGuiTabBarFlags.None;
                if (ImGui.BeginTabBar("Tabs", tab_bar_flags))
                {
                    int loop = 0;
                    foreach (var tab in items)
                    {
                        if (tab.Enabled)
                        {
                            if (ImGui.BeginTabItem(tab.Title))
                            {
                                float footer = (ImGui.GetStyle().ItemSpacing.Y) / 2 + ImGui.GetFrameHeightWithSpacing();
                                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Num.Vector2(4, 0));
                                ImGui.BeginChild("scrolling", new Num.Vector2(0, -footer), false, chat_sub_window_flags);


                                foreach (ChatText line in tab.Chat)
                                {

                                    if (tab.Config[0]) { ImGui.TextColored(chanColour[ConvertForArray(line.Channel)], line.Time + " "); ImGui.SameLine(); }
                                    if (tab.Config[1]) { ImGui.TextColored(chanColour[ConvertForArray(line.Channel)], line.ChannelShort + " "); ImGui.SameLine(); }
                                    if (line.Sender.Length > 0) { ImGui.TextColored(chanColour[ConvertForArray(line.Channel)], line.Sender + ":"); ImGui.SameLine(); }

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
                                                //MAP HANDLING WILL GO HERE
                                                PluginLog.Log("Clicked on " + textTypes.Payload.ToString());
                                            }
                                        }

                                        if (count < (line.Text.Count - 1))
                                        {
                                            ImGui.SameLine();
                                            count++;
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
                                ImGui.EndTabItem();
                            }
                        }
                        loop++;
                    }
                    ImGui.EndTabBar();
                    ImGui.End();
                }
            }

            if (configWindow)
            {
                ImGui.SetNextWindowSize(new Num.Vector2(300, 500), ImGuiCond.FirstUseEver);
                ImGui.Begin("Chat Config", ref configWindow);
                ImGuiTabBarFlags tab_bar_flags = ImGuiTabBarFlags.None;
                if (ImGui.BeginTabBar("Tabs", tab_bar_flags))
                {
                    if (ImGui.BeginTabItem("Main"))
                    {
                        float footer1 = (ImGui.GetStyle().ItemSpacing.Y) / 2 + ImGui.GetFrameHeightWithSpacing();
                        ImGui.BeginChild("scrolling", new Num.Vector2(0, -footer1), false);
                        ImGui.Text("");
                        ImGui.Columns(3);
                        ImGui.Checkbox("Inject Translation", ref injectChat); ImGui.NextColumn();
                        ImGui.Checkbox("Chat Extender", ref chatWindow); ImGui.NextColumn();
                        ImGui.Text(""); ImGui.NextColumn();
                        ImGui.Checkbox("Lock Position", ref no_move); ImGui.NextColumn();
                        ImGui.Checkbox("Lock Size", ref no_resize); ImGui.NextColumn();
                        ImGui.Checkbox("ClickThrough", ref no_mouse); ImGui.NextColumn();

                        ImGui.Columns(1);
                        ImGui.Text("Surrounds of Translated text");
                        ImGui.PushItemWidth(24);
                        ImGui.InputText("##Left", ref lTr, 3); ImGui.SameLine();
                        ImGui.PopItemWidth();
                        ImGui.Text("Translation"); ImGui.SameLine();
                        ImGui.PushItemWidth(24);
                        ImGui.InputText("##Right", ref rTr, 3);
                        ImGui.PopItemWidth();
                        ImGui.Text("");
                        
                        ImGui.Text("");
                        ImGui.SliderFloat("Chat Extender Alpha", ref alpha, 0.001f, 0.999f);
                        ImGui.Text("");

                        if (ImGui.TreeNode("Colours"))
                        {
                            ImGui.Columns(3);
                            ImGui.Text("Example"); ImGui.NextColumn();
                            ImGui.Text("Channel"); ImGui.NextColumn();
                            ImGui.Text("Text"); ImGui.NextColumn();
                            for (int i = 0; i < 37; i++)
                            {
                                ImGui.TextColored(chanColour[i], "[" + Channels[i] + "]"); ImGui.SameLine(); ImGui.TextColored(logColour[i], "Text"); ImGui.NextColumn();
                                ImGui.ColorEdit4(Channels[i] + " Colour1", ref chanColour[i], ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel); ImGui.NextColumn();
                                ImGui.ColorEdit4(Channels[i] + " Colour2", ref logColour[i], ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel); ImGui.NextColumn();
                            }
                            ImGui.TreePop();

                        }

                        ImGui.Columns(1);

                        if (ImGui.Button("Add New Tab"))
                        {
                            tempTitle = "New";

                            while (CheckDupe(items, tempTitle))
                            { tempTitle += "."; }

                            items.Add(new DynTab(tempTitle, new List<ChatText>(), true));
                            tempTitle = "Title";
                        }
                        ImGui.Text("");

                        if (ImGui.Button("Save and Close Config"))
                        {
                            SaveConfig();

                            configWindow = false;
                        }
                        ImGui.EndChild();
                        ImGui.EndTabItem();
                    }


                    if (ImGui.BeginTabItem("Tabs"))
                    {
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
                                    ImGui.Checkbox("Time Stamp", ref tab.Config[0]);
                                    ImGui.SameLine();
                                    ImGui.Checkbox("Channel", ref tab.Config[1]);
                                    ImGui.SameLine();
                                    ImGui.Checkbox("Translate", ref tab.Config[2]);
                                    ImGui.Checkbox("AutoScroll", ref tab.AutoScroll);
                                    ImGui.SameLine();
                                    ImGui.Checkbox("Save to file", ref tab.Config[3]);

                                    //TODO: Add a confirm prompt
                                    if (ImGui.Button("Delete Tab"))
                                    {
                                        if (EnabledTabs(items) > 1)
                                        { tab.Enabled = false; }
                                    }


                                    ImGui.Columns(2);
                                    ImGui.Text("Enable Channels"); ImGui.NextColumn();
                                    ImGui.Text(""); ImGui.NextColumn();

                                    for (int i = 0; i < 37; i++)
                                    {
                                        ImGui.PushStyleColor(ImGuiCol.Text, chanColour[i]);
                                        ImGui.Checkbox("[" + Channels[i] + "]", ref tab.Logs[i]); ImGui.NextColumn();
                                        ImGui.PopStyleColor();
                                    }
                                    ImGui.Columns(1);
                                    ImGui.EndChild();


                                }
                            }
                        }
                        ImGui.EndTabItem();
                    }

                }
                ImGui.EndTabBar();
                ImGui.EndChild();
            }
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
            Configuration.NoMove = no_move;
            Configuration.NoResize = no_resize;
            this.pluginInterface.SavePluginConfig(Configuration);
        }

        public void Wrap(String input)
        {
            String[] inputArray = input.Split(' ');

            int count = 0;
            foreach (String splits in inputArray)
            {
                if (ImGui.GetContentRegionAvail().X - 5 - ImGui.CalcTextSize(splits).X < 0) { ImGui.Text(""); }
                ImGui.Text(splits);

                if (count < (inputArray.Length - 1))
                {
                    ImGui.SameLine();
                    count++;
                }
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

        //TODO Shrink this with Channels[]
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
                    tmp.ChannelShort = GetChannelName(type.ToString());
                    tmp.Channel = type.ToString();
                    tmp.Sender = senderName;
                    tmp.ChannelColour = ConvertForArray(type.ToString());
                    List<TextTypes> rawtext = new List<TextTypes>();

                    int replace = 0;
                    Payload payloader = null;
                    PayloadType payloadType = PayloadType.RawText;
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
                                    wrangler.Payload = payload;
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

                        PluginLog.Log(payload.ToString());

                    }

                    tmp.Text = rawtext;

                    String messageString = message.TextValue;
                    String predictedLanguage = Lang(messageString);

                    if (predictedLanguage == language)
                    {
                        Task.Run(() => Tran(type, messageString, senderName));
                    }

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
                }

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
            foreach(TextTypes texts in textTypes)
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
            // 3 = Write
            // 4-9  = placeholders
            public bool[] Config = { false, false, false, false,false,false,false,false,false,false };
            public bool AutoScroll = true;
            public bool Scroll = false;
        }

        public class TextTypes
        {
            public string Text;
            public PayloadType Type;
            public Payload Payload;
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
            public string ChannelShort;
            public string Sender;
            public List<TextTypes> Text = new List<TextTypes>();
            public bool Selected;
            public int ChannelColour;
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
