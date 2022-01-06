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
                // Keep commands to allow xiv-macro to toggle windows open/closed
                var args = arguments.Split(' ');

                if (args[0] == "w" | args[0] == "window")
                {
                    config.ShowChatWindow = !config.ShowChatWindow;
                    SaveConfig();
                    return;
                }
                else if (args[0] == "c" | args[0] == "config")
                {
                    if (this.configWindow)
                    {
                        SaveConfig();
                    }
                    configWindow = !configWindow;
                    return;
                }
                else if (args[0] == "d" | args[0] == "debug")
                {
                    debug = !debug;
                    return;
                }
                else
                {
                    this.pluginInterface.Framework.Gui.Chat.PrintError("No valid command supplied.");
                    return;
                }
            }
        }
    }
}
