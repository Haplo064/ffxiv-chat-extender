using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace DalamudPlugin
{
    public class TranslatorPlugin : IDalamudPlugin
    {
        // Dalamud Plugin

        public string Name => "Translator Plugin";
        private DalamudPluginInterface pluginInterface;
        private bool loadWindow = true;
        private bool chatWindow = false;
        private string chatText = "";
        //Globals
        public bool injectChat = true;
        public int translator = 1;         //1=Google,2=Yandex
        public string language = "jpn";

        //Google Translate
        private static readonly GoogleTranslator TransG = new GoogleTranslator();
        // Yandex Translate
        private YandexTranslate.Translator TransY = new YandexTranslate.Translator();
        // NCat
        public static RankedLanguageIdentifierFactory factory = new RankedLanguageIdentifierFactory();
        public static RankedLanguageIdentifier identifier = factory.Load(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\XIVLauncher\\plugins\\Translator\\Core14.profile.xml");

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            // Initializing plugin, hooking into chat.
            this.pluginInterface = pluginInterface;
            pluginInterface.Framework.Gui.Chat.OnChatMessage += Chat_OnChatMessage;
            pluginInterface.UiBuilder.OnBuildUi += ChatUI;

            Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\XIVLauncher\\plugins\\Translator\\config.json"));

            TransY.Make("https://translate.yandex.net/api/v1.5/tr.json/translate", config.YandexKey);

            // Set up command handlers
            pluginInterface.CommandManager.AddHandler("/trn", new CommandInfo(OnTranslateCommand)
            {
                HelpMessage = "Configure Translator Engine of Translator. Usage: /trn t <#> (1=Google, 2=Yandex)"
            });

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
                {this.chatWindow = true; PrintChat(XivChatType.Notice, "[TRN]", "Opened Chat Window"); return; }

                else if (bits[0] == "h" | bits[0] == "help")
                {PrintChat(XivChatType.Notice, "[TRN]", "Avaliable options:\n[e/engine] <1/google 2/yandex>\n[i/inject] <1/true/on 0/false/off>\n[w/window]"); return; }
                
                else
                { this.pluginInterface.Framework.Gui.Chat.PrintError("No valid command supplied. Try 'h/help'"); return; }
            }



        }

        public class Config
        {
            public string YandexKey { get; set; }
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
                if (ImGui.Begin("Chat Log", ref chatWindow))
                {
                    ImGui.TextUnformatted(this.chatText);
                }
                ImGui.End();
            }          
        }




        private void Chat_OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            String messageString = message.TextValue;
            String temp = Lang(messageString);
            
            if (temp == language)
            {
                var senderName = sender.TextValue;
                Task.Run(() => Tran(type, messageString, senderName));
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





    }
}
