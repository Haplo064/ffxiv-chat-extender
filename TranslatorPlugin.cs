using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.Chat;
using Dalamud.Plugin;
using GoogleTranslateFreeApi;
using IvanAkcheurov.NTextCat.Lib;


namespace DalamudPlugin
{
    public class TranslatorPlugin : IDalamudPlugin
    {
        public string Name => "Translator Plugin";

        private DalamudPluginInterface pluginInterface;
        private static readonly GoogleTranslator Trans = new GoogleTranslator();

        public static IvanAkcheurov.NTextCat.Lib.RankedLanguageIdentifierFactory factory = new RankedLanguageIdentifierFactory();
        public static IvanAkcheurov.NTextCat.Lib.RankedLanguageIdentifier identifier = factory.Load(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\XIVLauncher\\plugins\\Translator\\Core14.profile.xml");

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
            pluginInterface.Framework.Gui.Chat.OnChatMessage += Chat_OnChatMessage;

        }

        private void Chat_OnChatMessage(XivChatType type, uint senderId, ref Dalamud.Game.Internal.Libc.StdString sender, ref Dalamud.Game.Internal.Libc.StdString message, ref bool isHandled)
        {
            var messageString = message.Value;
            if (lang(messageString) == "jpn")
            {
                var senderName = sender.ToString();
                Task.Run(() => tran(type, messageString, senderName));
            }
        }

        public void tran(XivChatType type, string messageString, string senderName)
        {
            var chat = new XivChatEntry();
            chat.Type = type;
            chat.Name = "[TRN] "+senderName;
            chat.MessageBytes = Encoding.UTF8.GetBytes(translate(messageString));          
            pluginInterface.Framework.Gui.Chat.PrintChat(chat);
        }

        public static string translate(string apple)
        {
            var text = Trans.TranslateLiteAsync(apple, Language.Auto, Language.English).GetAwaiter().GetResult();
            return text.MergedTranslation;
        }

        public static string lang(string banana)
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
