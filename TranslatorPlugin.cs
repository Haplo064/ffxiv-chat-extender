using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.Chat;
using Dalamud.Game.Chat.SeStringHandling;
using Dalamud.Plugin;
using IvanAkcheurov.NTextCat.Lib;
using Serilog;
using Yandex;
using ImGuiNET;

namespace DalamudPlugin
{
    public class TranslatorPlugin : IDalamudPlugin
    {
        // Dalamud Plugin

        public string Name => "Translator Plugin";
        private DalamudPluginInterface pluginInterface;
        private bool drawWindow = true;
        //Globals
        bool injectChat = true;
        // Google Translate
        YandexTranslate.Translator Trans = new YandexTranslate.Translator();
        // NCat
        public static RankedLanguageIdentifierFactory factory = new RankedLanguageIdentifierFactory();
        public static RankedLanguageIdentifier identifier = factory.Load(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\XIVLauncher\\plugins\\Translator\\Core14.profile.xml");

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            // Initializing plugin, hooking into chat.
            this.pluginInterface = pluginInterface;
            pluginInterface.Framework.Gui.Chat.OnChatMessage += Chat_OnChatMessage;
            pluginInterface.UiBuilder.OnBuildUi += ShowUI;
            Trans.Make("https://translate.yandex.net/api/v1.5/tr.json/translate", "trnsl.1.1.20200209T032001Z.c602678c5616cc9a.7142796a81b55a5b7fde88e906a49b7da90f67d8");
            
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
                }
                ImGui.End();
            }
        }

            private void Chat_OnChatMessage(XivChatType type, uint senderId, ref Dalamud.Game.Internal.Libc.StdString sender, ref Dalamud.Game.Internal.Libc.StdString message, ref bool isHandled)
        {
            String messageString = message.Value;
            String temp = Lang(messageString);
            
            if (temp == "jpn")
            {
                var senderName = SeString.Parse(sender.RawData).TextValue;
                Task.Run(() => Tran(type, messageString, senderName));
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
            //Log.Information("Just before sending to yandex...");
            var text = Trans.Translate(apple, "en");
            //Log.Information("Response!" + text);
            return text.Text[0];
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
