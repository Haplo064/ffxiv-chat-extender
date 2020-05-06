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

//TODO
//https://github.com/Haplo064/ffxiv-chat-extender/projects/2

namespace DalamudPlugin
{
    public partial class ChatExtenderPlugin : IDalamudPlugin
    {

        // Dalamud Plugin

        public string Name => "Chat Extender";
        private DalamudPluginInterface pluginInterface;
        private bool chatWindow = false;
        private bool configWindow = false;
        private bool bubblesWindow = false;
        //Globals

        float minH = 1.5f;
        float maxH = 0.005f;

        public bool injectChat = false;
        public int translator = 1;         //1=Google,2=Yandex
        public string language = "jpn";
        public string yandex = "";
        public List<TabBase> items = new List<TabBase>();
        public List<TabBase> itemsTemp = new List<TabBase>();
        public List<ChatText> chatBubble = new List<ChatText>();
        public List<BubbleOffset> bubbleOffsets = new List<BubbleOffset>();
        public uint bufSize = 24;
        public string tempTitle = "Title";
        public string tempHigh = "words,to,highlight";
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
        static bool no_mouse2 = false;
        static bool flickback = false;
        static uint tab_ind;
        static uint tab_norm;
        static uint tab_sel;
        static uint tab_ind_text;
        static uint tab_norm_text;
        static uint tab_sel_text;
        static bool allowTranslation = false;
        public ImFontPtr font;
        public int noRepeats = 0;

        public Highlighter high = new Highlighter();

        static int space_hor = 4;
        static int space_ver = 0;
        static int maxBubbleWidth = 500;
        static bool drawDebug = false;
        static bool boolUp = true;
        static bool bubblesChannel = false;
        static ImGuiScene.TextureWrap goatImage;

        static Num.Vector2 bubble_TL1 = new Num.Vector2(0f  / 75f, 0f  / 75f);
        static Num.Vector2 bubble_TL2 = new Num.Vector2(25f / 75f, 25f / 75f);
        static Num.Vector2 bubble_TM1 = new Num.Vector2(25f / 75f, 0f  / 75f);
        static Num.Vector2 bubble_TM2 = new Num.Vector2(50f / 75f, 25f / 75f);
        static Num.Vector2 bubble_TR1 = new Num.Vector2(50f / 75f, 0f  / 75f);
        static Num.Vector2 bubble_TR2 = new Num.Vector2(75f / 75f, 25f / 75f);

        static Num.Vector2 bubble_ML1 = new Num.Vector2(0f  / 75f, 25f / 75f);
        static Num.Vector2 bubble_ML2 = new Num.Vector2(25f / 75f, 50f / 75f);
        static Num.Vector2 bubble_MM1 = new Num.Vector2(25f / 75f, 25f / 75f);
        static Num.Vector2 bubble_MM2 = new Num.Vector2(50f / 75f, 50f / 75f);
        static Num.Vector2 bubble_MR1 = new Num.Vector2(50f / 75f, 25f / 75f);
        static Num.Vector2 bubble_MR2 = new Num.Vector2(75f / 75f, 50f / 75f);

        static Num.Vector2 bubble_BL1 = new Num.Vector2(0f  / 75f, 50f / 75f);
        static Num.Vector2 bubble_BL2 = new Num.Vector2(25f / 75f, 75f / 75f);
        static Num.Vector2 bubble_BM1 = new Num.Vector2(25f / 75f, 50f / 75f);
        static Num.Vector2 bubble_BM2 = new Num.Vector2(50f / 75f, 75f / 75f);
        static Num.Vector2 bubble_BR1 = new Num.Vector2(50f / 75f, 50f / 75f);
        static Num.Vector2 bubble_BR2 = new Num.Vector2(75f / 75f, 75f / 75f);

        static int xDisp = 4;
        static int xCut  = 0;
        static int yDisp = 0;
        static int yCut  = 0;

        static float bubbleRounding = 20f;

        static string pathString = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\FFXIV_ChatExtender\Logs\";
        static string dllPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        static string fontFile = "FFXIV_Chat.ttf";
        static string fontPath = Path.Combine(dllPath, fontFile);

        public Num.Vector4 timeColour = new Num.Vector4(255, 255, 255, 255);
        public Num.Vector4 nameColour = new Num.Vector4(255, 255, 255, 255);

        public Num.Vector4[] logColour =
        {
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255)
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
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255)
        };

        public Num.Vector4[] bubbleColour =
        {
                new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),
                new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),
                new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),
                new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),
                new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),
                new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),
                new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),
                new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),
                new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),
                new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),
                new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),
                new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),
                new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f),new Num.Vector4(0.866f,0.819f,0.761f,1f)
        };

        public bool[] bubbleEnable =
        {
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

        public String[] Channels =
        {
            "None","Debug","Urgent","Notice","Say",
            "Shout","TellOutgoing","TellIncoming","Party","Alliance",
            "Ls1","Ls2","Ls3","Ls4","Ls5",
            "Ls6","Ls7","Ls8","FreeCompany","NoviceNetwork",
            "CustomEmote","StandardEmote","Yell","CrossParty","PvPTeam",
            "Echo","SystemError","GatheringSystemMessage","RetainerSale","CrossLinkShell1",
            "CrossLinkShell2","CrossLinkShell3","CrossLinkShell4","CrossLinkShell5","CrossLinkShell6",
            "CrossLinkShell7","CrossLinkShell8","SystemMessage","Actions","Damage",
            "FailedAttacks","ItemsUsed","Healing","BenefictsStart","DetrimentsStart",
            "BenefictsEnd","DetrimentsEnd","Alarms","BattleSystemMessage","NPC",
            "Loot","Progression","Loot Rolls","Synthesis","NPCAnnouncement",
            "FCAnnouncement","FCLogin","RecruitmentNotice","SignMarking","Randoms",
            "OrchestronTrack","PVPTeamAnnouncement","PVPTeamLogin","MessageBookAlert"
        };

        public String[] Chan =
            {
                "[NNE]","[DBG]","[URG]","[NTC]","[SAY]",
                "[SHT]","[TLO]","[TLI]","[PTY]","[ALC]",
                "[LS1]","[LS2]","[LS3]","[LS4]","[LS5]",
                "[LS6]","[LS7]","[LS8]","[FRC]","[NNW]",
                "[EMC]","[EMS]","[YLL]","[CPT]","[PVP]",
                "[ECH]","[SER]","[GSM]","[RSL]","[CW1]",
                "[CW2]","[CW3]","[CW4]","[CW5]","[CW6]",
                "[CW7]","[CW8]","[SMG]","[UAC]","[DAM]",
                "[FAT]","[ISU]","[HLG]","[BFS]","[DTS]",
                "[BFE]","[DTE]","[ALM]","[BSM]","[NPC]",
                "[LOT]","[PGR]","[LTR]","[SYN]","[NPA]",
                "[FCA]","[FCL]","[RNT]","[SMK]","[RND]",
                "[ORC]","[PTA]","[PTL]","[MBA]"
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
            font = ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPath, 18.0f);
            var imagePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"Chat_Box_A.png");
            goatImage = pluginInterface.UiBuilder.LoadImage(imagePath);


            // Initializing plugin, hooking into chat.
            this.pluginInterface = pluginInterface;
            Configuration = pluginInterface.GetPluginConfig() as ChatExtenderPluginConfiguration ?? new ChatExtenderPluginConfiguration();
            


            tab_ind = UintCol(255, 50, 70, 50);
            tab_ind_text = UintCol(255, 150, 150, 150);
            tab_norm = UintCol(255, 50, 50, 50);
            tab_norm_text = UintCol(255, 150, 150, 150);
            tab_sel = UintCol(255, 90, 90, 90);
            tab_sel_text = UintCol(255, 250, 255, 255);

            try
            { allowTranslation = Configuration.AllowTranslation; }
            catch (Exception)
            {
                PluginLog.LogError("Failed to Load Translation setting!");
                allowTranslation = false;
            }

            try
            { bubblesWindow = Configuration.BubblesWindow; }
            catch (Exception)
            {
                PluginLog.LogError("Failed to Load BubbleWindow setting!");
                bubblesWindow = false;
            }

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
            { chanColour = Configuration.ChanColour.ToArray(); }
            catch (Exception)
            { PluginLog.LogError("No ChanColour to load!"); }

            try
            { bubbleEnable = Configuration.BubbleEnable.ToArray(); }
            catch (Exception)
            { PluginLog.LogError("No BubbleEnable to load!"); }

            try
            { logColour = Configuration.LogColour.ToArray(); }
            catch (Exception)
            { PluginLog.LogError("No LogColour to load!"); }

            try
            { bubbleColour = Configuration.BubbleColour.ToArray(); }
            catch (Exception)
            { PluginLog.LogError("No BubbleColour to load!"); }

            try
            { injectChat = Configuration.Inject; }
            catch (Exception)
            {
                PluginLog.LogError("Failed to Load Inject Status!");
                injectChat = false;
            }


            try
            {
                if (Configuration.Translator.HasValue)
                {
                    translator = Configuration.Translator.Value;
                }
            }
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

            try
            {
                high = Configuration.High;
                tempHigh = String.Join(",", high.highlights);
            }
            catch (Exception)
            {
                PluginLog.LogError("Failed to Load Highlighter");
                high = new Highlighter();
            }

            try
            {
                if (high.highlights.Length < 1)
                {
                    high = new Highlighter();
                }
            }
            catch (Exception)
            {
                PluginLog.LogError("Failed to Load Highlighter");
                high = new Highlighter();
            }


            try
            { no_mouse2 = Configuration.NoMouse2; }
            catch (Exception)
            {
                PluginLog.LogError("Failed to Load NoMouse2 Config!");
                no_mouse2 = false;
            }

            try
            { no_scrollbar = Configuration.NoScrollBar; }
            catch (Exception)
            {
                PluginLog.LogError("Failed to Load ScrollBar Config!");
                no_scrollbar = false;
            }

            try
            {
                if (Configuration.Space_Hor.HasValue)
                {
                    space_hor = Configuration.Space_Hor.Value;
                }
            }
            catch (Exception)
            {
                PluginLog.LogError("Failed to Load Horizontal Spacing!");
                space_hor = 4;
            }

            try
            {
                if (Configuration.Space_Ver.HasValue)
                {
                    space_ver = Configuration.Space_Ver.Value;
                }
            }
            catch (Exception)
            {
                PluginLog.LogError("Failed to Load Vertical Spacing!");
                space_ver = 0;
            }

            try
            {
                if (Configuration.TimeColour.Z > 0)
                {
                    timeColour = Configuration.TimeColour;
                }
                else
                {
                    timeColour = new Num.Vector4(255, 255, 255, 255);
                }
            }
            catch (Exception)
            {
                PluginLog.LogError("Failed to Load Time Colour!");
                timeColour = new Num.Vector4(255, 255, 255, 255);
            }

            try
            {
                if (Configuration.NameColour.Z > 0)
                {
                    nameColour = Configuration.NameColour;
                }
                else
                {
                    nameColour = new Num.Vector4(255, 255, 255, 255);
                }
            }
            catch (Exception)
            {
                PluginLog.LogError("Failed to Load Name Colour!");
                nameColour = new Num.Vector4(255, 255, 255, 255);
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

            if (items[0].Config.Length == 3)
            {
                foreach (TabBase item in items)
                {
                    bool[] temp = { false, false, false, false, false, false, false, false, false, false };
                    temp[0] = item.Config[0];
                    temp[1] = item.Config[1];
                    temp[2] = item.Config[2];
                    item.Config = temp;
                }
            }

            if (items[0].Filter == null)
            {
                foreach (TabBase item in items)
                {
                    item.Filter = "";
                    item.FilterOn = false;
                }
            }

            try
            {
                if (Configuration.Items[0].Logs.Length < Channels.Length)
                {
                    int l = 0;
                    List<TabBase> templist = new List<TabBase>();
                    foreach (TabBase items in Configuration.Items)
                    {
                        TabBase temp = new TabBase();
                        temp.AutoScroll = items.AutoScroll;
                        temp.Chat = items.Chat;
                        temp.Config = items.Config;
                        temp.Enabled = items.Enabled;
                        temp.Scroll = items.Scroll;
                        temp.Title = items.Title;
                        int i = 0;
                        foreach (bool set in items.Logs)
                        {
                            //PluginLog.Log(i.ToString());
                            temp.Logs[i] = set;
                            i++;
                        }
                        //PluginLog.Log("bool length:" + temp.Logs.Length.ToString());
                        templist.Add(temp);
                        l++;
                    }

                    items = templist;

                    Num.Vector4[] logColour_temp =
                    {
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255)
                };

                    int j = 0;
                    foreach (Num.Vector4 vec in logColour)
                    {
                        logColour_temp[j] = vec;
                        j++;
                    }
                    logColour = logColour_temp;

                    Num.Vector4[] chanColour_temp =
                    {
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),
                new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255),new Num.Vector4(255,255,255,255)
                };

                    int k = 0;
                    foreach (Num.Vector4 vec in chanColour)
                    {
                        chanColour_temp[k] = vec;
                        k++;
                    }
                    chanColour = chanColour_temp;
                }
            }
            catch (Exception)
            {
                PluginLog.Log("Fresh install, no log to fix!");
            }

            //Adding in Chans
            try
            {
                if (Configuration.Items[0].Chans.Length < Channels.Length)
                {
                    int l = 0;
                    List<TabBase> templist = new List<TabBase>();
                    foreach (TabBase items in Configuration.Items)
                    {
                        TabBase temp = new TabBase();
                        temp.AutoScroll = items.AutoScroll;
                        temp.Chat = items.Chat;
                        temp.Config = items.Config;
                        temp.Enabled = items.Enabled;
                        temp.Scroll = items.Scroll;
                        temp.Title = items.Title;
                        temp.Logs = items.Logs.ToArray();
                        temp.Chans =
                            new bool[] {
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
                        templist.Add(temp);
                        l++;
                    }

                    items = templist;
                }

            }
            catch (Exception)
            {
                PluginLog.Log("Fresh install, no Chans to fix!");
            }

            try
            {
                if (Configuration.Chan.Length > 30)
                {
                    Chan = Configuration.Chan.ToArray();
                }
            }
            catch (Exception)
            {
                PluginLog.Log("No Chan list to load");
            }

            if (translator == 0) { translator = 1; }

            SaveConfig();

            TransY.Make("https://translate.yandex.net/api/v1.5/tr.json/translate", Configuration.YandexKey);

            // Set up command handlers
            this.pluginInterface.CommandManager.AddHandler("/cht", new CommandInfo(OnTranslateCommand)
            {
                HelpMessage = "Open config with '/cht c', and the extender with '/cht w'"
            });

            this.pluginInterface.Framework.Gui.Chat.OnChatMessage += Chat_OnChatMessage;
            this.pluginInterface.UiBuilder.OnBuildUi += ChatUI;
            this.pluginInterface.UiBuilder.OnOpenConfigUi += Chat_ConfigWindow;
            this.pluginInterface.UiBuilder.OnBuildUi += ChatBubbles;

        }

    }
}
