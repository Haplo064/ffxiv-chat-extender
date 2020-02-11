using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.Chat;
using Dalamud.Game.Chat.SeStringHandling;
using Dalamud.Plugin;
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
        private bool drawWindow = true;
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
            pluginInterface.UiBuilder.OnBuildUi += ShowUI;

            Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\XIVLauncher\\plugins\\Translator\\config.json"));

            TransY.Make("https://translate.yandex.net/api/v1.5/tr.json/translate", config.YandexKey);
            
        }

        public class Config
        {
            public string YandexKey { get; set; }
        }

        private void ShowUI()
        {
            // use ImGui.NET things here
            if (drawWindow)
            {
                if (ImGui.Begin("Translator", ref drawWindow))
                {
                    ImGui.Text("Powered by Yandex.Translate");
                    ImGui.Text("http://translate.yandex.com");
                    ImGui.Text("===========================");
                    ImGui.Text("(Feel free to close this!)");
                }
                ImGui.End();
            }
        }

            private void Chat_OnChatMessage(XivChatType type, uint senderId, ref Dalamud.Game.Internal.Libc.StdString sender, ref Dalamud.Game.Internal.Libc.StdString message, ref bool isHandled)
        {
            String messageString = message.Value;
            String temp = Lang(messageString);
            
            if (temp == language)
            {
                var senderName = SeString.Parse(sender.RawData).TextValue;
                Task.Run(() => Tran(type, messageString, senderName));
            }

            if (messageString == "!trn t 1")
            {
                translator = 1;
                var chat = new XivChatEntry
                { Type = XivChatType.Notice, Name = "[TRN]", MessageBytes = Encoding.UTF8.GetBytes("Translator set to Google") };
                pluginInterface.Framework.Gui.Chat.PrintChat(chat);
            }

            if (messageString == "!trn t 2")
            {
                translator = 2;
                var chat = new XivChatEntry
                { Type = XivChatType.Notice, Name = "[TRN]", MessageBytes = Encoding.UTF8.GetBytes("Translator set to Yandex") };
                pluginInterface.Framework.Gui.Chat.PrintChat(chat);
            }

        }

        public void Tran(XivChatType type, string messageString, string senderName)
        {

            string output = Translate(messageString);

            if (injectChat == true)
            {
                var chat = new XivChatEntry
                { Type = type, Name = "[TRN] " + senderName, MessageBytes = Encoding.UTF8.GetBytes(output) };
                pluginInterface.Framework.Gui.Chat.PrintChat(chat);
            }

            //TODO
            /*if (drawWindow)
            {
                if (ImGui.Begin("Translator", ref drawWindow))
                {
                    ImGui.Text(type.ToString() + " : " + senderName + " : " + output);
                }
                ImGui.End();
            }*/
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
        }





    }
}
