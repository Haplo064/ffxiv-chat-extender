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
using Dalamud.Interface;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Runtime.Serialization.Formatters.Binary;
using System.Numerics;

//TODO
//https://github.com/Haplo064/ffxiv-chat-extender/projects/2

namespace DalamudPlugin
{
    public partial class ChatExtenderPlugin : IDalamudPlugin
    {

        // Dalamud Plugin
        public string Name => "Chat Extender";
        private DalamudPluginInterface pluginInterface;
        private bool configWindow = false;
        private bool hideWithChat = true;

        public static ChatExtenderPluginConfiguration config;

        public static Vector2 windowSize = new Vector2(100, 100);
        public static TabBase activeTab;
        public static ConcurrentQueue<TextLogEntry> chatBuffer = new ConcurrentQueue<TextLogEntry>();
        public static List<TabBase> tabs = new List<TabBase>();
        public static ImFontPtr font;
        public static ImFontPtr outlineFont;

        public uint bufSize = 24;
        public string tempTitle = "Title";
        public string tempHigh = "words,to,highlight";
        public string fontFilePath = "";

        public bool nulled = false;
        public bool debug = false;
        public bool outputAllJsons = false;
        public bool outputErrorJsons = false;

        static bool no_titlebar = true;
        static bool no_menu = true;
        static bool no_collapse = true;
        static bool no_nav = false;
        static bool flickback = false;

        public static bool fontsLoaded = false;
        
        static string appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        static string dllPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public static Dictionary<int, ChannelSettings> ChannelSettingsTable = new Dictionary<int, ChannelSettings>
        {
            {0    , new ChannelSettings("None"                  , "[NNE]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {1    , new ChannelSettings("Debug"                 , "[DBG]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {2    , new ChannelSettings("Urgent"                , "[URG]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {3    , new ChannelSettings("Notice"                , "[NTC]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {10   , new ChannelSettings("Say"                   , "[SAY]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {11   , new ChannelSettings("Shout"                 , "[SHT]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {12   , new ChannelSettings("TellOutgoing"          , "[TLO]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {13   , new ChannelSettings("TellIncoming"          , "[TLI]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {14   , new ChannelSettings("Party"                 , "[PTY]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {15   , new ChannelSettings("Alliance"              , "[ALC]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {16   , new ChannelSettings("Ls1"                   , "[LS1]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {17   , new ChannelSettings("Ls2"                   , "[LS2]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {18   , new ChannelSettings("Ls3"                   , "[LS3]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {19   , new ChannelSettings("Ls4"                   , "[LS4]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {20   , new ChannelSettings("Ls5"                   , "[LS5]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {21   , new ChannelSettings("Ls6"                   , "[LS6]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {22   , new ChannelSettings("Ls7"                   , "[LS7]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {23   , new ChannelSettings("Ls8"                   , "[LS8]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {24   , new ChannelSettings("FreeCompany"           , "[FRC]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {27   , new ChannelSettings("NoviceNetwork"         , "[NNW]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {28   , new ChannelSettings("CustomEmote"           , "[EMC]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {29   , new ChannelSettings("StandardEmote"         , "[EMS]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {30   , new ChannelSettings("Yell"                  , "[YLL]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {32   , new ChannelSettings("CrossParty"            , "[CPT]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {36   , new ChannelSettings("PvPTeam"               , "[PVP]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {56   , new ChannelSettings("Echo"                  , "[ECH]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {57   , new ChannelSettings("SystemMessage"         , "[SMG]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {58   , new ChannelSettings("SystemError"           , "[SER]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {59   , new ChannelSettings("GatheringSystemMessage", "[GSM]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {71   , new ChannelSettings("RetainerSale"          , "[RSL]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {37   , new ChannelSettings("CrossLinkShell1"       , "[CW1]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {101  , new ChannelSettings("CrossLinkShell2"       , "[CW2]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {102  , new ChannelSettings("CrossLinkShell3"       , "[CW3]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {103  , new ChannelSettings("CrossLinkShell4"       , "[CW4]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {104  , new ChannelSettings("CrossLinkShell5"       , "[CW5]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {105  , new ChannelSettings("CrossLinkShell6"       , "[CW6]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {106  , new ChannelSettings("CrossLinkShell7"       , "[CW7]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {107  , new ChannelSettings("CrossLinkShell8"       , "[CW8]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {43   , new ChannelSettings("Actions"               , "[UAC]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {41   , new ChannelSettings("Damage"                , "[DAM]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {42   , new ChannelSettings("FailedAttacks"         , "[FAT]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {44   , new ChannelSettings("ItemsUsed"             , "[ISU]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {45   , new ChannelSettings("Healing"               , "[HLG]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {46   , new ChannelSettings("BenefictsStart"        , "[BFS]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {47   , new ChannelSettings("DetrimentsStart"       , "[DTS]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {48   , new ChannelSettings("BenefictsEnd"          , "[BFE]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {49   , new ChannelSettings("DetrimentsEnd"         , "[DTE]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {55   , new ChannelSettings("Alarms"                , "[ALM]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {8250 , new ChannelSettings("BattleSystemMessage"   , "[BSM]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {61   , new ChannelSettings("Event"                 , "[EVT]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {62   , new ChannelSettings("Loot"                  , "[LOT]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {64   , new ChannelSettings("Progression"           , "[PGR]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {65   , new ChannelSettings("Loot Rolls"            , "[LTR]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {66   , new ChannelSettings("Synthesis"             , "[SYN]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {67   , new ChannelSettings("FishingSystemMessage"  , "[FSH]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {68   , new ChannelSettings("NPCAnnouncement"       , "[NPA]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {69   , new ChannelSettings("FCAnnouncement"        , "[FCA]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {70   , new ChannelSettings("FCLogin"               , "[FCL]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {72   , new ChannelSettings("RecruitmentNotice"     , "[RNT]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {73   , new ChannelSettings("SignMarking"           , "[SMK]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {74   , new ChannelSettings("Randoms"               , "[RND]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {76   , new ChannelSettings("OrchestronTrack"       , "[MUS]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {77   , new ChannelSettings("PVPTeamAnnouncement"   , "[PTA]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {78   , new ChannelSettings("PVPTeamLogin"          , "[PTL]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {79   , new ChannelSettings("MessageBookAlert"      , "[MBA]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {60   , new ChannelSettings("ErrorMessage"          , "[ERR]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
            {75   , new ChannelSettings("NoviceNetworkNotice"   , "[NNW]", new Vector4(1, 1, 1, 1), new Vector4(0, 0, 0, 1))},
        };

        //FFXIV Chat Box stuff
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr GetBaseUIObjDelegate();
        private GetBaseUIObjDelegate getBaseUIObj;

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        private delegate IntPtr GetUI2ObjByNameDelegate(IntPtr getBaseUIObj, string UIName, int index);
        private GetUI2ObjByNameDelegate getUI2ObjByName;

        public IntPtr scan1;
        public IntPtr scan2;

        public IntPtr chatLog;
        public IntPtr chatLogStuff;
        public IntPtr chatLogPanel_0;

        public float[] chatLogPosition;
        public int Width = 0;
        public int Height = 0;
        public byte Alpha = 0;
        public byte BoxHide = 0;
        public byte BoxOn = 50;
        public byte BoxOff = 82;
        public int sleep = 0;


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
            try
            {
                // Initializing plugin, hooking into chat.
                this.pluginInterface = pluginInterface;
                config = pluginInterface.GetPluginConfig() as ChatExtenderPluginConfiguration ?? new ChatExtenderPluginConfiguration();
                this.pluginInterface.UiBuilder.OnBuildFonts += AddFonts;
                this.pluginInterface.UiBuilder.RebuildFonts();

                //Hooks for FFXIV ChatBox
                scan1 = pluginInterface.TargetModuleScanner.ScanText("E8 ?? ?? ?? ?? 41 b8 01 00 00 00 48 8d 15 ?? ?? ?? ?? 48 8b 48 20 e8 ?? ?? ?? ?? 48 8b cf");
                scan2 = pluginInterface.TargetModuleScanner.ScanText("e8 ?? ?? ?? ?? 48 8b cf 48 89 87 ?? ?? 00 00 e8 ?? ?? ?? ?? 41 b8 01 00 00 00");

                getBaseUIObj = Marshal.GetDelegateForFunctionPointer<GetBaseUIObjDelegate>(scan1);
                getUI2ObjByName = Marshal.GetDelegateForFunctionPointer<GetUI2ObjByNameDelegate>(scan2);
                chatLog = getUI2ObjByName(Marshal.ReadIntPtr(getBaseUIObj(), 0x20), "ChatLog", 1);
                chatLogStuff = getUI2ObjByName(Marshal.ReadIntPtr(getBaseUIObj(), 0x20), "ChatLog", 1);
                chatLogPanel_0 = getUI2ObjByName(Marshal.ReadIntPtr(getBaseUIObj(), 0x20), "ChatLog", 1);
                chatLogPosition = new float[2];
                
                if (config.ChannelSettings == null)
                {
                    config.ChannelSettings = ChannelSettingsTable;
                }
                else
                {
                    foreach (var key in ChannelSettingsTable.Keys)
                    {
                        if (config.ChannelSettings.ContainsKey(key))
                        {
                            ChannelSettingsTable[key].Update(config.ChannelSettings[key]);
                        }
                    }

                    config.ChannelSettings = ChannelSettingsTable;
                }

                if (config.Tabs == null || config.Tabs.Count == 0)
                {
                    TabBase newTab = new TabBase
                    {
                        Title = "New Tab",
                        Enabled = true
                    };
                    tabs = new List<TabBase> { newTab };
                    config.Tabs = tabs;
                }
                else
                {
                    tabs = config.Tabs;
                }

                foreach(var tab in config.Tabs)
                {
                    if (tab.EnabledChannels == null)
                    {
                        tab.EnabledChannels = new Dictionary<string, BoolRef>();
                    }
                    if (tab.ShowChannelTag == null)
                    {
                        tab.ShowChannelTag = new Dictionary<string, BoolRef>();
                    }

                    foreach (var key in ChannelSettingsTable.Keys)
                    {
                        var channelName = ChannelSettingsTable[key].Name;
                        if (!tab.EnabledChannels.ContainsKey(channelName))
                        {
                            tab.EnabledChannels.Add(channelName, false);
                        }
                        if (!tab.ShowChannelTag.ContainsKey(channelName))
                        {
                            tab.ShowChannelTag .Add(channelName, true);
                        }
                    }
                }

                SaveConfig();
                
                // Set up command handlers
                this.pluginInterface.CommandManager.AddHandler("/cht", new CommandInfo(OnTranslateCommand)
                {
                    HelpMessage = "Open config with '/cht c', and the extender with '/cht w'"
                });

                this.pluginInterface.Framework.Gui.Chat.OnChatMessage += Chat_OnChatMessage;
                this.pluginInterface.UiBuilder.OnBuildUi += ChatUI;
                this.pluginInterface.UiBuilder.OnOpenConfigUi += Chat_ConfigWindow;
            }
            catch (Exception e)
            {
                ErrorLog(e.ToString());
            }
        }
    }
}
