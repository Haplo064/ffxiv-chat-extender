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
//https://github.com/Haplo064/ffxiv-chat-extender/projects/2


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
        //public ImFontPtr font = ImGui.GetIO().Fonts.AddFontFromFileTTF("FFXIV_Chat.ttf", 20);

        public Highlighter high = new Highlighter();

        static int space_hor = 4;
        static int space_ver = 0;
        
        static string pathString = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\FFXIV_ChatExtender\Logs\";

        public Num.Vector4 timeColour = new Num.Vector4(255, 255, 255, 255);

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
                tempHigh = String.Join(",",high.highlights);
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

            if (items[0].Filter==null)
            {
                foreach (TabBase item in items)
                {
                    item.Filter="";
                    item.FilterOn=false;
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
            catch(Exception)
            {
                PluginLog.Log("No Chan list to load");
            }
            
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
                if (tabs.Enabled)
                {
                    TabBase babyClone = new TabBase();
                    babyClone.AutoScroll = tabs.AutoScroll;
                    babyClone.Chat = new List<ChatText>();
                    babyClone.Config = tabs.Config.ToArray();
                    babyClone.Enabled = tabs.Enabled;
                    babyClone.Logs = tabs.Logs.ToArray();
                    babyClone.Scroll = tabs.Scroll;
                    babyClone.Title = tabs.Title.ToString();
                    babyClone.Filter = tabs.Filter.ToString();
                    babyClone.FilterOn = tabs.FilterOn;
                    clone.Add(babyClone);
                }
            }
            return clone;
        }

        private List<TabBase> CopyItems(List<TabBase> items)
        {
            List<TabBase> clone = new List<TabBase>();
            foreach (TabBase tabs in items)
            {
                if (tabs.Enabled)
                {
                    TabBase babyClone = new TabBase();
                    babyClone.AutoScroll = tabs.AutoScroll;
                    babyClone.Chat = tabs.Chat;
                    babyClone.Config = tabs.Config.ToArray();
                    babyClone.Enabled = tabs.Enabled;
                    babyClone.Logs = tabs.Logs.ToArray();
                    babyClone.Scroll = tabs.Scroll;
                    babyClone.Title = tabs.Title.ToString();
                    babyClone.Filter = tabs.Filter.ToString();
                    babyClone.FilterOn = tabs.FilterOn;
                    clone.Add(babyClone);
                }
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
            public bool NoMouse2 { get; set; }
            public bool NoMove { get; set; }
            public bool NoResize { get; set; }
            public bool NoScrollBar { get; set; }
            public int? Space_Hor { get; set; }
            public int? Space_Ver { get; set; }
            public Num.Vector4 TimeColour { get; set; }
            public Highlighter High { get; set; }
            public String[] Chan { get; set; }
        }

        private void ChatUI()
        {
            ImGuiWindowFlags chat_window_flags = 0;
            ImGuiWindowFlags chat_sub_window_flags = 0;
            if (no_titlebar) chat_window_flags |= ImGuiWindowFlags.NoTitleBar;
            if (no_scrollbar) chat_window_flags |= ImGuiWindowFlags.NoScrollbar;
            if (no_scrollbar) chat_sub_window_flags |= ImGuiWindowFlags.NoScrollbar;
            if (!no_menu) chat_window_flags |= ImGuiWindowFlags.MenuBar;
            if (no_move) chat_window_flags |= ImGuiWindowFlags.NoMove;
            if (no_resize) chat_window_flags |= ImGuiWindowFlags.NoResize;
            if (no_collapse) chat_window_flags |= ImGuiWindowFlags.NoCollapse;
            if (no_nav) chat_window_flags |= ImGuiWindowFlags.NoNav;
            if (no_mouse) { chat_window_flags |= ImGuiWindowFlags.NoMouseInputs; }
            if (no_mouse2) { chat_sub_window_flags |= ImGuiWindowFlags.NoMouseInputs; }

            if (chatWindow)
            {
                if(flickback)
                {
                    no_mouse = false;
                    flickback = false;
                }
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
                            //WIP

                            if (tab.sel)
                            {
                                ImGui.PushStyleColor(ImGuiCol.Tab, tab_sel);
                                ImGui.PushStyleColor(ImGuiCol.Text, tab_sel_text);
                                tab.sel = false;
                            }
                            else if (tab.msg)
                            {
                                ImGui.PushStyleColor(ImGuiCol.Tab, tab_ind);
                                ImGui.PushStyleColor(ImGuiCol.Text, tab_ind_text);
                            }
                            else
                            {
                                ImGui.PushStyleColor(ImGuiCol.Tab, tab_norm);
                                ImGui.PushStyleColor(ImGuiCol.Text, tab_norm_text);
                            }



                            if (ImGui.BeginTabItem(tab.Title))
                            {
                                tab.sel = true;
                                float footer = (ImGui.GetStyle().ItemSpacing.Y) / 2 + ImGui.GetFrameHeightWithSpacing();
                                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Num.Vector2(space_hor, space_ver));
                                ImGui.BeginChild("scrolling", new Num.Vector2(0, -footer), false, chat_sub_window_flags);
                                

                                foreach (ChatText line in tab.Chat)
                                {
                                    if (tab.FilterOn)
                                    {
                                        if (ContainsText(line.Text, tab.Filter))
                                        {
                                            if (tab.Config[0]) { ImGui.TextColored(timeColour, line.Time + " "); ImGui.SameLine(); }
                                            if (tab.Config[1] && tab.Chans[ConvertForArray(line.Channel)]) { ImGui.TextColored(chanColour[ConvertForArray(line.Channel)], line.ChannelShort + " "); ImGui.SameLine(); }
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
                                                        this.pluginInterface.Framework.Gui.OpenMapWithMapLink((Dalamud.Game.Chat.SeStringHandling.Payloads.MapLinkPayload)textTypes.Payload);
                                                    }
                                                }

                                                if (count < (line.Text.Count - 1))
                                                {
                                                    ImGui.SameLine();                                                    count++;
                                                }

                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (tab.Config[0]) { ImGui.TextColored(timeColour, line.Time + " "); ImGui.SameLine(); }
                                        if (tab.Config[1] && tab.Chans[ConvertForArray(line.Channel)]) { ImGui.TextColored(chanColour[ConvertForArray(line.Channel)], line.ChannelShort + " "); ImGui.SameLine(); }
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
                                                    this.pluginInterface.Framework.Gui.OpenMapWithMapLink((Dalamud.Game.Chat.SeStringHandling.Payloads.MapLinkPayload)textTypes.Payload);
                                                }
                                            }

                                            if (count < (line.Text.Count - 1))
                                            {
                                                ImGui.SameLine();
                                                count++;
                                            }

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

                                if (tab.FilterOn)
                                {
                                    ImGui.InputText("Filter Text", ref tab.Filter, 999);
                                    if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Only show lines with this text."); }
                                }

                                if (no_mouse2 && !no_mouse)
                                {
                                    Num.Vector2 vMin = ImGui.GetWindowContentRegionMin();
                                    Num.Vector2 vMax = ImGui.GetWindowContentRegionMax();

                                    vMin.X += ImGui.GetWindowPos().X;
                                    vMin.Y += ImGui.GetWindowPos().Y + 22;
                                    vMax.X += ImGui.GetWindowPos().X - 22;
                                    vMax.Y += ImGui.GetWindowPos().Y;

                                    if (ImGui.IsMouseHoveringRect(vMin, vMax)) { no_mouse = true; flickback = true; }
                                }
                                tab.msg = false;
                                ImGui.EndTabItem();
                            }
                            ImGui.PopStyleColor();
                            ImGui.PopStyleColor();
                        }
                        loop++;
                    }
                    ImGui.EndTabBar();
                    ImGui.End();
                }
            }

            if (configWindow)
            {
                //ImGui.PushFont(font);

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
                        ImGui.Checkbox("Inject Translation", ref injectChat);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Inject translated text into the normal FFXIV Chatbox"); }
                        ImGui.NextColumn();
                        ImGui.Checkbox("Chat Extender", ref chatWindow);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Enable/Disable the Chat Extender"); }
                        ImGui.NextColumn();
                        ImGui.Text("");
                        ImGui.NextColumn();

                        ImGui.Checkbox("Scrollbar", ref no_scrollbar);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Shows ScrollBar"); }
                        ImGui.NextColumn();
                        ImGui.Checkbox("Lock Window Position", ref no_move);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Lock/Unlock the position of the Chat Extender"); }
                        ImGui.NextColumn();
                        ImGui.Checkbox("Lock Window Size", ref no_resize);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Lock/Unlock the size of the Chat Extender"); }
                        ImGui.NextColumn();

                        ImGui.Checkbox("ClickThrough Tab Bar", ref no_mouse);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Enable/Disable being able to clickthrough the Tab Bar of the Chat Extender"); }
                        ImGui.NextColumn();
                        ImGui.Checkbox("ClickThrough Chat", ref no_mouse2);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Enable/Disable being able to clickthrough the Chat Extension chatbox"); }
                        ImGui.NextColumn();
                        ImGui.Text("");
                        ImGui.NextColumn();

                        ImGui.InputInt("H Spacing", ref space_hor);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Horizontal spacing of chat text"); }
                        ImGui.NextColumn();
                        ImGui.InputInt("V Spacing", ref space_ver);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Vertical spacing of cha text"); }
                        ImGui.NextColumn();
                        ImGui.Text("");
                        ImGui.NextColumn();

                        ImGui.Columns(1);
                        ImGui.Text("Surrounds of Translated text");
                        ImGui.PushItemWidth(24);
                        ImGui.InputText("##Left", ref lTr, 3); ImGui.SameLine();
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Alter the characters on the left of Translated text"); }
                        ImGui.PopItemWidth();
                        ImGui.Text("Translation"); ImGui.SameLine();
                        ImGui.PushItemWidth(24);
                        ImGui.InputText("##Right", ref rTr, 3);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Alter the characters on the right of Translated text"); }
                        ImGui.PopItemWidth();
                        ImGui.Text("");

                        ImGui.Text("");
                        ImGui.SliderFloat("Chat Extender Alpha", ref alpha, 0.001f, 0.999f);
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Alter the Alpha of the Chat Extender"); }
                        ImGui.Text("");

                        if (ImGui.TreeNode("Tab Order"))
                        {

                            ImGui.Columns(3);
                            ImGui.Text("Tab"); ImGui.NextColumn();
                            ImGui.Text(""); ImGui.NextColumn();
                            ImGui.Text(""); ImGui.NextColumn();

                            List<TabBase> temp_clone = new List<TabBase>();
                            temp_clone = CopyItems(items);
                            for (int i = 0; i < (items.Count); i++)
                            {
                                ImGui.Text(items[i].Title); ImGui.NextColumn();
                                if (i > 0)
                                {
                                    if (ImGui.Button("^##" + i.ToString()))
                                    {
                                        TabBase mover = temp_clone[i];
                                        temp_clone.RemoveAt(i);
                                        temp_clone.Insert(i - 1, mover);
                                    }
                                }
                                ImGui.NextColumn();
                                if (i < items.Count - 1)
                                {
                                    if (ImGui.Button("v##" + i.ToString()))
                                    {
                                        TabBase mover = temp_clone[i];
                                        temp_clone.RemoveAt(i);
                                        temp_clone.Insert(i + 1, mover);
                                    }
                                }
                                ImGui.NextColumn();
                            }
                            ImGui.Columns(1);
                            items = CopyItems(temp_clone);
                            ImGui.TreePop();

                        }


                        if (ImGui.TreeNode("Channels"))
                        {

                            ImGui.Columns(4);
                            ImGui.Text("Example"); ImGui.NextColumn();
                            ImGui.Text("Colour 1"); ImGui.NextColumn();
                            ImGui.Text("Colour 2"); ImGui.NextColumn();
                            ImGui.Text(""); ImGui.NextColumn();
                            ImGui.TextColored(timeColour, "[12:00]"); ImGui.NextColumn();
                            ImGui.ColorEdit4("Time Colour", ref timeColour, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel); ImGui.NextColumn();
                            ImGui.Text(""); ImGui.NextColumn();
                            ImGui.Text(""); ImGui.NextColumn();
                            for (int i = 0; i < (Channels.Length); i++)
                            {
                                ImGui.InputText("##Tab Name"+i.ToString(), ref Chan[i], 99); ImGui.NextColumn();
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
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Add a new Tab to the Chat Extender"); }

                        ImGui.Text("");
                        ImGui.Text("Highlight Example");
                        HighlightText();
                        ImGui.InputText("##HighlightText", ref tempHigh, 999); if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Will highlight EXACT matches only. Seperate words with [,]."); }
                        ImGui.SameLine();
                        if (ImGui.Button("Apply"))
                        {
                            high.highlights = tempHigh.Split(',');
                        }
                        ImGui.Columns(4);
                        ImGui.SliderInt("Alpha", ref high.htA, 0, 255); ImGui.NextColumn();
                        ImGui.SliderInt("Blue", ref high.htB, 0, 255); ImGui.NextColumn();
                        ImGui.SliderInt("Green", ref high.htG, 0, 255); ImGui.NextColumn();
                        ImGui.SliderInt("Red", ref high.htR, 0, 255); ImGui.NextColumn();
                        ImGui.Columns(1);
                        ImGui.Text("");


                        if (ImGui.Button("Save and Close Config"))
                        {
                            SaveConfig();

                            configWindow = false;
                        }
                        if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Changes will only be saved for the current session unless you do this!"); }
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
                                    if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Change the title of the Tab"); }

                                    ImGui.Columns(3);
                                    
                                    ImGui.Checkbox("Time Stamp", ref tab.Config[0]);
                                    if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Show Timestamps in this Tab"); }
                                    ImGui.NextColumn();
                                    ImGui.Checkbox("Channel", ref tab.Config[1]);
                                    if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Show the Channel the message came from"); }
                                    ImGui.NextColumn();
                                    ImGui.Checkbox("Translate", ref tab.Config[2]);
                                    if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Enable Japanese -> English translation"); }
                                    ImGui.NextColumn();

                                    ImGui.Checkbox("AutoScroll", ref tab.AutoScroll);
                                    if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Enable the Chat to scroll automatically on a new message"); }
                                    ImGui.NextColumn();
                                    ImGui.Checkbox("Save to file", ref tab.Config[3]);
                                    if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Write this tab to '\\My Documents\\FFXIV_ChatExtender\\Logs\\<YYYYMMDD>_TAB.txt"); }
                                    ImGui.NextColumn();
                                    ImGui.Checkbox("Enable Filter", ref tab.FilterOn);
                                    if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Enable Filtering of text"); }
                                    ImGui.NextColumn();

                                    ImGui.Columns(1);

                                    ImGui.Text("");


                                    //TODO: Add a confirm prompt

                                    if (EnabledTabs(items) > 1)
                                    { 
                                        if (ImGui.Button("Delete Tab"))
                                        { tab.Enabled = false; }
                                    }
                                    if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Removes Tab"); }



                                    ImGui.Columns(2);
                                    ImGui.Text("Channel"); ImGui.NextColumn();
                                    if (tab.Config[1]) { ImGui.Text("Show Short"); }
                                    else { ImGui.Text(""); }
                                    ImGui.NextColumn();

                                    for (int i = 0; i < (Channels.Length); i++)
                                    {
                                        ImGui.PushStyleColor(ImGuiCol.Text, chanColour[i]);
                                        ImGui.Checkbox("[" + Channels[i] + "]", ref tab.Logs[i]); ImGui.NextColumn();
                                        if (tab.Config[1]) { ImGui.Checkbox(Chan[i], ref tab.Chans[i]); }
                                        else { ImGui.Text(""); } ImGui.NextColumn();
                                        ImGui.PopStyleColor();
                                    }
                                    ImGui.Columns(1);
                                    ImGui.EndChild();


                                }
                            }
                        }
                        ImGui.EndTabItem();
                    }

                    /*
                    if (ImGui.BeginTabItem("Debug"))
                    {
                        ImGui.Text("This is not the tab you are looking for. Move along.");
                        ImGui.EndTabItem();
                    }
                    */
                }

                ImGui.EndTabBar();
                ImGui.EndChild();

                //ImGui.PopFont();
            }
        }

        public uint UintCol(int A, int B, int G, int R)
        {
            //PluginLog.Log();
            return Convert.ToUInt32("0x" + A.ToString("X2") + B.ToString("X2") + G.ToString("X2") + R.ToString("X2"), 16);
            //return UInt32.Parse("0x" + R.ToString("X2") + G.ToString("X2") + B.ToString("X2") + A.ToString("X2"));
        }

        public bool ContainsText(List<TextTypes> text,string find)
        {
            String concat = "";
                foreach(TextTypes texts in text)
            {
                concat += texts.Text + " ";
            }

            return concat.Contains(find);
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
            Configuration.NoMouse2 = no_mouse2;
            Configuration.NoMove = no_move;
            Configuration.NoResize = no_resize;
            Configuration.Space_Hor = space_hor;
            Configuration.Space_Ver = space_ver;
            Configuration.TimeColour = timeColour;
            Configuration.High = high;
            Configuration.Chan = Chan.ToArray();
            this.pluginInterface.SavePluginConfig(Configuration);
        }

        public void HighlightText()
        {
            ImGui.GetWindowDrawList().AddRectFilled(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), UintCol(high.htA, high.htB, high.htG, high.htR), 2.0f);
        }

        public void Wrap(String input)
        {
            String[] inputArray = input.Split(' ');

            int count = 0;
            foreach (String splits in inputArray)
            {
                if (ImGui.GetContentRegionAvail().X - 5 - ImGui.CalcTextSize(splits).X < 0) { ImGui.Text(""); }

                ImGui.Text(splits);
                foreach (String word in high.highlights)
                {
                    string search = StripPunctuation(splits.ToLower());
                    if (search == word) HighlightText();
                }

                if (count < (inputArray.Length - 1))
                {
                    ImGui.SameLine();
                    count++;
                }
            }
        }

        public static string StripPunctuation(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s)
            {
                if (!char.IsPunctuation(c))
                    sb.Append(c);
            }
            return sb.ToString();
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
            try
            {
                int value = Array.IndexOf(Channels, type);
                if (value >= 0) { return Array.IndexOf(Channels, type); }
                else
                {
                    PluginLog.Log(type);
                    type = AddOnChannels(Int32.Parse(type.ToString()) & 127);
                    value = Array.IndexOf(Channels, type);
                    if (value >= 0) { return Array.IndexOf(Channels, type); }
                    else { return Int32.Parse(type.ToString()) & 127; }
                }
            }
            catch (Exception e)
            {
                PluginLog.Log(e.ToString());
                return 0;
            }
        }

        public string AddOnChannels(int channel)
        {
            if (channel == 57) return "SystemMessage";
            if (channel == 43) return "Actions";
            if (channel == 41) return "Damage";
            if (channel == 42) return "FailedAttacks";
            if (channel == 44) return "ItemsUsed";
            if (channel == 45) return "Healing";
            if (channel == 46) return "BenefictsStart";
            if (channel == 47) return "DetrimentsStart";
            if (channel == 48) return "BenefictsEnd";
            if (channel == 49) return "DetrimentsEnd";
            if (channel == 55) return "Alarms";
            if (channel == 58) return "BattleSystemMessage";
            if (channel == 61) return "NPC";
            if (channel == 62) return "Loot";
            if (channel == 64) return "Progression";
            if (channel == 65) return "LootRolls";
            if (channel == 66) return "Synthesis";
            if (channel == 68) return "NPCAnnouncement";
            if (channel == 69) return "FCAnnouncement";
            if (channel == 70) return "FCLogin";
            if (channel == 72) return "RecruitmentNotice";
            if (channel == 73) return "SignMarking";
            if (channel == 74) return "Randoms";
            if (channel == 76) return "OrchestronTrack";
            if (channel == 77) return "PVPTeamAnnouncement";
            if (channel == 78) return "PVPTeamLogin";
            if (channel == 79) return "MessageBookAlert";
            else return channel.ToString();
        }

        public string GetChannelName(string type)
        {
            try
            { return Chan[ConvertForArray(type)]; }
            catch(Exception)
            { return type; }
        }

        private void Chat_OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            var senderName = sender.TextValue;
            List<Payload> payloads = message.Payloads;


            foreach (var tab in items)
            {
                int chan = ConvertForArray(type.ToString());
                if (chan < Channels.Length)
                {
                    if (tab.Logs[chan])
                    {
                        ChatText tmp = new ChatText();

                        tmp.Time = GetTime();
                        tmp.ChannelShort = GetChannelName(type.ToString());
                        try
                        {
                            tmp.Channel = Channels[chan].Trim().Replace(" ", "");
                        }
                        catch (Exception)
                        {
                            tmp.Channel = chan.ToString();
                        }

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
                                        wrangler.Payload = payloader;
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

                            //PluginLog.Log(payload.ToString());

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
                        tab.msg = true;
                    }
                }
                else PluginLog.Log("[" + chan.ToString() + "] " + message.TextValue);

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
            public string[] highlights = {""};
            public int htA = 120;
            public int htB = 255;
            public int htG = 255;
            public int htR = 255;
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
