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
    }
}
