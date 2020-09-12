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
using System.Runtime.InteropServices;
using Dalamud.Configuration;
using Num = System.Numerics;
using System.Reflection;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using System.Numerics;
using Dalamud.Game.Chat.SeStringHandling.Payloads;
using System.Text.RegularExpressions;

namespace DalamudPlugin
{
    public partial class ChatExtenderPlugin : IDalamudPlugin
    {
        private void AddFonts()
        {
            AddFont("XIVfree.ttf", ref font);
            AddFont("XIVfree_outline.ttf", ref outlineFont);
            fontsLoaded = true;
        }

        private unsafe void AddFont(string fileName, ref ImFontPtr fontPtr)
        {
            string fontFile = fileName;
            string fontPath = Path.Combine(dllPath, fontFile);

            ImFontConfigPtr fontConfig = ImGuiNative.ImFontConfig_ImFontConfig();
            fontConfig.MergeMode = true;
            fontConfig.PixelSnapH = false;

            var gameRangeHandle = GCHandle.Alloc(new ushort[]
            {
                0xE016,
                0xf739,
                0
            }, GCHandleType.Pinned);

            var gameRangeHandle2 = GCHandle.Alloc(new ushort[]
            {
                0x2013,
                0x303D,
                0
            }, GCHandleType.Pinned);

            fontPtr = ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPath, config.FontSize);
            ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPath, config.FontSize, fontConfig, gameRangeHandle.AddrOfPinnedObject());
            ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPath, config.FontSize, fontConfig, gameRangeHandle2.AddrOfPinnedObject());


            fontConfig.Destroy();
            gameRangeHandle.Free();
            gameRangeHandle2.Free();
        }
        
        private void UpdateFonts()
        {
            // This is causing a crash
            //AddFonts();
            //pluginInterface.UiBuilder.RebuildFonts();
        }

        private void Chat_ConfigWindow(object Sender, EventArgs args)
        {
            configWindow = true;
        }
        
        public void SaveConfig()
        {
            config.Tabs = tabs;
            config.ChannelSettings = ChannelSettingsTable;
            this.pluginInterface.SavePluginConfig(config);
        }

        public ChannelSettings GetChannelSettings(XivChatType type)
        {
            if (ChannelSettingsTable.ContainsKey((int)type))
            {
                return ChannelSettingsTable[(int)type];
            }
            else if (ChannelSettingsTable.ContainsKey(127 & (int)type))
            {
                return ChannelSettingsTable[127 & (int)type];
            }
            else
            {
                return ChannelSettingsTable[0];
            }
        }

        private void Chat_OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            try
            {
                if (!isHandled)
                {
                    if (outputAllJsons)
                    {
                        try
                        {
                            JsonLog(new OnChatMessageArgs(type, senderId, sender, message));
                        }
                        catch (Exception e)
                        {
                            ErrorLog(e.ToString());
                        }
                    }

                    ChannelSettings channel = GetChannelSettings(type);

                    if (channel.Name == "None" && type != XivChatType.None)
                    {
                        DebugLog($"{type.ToString()} ({(int)type})\t{message.TextValue}");
                    }

                    var splitPayloads = new List<List<Payload>>();
                    var currentPayloadList = new List<Payload>();

                    foreach (var payload in message.Payloads)
                    {
                        if (payload.Type == PayloadType.RawText && ((TextPayload)payload).Text.Contains('\n'))
                        {
                            var newPayloads = ((TextPayload)payload).Text.Split('\n').Select(x => new TextPayload(x)).ToList();

                            currentPayloadList.Add(newPayloads[0]);
                            splitPayloads.Add(currentPayloadList);
                            for (int i = 1; i < newPayloads.Count - 1; i++)
                            {
                                splitPayloads.Add(new List<Payload>
                                {
                                    newPayloads[i]
                                });
                            }
                            currentPayloadList = new List<Payload>
                            {
                                newPayloads[newPayloads.Count - 1]
                            };
                        }
                        else
                        {
                            currentPayloadList.Add(payload);
                        }
                    }
                    splitPayloads.Add(currentPayloadList);

                    var textLogEntries = new List<TextLogEntry>();
                    textLogEntries.Add(CreateTextLogEntry(channel, senderId, sender.Payloads, splitPayloads.First()));
                    
                    foreach (var payloads in splitPayloads.Skip(1))
                    {
                        textLogEntries.Add(CreateTextLogEntry(channel, 0, null, payloads));
                    }

                    foreach (var textLogEntry in textLogEntries)
                    {
                        chatBuffer.Enqueue(textLogEntry);

                        while (chatBuffer.Count > 200000)
                        {
                            TextLogEntry removedLogEntry;
                            chatBuffer.TryDequeue(out removedLogEntry);

                            foreach (var tab in tabs)
                            {
                                if (tab.EnabledChannels.ContainsKey(channel.Name) && tab.EnabledChannels[channel.Name])
                                {
                                    tab.FilteredLogs.RemoveAt(0);
                                }
                            }
                        }

                        foreach (var tab in tabs)
                        {
                            if (tab.EnabledChannels.ContainsKey(channel.Name))
                            {
                                if (tab.EnabledChannels[channel.Name])
                                {
                                    tab.AddLine(textLogEntry);
                                }
                            }
                            else
                            {
                                ErrorLog($"Channel name not found: {channel.Name}");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                PluginLog.LogError(e.ToString());
                if (outputErrorJsons)
                {
                    JsonLogError(new OnChatMessageArgs(type, senderId, sender, message));
                }
            }
        }

        public TextLogEntry CreateTextLogEntry(ChannelSettings channel, uint senderId, List<Payload> senderPayLoads, List<Payload> payloads)
        {
            ChatText text = new ChatText
            {
                Channel = channel,
                SenderId = senderId,
                Timestamp = DateTime.Now,
                IncludePrefix = true,
                Sender = null
            };

            if (senderPayLoads != null)
            {
                var senderInfo = ProcessPayloads(senderPayLoads, channel);

                if (senderInfo.Count() > 0)
                {
                    var sender = senderInfo[0];
                    switch (channel.Name)
                    {
                        case "StandardEmote":
                            break;
                        case "CustomEmote":
                            text.Sender = sender;
                            break;
                        case "TellOutgoing":
                            sender.Text = $">> {sender.Text}: ";
                            text.Sender = sender;
                            break;
                        case "TellIncoming":
                            sender.Text = $"{sender.Text} >> ";
                            text.Sender = sender;
                            break;
                        case "Party":
                            sender.Text = $"({sender.Text}) ";
                            text.Sender = sender;
                            break;
                        default:
                            sender.Text = $"{sender.Text}: ";
                            text.Sender = sender;
                            break;
                    }
                }
            }
            else
            {
                text.IncludePrefix = false;
            }

            text.Text = ProcessPayloads(payloads, channel);

            TextLogEntry textLogEntry = new TextLogEntry(text);

            return textLogEntry;
        }

        public List<TextTypes> ProcessPayloads(List<Payload> payloads, ChannelSettings channel)
        {
            List<TextTypes> result = new List<TextTypes>();
            var prevType = (PayloadType)(-1);

            int i = 0;
            while (i < payloads.Count)
            {
                var currentPayload = payloads[i];
                var payloadType = currentPayload.Type;
                var skipPayload = false;
                var movePastLink = false;

                var textItem = new TextTypes
                {
                    Payload = currentPayload,
                    Type = payloadType
                };

                switch (payloadType)
                {
                    case PayloadType.Player:
                        var playerPayload = (PlayerPayload)currentPayload;
                        var playerName = ((TextPayload)payloads[i + 1]).Text;
                        textItem.Text = $"{playerName}\uE500{playerPayload.World.Name}";
                        movePastLink = true;
                        break;

                    case PayloadType.MapLink:
                        var mapTextPayload = (TextPayload)payloads[i + 6];
                        textItem.Text = mapTextPayload.Text;
                        movePastLink = true;
                        break;

                    case PayloadType.Status:
                        var statusPayload = (StatusPayload)currentPayload;
                        textItem.Text = statusPayload.Status.Name;
                        movePastLink = true;
                        break;

                    case PayloadType.Item:
                        var itemPayload = (ItemPayload)currentPayload;
                        if (channel.Name != "Synthesis")
                        {
                            textItem.Text = itemPayload.Item.Name;
                        }
                        else if (itemPayload.Item.StackSize > 1)
                        {
                            textItem.Text = itemPayload.Item.Plural;
                        }
                        else
                        {
                            textItem.Text = itemPayload.Item.Singular;
                        }
                        textItem.Text = Regex.Replace(textItem.Text, @"[\u0000-\u001F]+", string.Empty);

                        movePastLink = true;
                        break;

                    case PayloadType.RawText:
                        var textPayload = (TextPayload)currentPayload;
                        if (prevType == PayloadType.Player)
                        {
                            if (textPayload.Text.Contains(' '))
                            {
                                textItem.Text = textPayload.Text.Substring(textPayload.Text.IndexOf(' '));
                            }
                            else
                            {
                                skipPayload = true;
                            }
                        }
                        else
                        {
                            textItem.Text = textPayload.Text;
                        }
                        i++;
                        break;

                    default:
                        skipPayload = true;
                        i++;
                        break;
                }

                if (!skipPayload)
                {
                    prevType = payloadType;
                    result.Add(textItem);
                }
                if (movePastLink)
                {
                    for (; i < payloads.Count(); i++)
                    {
                        var nextPayload = payloads[i];

                        if (nextPayload.Type == PayloadType.Unknown)
                        {
                            var unknownPayload = (RawPayload)nextPayload;

                            if (unknownPayload.Data.SequenceEqual(RawPayload.LinkTerminator.Data))
                            {
                                i++;
                                break;
                            }
                        }
                    }
                }
            }

            return result;
        }

        public void Dispose()
        {
            pluginInterface.Framework.Gui.Chat.OnChatMessage -= Chat_OnChatMessage;
            pluginInterface.CommandManager.RemoveHandler("/cht");
            this.pluginInterface.UiBuilder.OnBuildUi -= ChatUI;
            pluginInterface.UiBuilder.OnOpenConfigUi -= Chat_ConfigWindow;
            this.pluginInterface.UiBuilder.OnBuildFonts -= AddFonts;
        }

        bool CheckDupe(List<TabBase> items, string title)
        {
            foreach (var tab in items)
            {
                if (title == tab.Title) { return true; }
            }
            return false;
        }
        
        private void DebugLog(string s)
        {
            var logPath = Path.Combine(dllPath, "Logs");
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            File.AppendAllText(Path.Combine(logPath, DateTime.Now.ToString(@"yyyy_MM_dd") + ".txt"), s + "\r\n");
        }

        private void JsonLog(object o)
        {
            var jsonPath = Path.Combine(dllPath, @"Payloads\All", DateTime.Now.ToString(@"yyyy/MM/dd"));
            if (!Directory.Exists(jsonPath))
            {
                Directory.CreateDirectory(jsonPath);
            }
            File.WriteAllText(Path.Combine(jsonPath, DateTime.Now.Ticks + ".json"), JsonConvert.SerializeObject(o, Formatting.Indented, new JsonSerializerSettings()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto
            }));
        }

        private void JsonLogError(object o)
        {
            var jsonPath = Path.Combine(dllPath, @"Payloads\Error", DateTime.Now.ToString(@"yyyy/MM/dd"));
            if (!Directory.Exists(jsonPath))
            {
                Directory.CreateDirectory(jsonPath);
            }
            File.WriteAllText(Path.Combine(jsonPath, DateTime.Now.Ticks + ".json"), JsonConvert.SerializeObject(o, Formatting.Indented, new JsonSerializerSettings()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto
            }));
        }

        private void ErrorLog(string s)
        {
            var errorPath = Path.Combine(dllPath, "Errors");
            if (!Directory.Exists(errorPath))
            {
                Directory.CreateDirectory(errorPath);
            }
            File.AppendAllText(Path.Combine(errorPath, DateTime.Now.ToString(@"yyyy_MM_dd") + ".txt"), s + "\r\n");
        }
    }
}
