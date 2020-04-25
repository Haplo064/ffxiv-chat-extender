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
            public bool NoMouse2 { get; set; }
            public bool NoMove { get; set; }
            public bool NoResize { get; set; }
            public bool NoScrollBar { get; set; }
            public int? Space_Hor { get; set; }
            public int? Space_Ver { get; set; }
            public Num.Vector4 TimeColour { get; set; }
            public Num.Vector4 NameColour { get; set; }
            public Highlighter High { get; set; }
            public String[] Chan { get; set; }
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
                false, false, false, false, false,
                false, false, false, false, false,
                false, false, false, false, false,
                false, false, false, false, false,
                false, false, false, false, false,
                false, false, false, false
            };

            public bool[] Chans = {
                true, true, true, true, true,
                true, true, true, true, true,
                true, true, true, true, true,
                true, true, true, true, true,
                true, true, true, true, true,
                true, true, true, true, true,
                true, true, true, true, true,
                true, true, true, true, true,
                true, true, true, true, true,
                true, true, true, true, true,
                true, true, true, true, true,
                true, true, true, true, true,
                true, true, true, true
            };

            // 0 = Timestamp
            // 1 = Channel
            // 2 = Translate
            // 3 = Write
            // 4-9  = placeholders
            public bool[] Config = { false, false, false, false,false,false,false,false,false,false };
            public bool AutoScroll = true;
            public bool Scroll = false;
            public string Filter = "";
            public bool FilterOn = false;
            public bool msg = false;
            public bool sel = false;
        }

        public class TextTypes
        {
            public string Text;
            public PayloadType Type;
            public Payload Payload;
        }

        public class Highlighter
        {
            public string[] highlights = {"words,to,highlight"};
            public int htA = 120;
            public int htB = 255;
            public int htG = 255;
            public int htR = 255;
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
