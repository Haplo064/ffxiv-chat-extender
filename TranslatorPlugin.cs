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
using System.Text.RegularExpressions;


namespace DalamudPlugin
{
    public class TranslatorPlugin : IDalamudPlugin
    {
        // Dalamud Plugin

        public string Name => "Translator Plugin";
        private DalamudPluginInterface pluginInterface;
        private bool loadWindow = false;
        private bool chatWindow = false;
        private string chatText = "";
        //Globals
        public bool injectChat = true;
        public int translator = 1;         //1=Google,2=Yandex
        public string language = "jpn";
        public List<TabBase> items = new List<TabBase>();
        public uint bufSize = 24;
        public string tempTitle = "Title";

        //Google Translate
        private static readonly GoogleTranslator TransG = new GoogleTranslator();
        // Yandex Translate
        private YandexTranslate.Translator TransY = new YandexTranslate.Translator();
        // NCat
        public static RankedLanguageIdentifierFactory factory = new RankedLanguageIdentifierFactory();
        public static RankedLanguageIdentifier identifier = factory.Load(Path.Combine(AssemblyDirectory, "Core14.profile.xml"));

        public TranslatePluginConfiguration Configuration;

        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = typeof(TranslatorPlugin).Assembly.CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            // Initializing plugin, hooking into chat.
            this.pluginInterface = pluginInterface;


            Configuration = pluginInterface.GetPluginConfig() as TranslatePluginConfiguration ?? new TranslatePluginConfiguration();

            //TODO Clean up disabled tabs before load
            Serilog.Log.Error("1");
            //items.Add(new DynTab("Tab", "", true));
            Serilog.Log.Error("2");

            if (Configuration.Items == null)
                {
                    Serilog.Log.Error("Null");
                    items.Add(new DynTab("XXX", "", true));
                }
            else
            {
                Serilog.Log.Error("Not Null");
                if (Configuration.Items.Count == 0)
                {
                    Serilog.Log.Error("But Empty");
                    items.Add(new DynTab("YYY", "", true));
                }
                else
                {
                    Serilog.Log.Error("Not Empty");
                    items = Configuration.Items;
                }
            }


            TransY.Make("https://translate.yandex.net/api/v1.5/tr.json/translate", Configuration.YandexKey);

            // Set up command handlers
            this.pluginInterface.CommandManager.AddHandler("/trn", new CommandInfo(OnTranslateCommand)
            {
                HelpMessage = "Configure Translator Engine of Translator. Usage: /trn t <#> (1=Google, 2=Yandex)"
            });

            this.pluginInterface.Framework.Gui.Chat.OnChatMessage += Chat_OnChatMessage;
            this.pluginInterface.UiBuilder.OnBuildUi += ChatUI;
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
                String[] bits = arguments.Split(' ');
                if (bits[0] == "e" | bits[0] == "engine")
                {
                    if (bits[1] == "1" | bits[1] == "google") {translator = 1; PrintChat(XivChatType.Notice, "[TRN]", "Translator set to Google"); return; }
                    else if (bits[1] == "2"| bits[1] == "yandex") {translator = 2; PrintChat(XivChatType.Notice, "[TRN]", "Translator set to Yandex"); return; }
                    else {this.pluginInterface.Framework.Gui.Chat.PrintError("No valid setting supplied. 1=Google, 2=Yandex"); return; }
                }

                else if (bits[0] == "i" | bits[0] == "inject")
                {
                    if (bits[1] == "1" | bits[1] == "true" | bits[1] == "on") { injectChat = true; PrintChat(XivChatType.Notice, "[TRN]", "Chat injection on"); return; }
                    else if (bits[1] == "0" | bits[1] == "false" | bits[1] == "off") { injectChat = false; PrintChat(XivChatType.Notice, "[TRN]", "Chat injection off"); return; }
                    else { this.pluginInterface.Framework.Gui.Chat.PrintError("No valid setting supplied. Try: 1/0, on/off, true/false"); return; }
                }

                else if  (bits[0] == "w" | bits[0] == "window")
                {
                    if (this.chatWindow)
                    {
                        this.chatWindow = false;
                    }
                    else
                    {
                        this.chatWindow = true;
                    }
                    PrintChat(XivChatType.Notice, "[TRN]", "Opened Chat Window");
                    return;
                }

                else if (bits[0] == "h" | bits[0] == "help")
                {PrintChat(XivChatType.Notice, "[TRN]", "Avaliable options:\n[e/engine] <1/google 2/yandex>\n[i/inject] <1/true/on 0/false/off>\n[w/window]"); return; }
                
                else
                { this.pluginInterface.Framework.Gui.Chat.PrintError("No valid command supplied. Try 'h/help'"); return; }
            }



        }

        public class TranslatePluginConfiguration : IPluginConfiguration
        {
            public int Version { get; set; } = 0;
            public string YandexKey { get; set; }
            public List<TabBase> Items { get; set; }
        }

        private void ChatUI()
        {
            if (loadWindow)
            {
                if (ImGui.Begin("Translator", ref loadWindow))
                {
                    ImGui.Text("-=Translator Plugin Loaded=-");
                    ImGui.Text("============================");
                    ImGui.Text("Try '/trn h' for help!");
                }
                ImGui.End();
            }

            if (chatWindow)
            {
                ImGui.SetNextWindowSize(new Num.Vector2(200, 100), ImGuiCond.FirstUseEver);
                ImGui.SetNextWindowBgAlpha(0.2f);
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
                                ImGui.Text($"{tab.Text}");
                                ImGui.EndChild();
                                ImGui.EndTabItem();
                            }
                        }
                    }

                    if (ImGui.BeginTabItem("+"))
                    {
                        ImGui.Text("Config");

                        foreach (var tab in items)
                        {
                            if (tab.Enabled)
                            {
                                if (ImGui.TreeNode(tab.Title))
                                {
                                    float footer = (ImGui.GetStyle().ItemSpacing.Y) / 2 + ImGui.GetFrameHeightWithSpacing();
                                    ImGui.BeginChild("scrolling", new Num.Vector2(0, -footer), false);
                                    ImGui.Text("Change Name of Tab");
                                    ImGui.InputText("Tab Name", ref tempTitle, bufSize);
                                    if (ImGui.Button("Set Tab Title"))
                                    {
                                        if (tempTitle.Length == 0) { tempTitle += "."; }

                                        while (CheckDupe(items, tempTitle))
                                        { tempTitle += "."; }

                                        tab.Title = tempTitle;
                                        tempTitle = "Title";
                                    }
                                    if (ImGui.TreeNode(tab.Title + " |Display"))
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
                                        tab.Enabled = false;
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

                            items.Add(new DynTab(tempTitle, "", true));
                            tempTitle = "Title";
                        }
                        if (ImGui.Button("Save Layout"))
                        {
                            Configuration.Items = items;
                        }
                        ImGui.EndTabItem();
                    }
                    ImGui.EndTabBar();
                    ImGui.End();
                }
            }
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

        private void Chat_OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            String messageString = message.TextValue;
            String temp = Lang(messageString);
            var senderName = sender.TextValue;
            if (temp == language)
            {
                Task.Run(() => Tran(type, messageString, senderName));
            }

            Serilog.Log.Error(((int)type).ToString() + " " + type);

            foreach (var tab in items)
            {
                
                if (tab.Logs[ConvertForArray(type.ToString())])
                {
                    tab.Text += senderName + ": " + Regex.Replace(Regex.Replace(Regex.Escape(messageString), "\\ "," "),"\\.",".") +"\n";
                }
                
            }
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
                PrintChat(type, "[TRN] " + senderName, output);
            }
            this.chatText += "\n"+senderName+": "+output;
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
            pluginInterface.CommandManager.RemoveHandler("/trn");
        }

        public class TabBase
        {
            public string Title;
            public string Text;
            public bool Enabled;
            //public abstract void Render();
            public bool[] Logs = {
                false, false, false, false, false,
                false, false, false, false, false,
                false, false, false, false, false,
                false, false, false, false, false,
                false, false, false, false, false,
                false, false, false, false, false,
                false, false, false, false, false,
                false, false };
        }

        bool CheckDupe(List<TabBase> items, string title)
        {
            foreach (var tab in items)
            {
                if (title == tab.Title) { return true; }
            }
            return false;
        }

        public class DynTab : TabBase
        {
            public DynTab(string title, string text, bool enabled)
            {
                Title = title;
                Text = text;
                Enabled = enabled;
            }
            /*
            public override void Render()
            {
                // do shit
            }
            */
        }



    }
}
