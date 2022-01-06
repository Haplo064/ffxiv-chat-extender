using Dalamud.Configuration;
using Dalamud.Game.Chat;
using Dalamud.Game.Chat.SeStringHandling;
using Dalamud.Game.Chat.SeStringHandling.Payloads;
using Dalamud.Plugin;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DalamudPlugin
{
    public partial class ChatExtenderPlugin : IDalamudPlugin
    {
        public class ChatExtenderPluginConfiguration : IPluginConfiguration
        {
            public int Version { get; set; } = 0;
            public bool ShowChatWindow = true;
            public float Alpha = 0.5f;
            public bool NoMouse = false;
            public bool NoMouse2 = false;
            public bool NoMove = false;
            public bool NoResize = false;
            public bool NoScrollBar = false;
            public bool AllowTranslation = false;
            public bool BubblesWindow = false;
            public float FontSize = 14;
            public bool HourTime;
            public bool FontShadow;
            public Dictionary<int, ChannelSettings> ChannelSettings;
            public List<TabBase> Tabs;

            public Vector4 TimeColor
            {
                get { return TimeColorRef.Color; }
                set { TimeColorRef.Color = value; }
            }
            public Vector4 TimeShadowColor
            {
                get { return TimeColorShadowRef.Color; }
                set { TimeColorShadowRef.Color = value; }
            }

            [JsonIgnore]
            public ColorRef TimeColorRef = new ColorRef(new Vector4(1, 1, 1, 1));

            [JsonIgnore]
            public ColorRef TimeColorShadowRef = new ColorRef(new Vector4(0, 0, 0, 1));
        }

        public class BoolRef
        {
            public bool Value;

            public static implicit operator bool(BoolRef b) => b.Value;
            public static implicit operator BoolRef(bool b) => new BoolRef { Value = b };
        }

        public class TabBase
        {
            public string Title;

            [JsonIgnore]
            public List<TextLogEntry> FilteredLogs = new List<TextLogEntry>();

            [JsonIgnore]
            private List<int> CumulativeLengths = new List<int>();

            [JsonIgnore]
            public List<TextLogEntry> NeedsUpdateLogs = new List<TextLogEntry>();

            [JsonIgnore]
            public bool needsRecomputeCumulativeLengths;

            public bool Enabled;

            public Dictionary<string, BoolRef> EnabledChannels;
            public Dictionary<string, BoolRef> ShowChannelTag;
            
            public bool Timestamps = true;
            public bool ShowChannelTagAll = false;
            public bool AutoScroll = true;
            public bool SaveToFile = false;
            public bool ScrollOnce = false;
            public string Filter = "";
            public bool FilterOn = false;
            public bool msg = false;
            public bool sel = false;

            private Task updateFilterTask;
            private bool cancelTask = false;

            public void UpdateFilteredLines()
            {
                if (updateFilterTask != null)
                {
                    cancelTask = true;
                    updateFilterTask.Wait();
                    cancelTask = false;
                }
                updateFilterTask = Task.Run(() =>
                {
                    FilteredLogs = new List<TextLogEntry>();

                    foreach (var chatLine in chatBuffer)
                    {
                        if (cancelTask)
                        {
                            return;
                        }
                        if (EnabledChannels[chatLine.line.Channel.Name])
                        {
                            if (CheckLineFilter(string.Join("", chatLine.Text.Select(x => x.Text))))
                            {
                                FilteredLogs.Add(chatLine);
                            }
                        }
                    }

                    needsRecomputeCumulativeLengths = true;
                    ScrollOnce = AutoScroll;
                });
            }

            public List<int> GetCumulativeLineSum()
            {
                if (needsRecomputeCumulativeLengths)
                {
                    CumulativeLengths = RecomputeCumulativeLineSum().ToList();
                    needsRecomputeCumulativeLengths = false;
                }

                return CumulativeLengths;
            }

            private IEnumerable<int> RecomputeCumulativeLineSum()
            {
                var sum = 0;
                foreach (var line in FilteredLogs)
                {
                    sum += line.ApproximateWrappedLineCount;
                    yield return sum;
                }
            }

            public void AddLine(TextLogEntry line)
            {
                if (line.Text == null)
                {
                    return;
                }
                
                if (CheckLineFilter(string.Join("", line.Text.Select(x => x.Text))))
                {
                    NeedsUpdateLogs.Add(line);
                    ScrollOnce = AutoScroll;
                }
            }

            public void ComputeNewLineSum()
            {
                if (NeedsUpdateLogs.Count > 0)
                {
                    foreach (var line in NeedsUpdateLogs)
                    {
                        CumulativeLengths.Add(CumulativeLengths.LastOrDefault() + line.ApproximateWrappedLineCount);
                        FilteredLogs.Add(line);
                    }

                    NeedsUpdateLogs = new List<TextLogEntry>();
                }
            }

            private bool CheckLineFilter(string text)
            {
                try
                {
                    return
                        !FilterOn ||
                        string.IsNullOrWhiteSpace(Filter) ||
                        CultureInfo.InvariantCulture.CompareInfo.IndexOf(text, Filter, CompareOptions.IgnoreCase) >= 0 ||
                        Regex.Matches(text, Filter, RegexOptions.IgnoreCase | RegexOptions.Compiled).Count > 0;
                }
                catch
                {
                    // Invalid regex string, ignore the exception
                    return false;
                }
            }
        }

        public class TextTypes
        {
            public string Text;
            public PayloadType Type;
            public Payload Payload;
            public bool IsHovered = false;
            public uint highlightColor;

            public void SetHighlightColor(Vector4 rgb1, Vector4 rgb2)
            {
                var c1 = Color.FromArgb((int)(255 * rgb1.X), (int)(255 * rgb1.Y), (int)(255 * rgb1.Z));
                var h1 = c1.GetHue();
                var s1 = c1.GetSaturation();
                var b1 = c1.GetBrightness();

                var c2 = Color.FromArgb((int)(255 * rgb2.X), (int)(255 * rgb2.Y), (int)(255 * rgb2.Z));
                var h2 = c2.GetHue();
                var s2 = c2.GetSaturation();
                var b2 = c2.GetBrightness();

                var r = 1f;
                var g = 1f;
                var b = 1f;

                if (s1 == 0 && s2 == 0)
                {
                    highlightColor = 0x60FFFFFF;
                    return;
                }

                var d1 = Math.Max(h1, h2) - Math.Min(h1, h2);
                var d2 = 360 - Math.Max(h1, h2) + Math.Min(h1, h2);

                var h = 0f;

                if (d1 > d2)
                {
                    if (h1 < h2)
                    {
                        h1 += 360;
                    }
                    else
                    {
                        h2 += 360;
                    }
                };

                h = (((s1 * h1 + s2 * h2) / (s1 + s2)) % 360) / 360;

                var s = Math.Max(s1, s2);

                var H = h * 6;
                var F = H - (int)H;
                var V1 = 1 - s;
                var V2 = 1 - s * F;
                var V3 = 1 - s * (1 - F);

                if ((int)H == 0) { r = 1; g = V3; b = V1; }
                else if ((int)H == 1) { r = V2; g = 1; b = V1; }
                else if ((int)H == 2) { r = V1; g = 1; b = V3; }
                else if ((int)H == 3) { r = V1; g = V2; b = 1; }
                else if ((int)H == 4) { r = V3; g = V1; b = 1; }
                else { r = 1; g = V1; b = V2; }

                var R = (uint)(Math.Max(255 * r, 128));
                var G = (uint)(Math.Max(255 * g, 128));
                var B = (uint)(Math.Max(255 * b, 128));

                highlightColor = 0x60000000 | (B << 16) | (G << 8) | R;
            }
        }

        public class OnChatMessageArgs
        {
            public XivChatType Type { get; set; }
            public uint SenderId { get; set; }
            public SeString Sender { get; set; }
            public SeString Message { get; set; }

            public OnChatMessageArgs(XivChatType type, uint senderId, SeString sender, SeString message)
            {
                this.Type = type;
                this.SenderId = senderId;
                this.Sender = sender;
                this.Message = message;
            }
        }

        public class ColorRef
        {
            public Vector4 Color;

            public ColorRef(Vector4 color)
            {
                this.Color = color;
            }

            public static implicit operator Vector4(ColorRef c) => c.Color;
        }

        public class ColorString
        {
            public string Text;
            public Vector4 Color;
            public Vector4 ShadowColor;
            public ColorRef ReferenceColor;
            public ColorRef ReferenceShadowColor;
            public bool UseChannelColors;
            public TextTypes SourcePayloadContainer;

            private static string delimiterRegex = "( +)";

            public ColorString() { }

            public ColorString(Vector4 color1, Vector4 color2, TextTypes source)
            {
                this.SourcePayloadContainer = source;
                if (this.SourcePayloadContainer != null)
                {
                    this.SourcePayloadContainer.SetHighlightColor(color1, color2);
                }
            }

            public ColorString(ColorString copy, string newText)
            {
                this.Text = newText;
                this.Color = copy.Color;
                this.ShadowColor = copy.ShadowColor;
                this.ReferenceColor = copy.ReferenceColor;
                this.ReferenceShadowColor = copy.ReferenceShadowColor;
                this.UseChannelColors = copy.UseChannelColors;
                this.SourcePayloadContainer = copy.SourcePayloadContainer;
            }

            private static ColorString MakeLinkChar(TextTypes source = null)
            {
                return new ColorString { Text = "\uE0BB", Color = new Vector4(1, 0.41568f, 0.06274f, 1), ShadowColor = new Vector4(0, 0, 0, 1), SourcePayloadContainer = source };
            }

            private static ColorString MakeCrossWorldChar(TextTypes source = null)
            {
                return new ColorString { Text = "\uE500", Color = new Vector4(1, 1, 1, 1), ShadowColor = new Vector4(0.2f, 0.16f, 0.745f, 1) };
            }

            private static ColorString MakeBuffChar(TextTypes source = null)
            {
                return new ColorString { Text = "\uE05C", Color = new Vector4(0.33333f, 0.79215f, 1, 1), ShadowColor = new Vector4(0, 0, 0, 1), SourcePayloadContainer = source };
            }

            private static ColorString MakeDebuffChar(TextTypes source = null)
            {
                return new ColorString { Text = "\uE05B", Color = new Vector4(1, 0, 0, 1), ShadowColor = new Vector4(0, 0, 0, 1), SourcePayloadContainer = source };
            }

            private static ColorString MakeCollectibleChar(Vector4 color1, Vector4 color2, TextTypes source = null)
            {
                return new ColorString { Text = "\uE03D", Color = color1, ShadowColor = color2, SourcePayloadContainer = source };
            }

            private static ColorString MakeHqChar(Vector4 color1, Vector4 color2, TextTypes source = null)
            {
                return new ColorString { Text = "\uE03C", Color = color1, ShadowColor = color2, SourcePayloadContainer = source };
            }

            private static IEnumerable<ColorString> FromStringSplitDelimiters(string str, ColorRef color, ColorRef shadowColor, TextTypes source = null)
            {
                return Regex.Split(str, delimiterRegex, RegexOptions.Compiled).Select(x =>
                {
                    return new ColorString(color, shadowColor, source)
                    {
                        Text = x,
                        ReferenceColor = color,
                        ReferenceShadowColor = shadowColor
                    };
                });
            }

            private static IEnumerable<ColorString> FromStringSplitDelimiters(string str, Vector4 color, Vector4 shadowColor, TextTypes source = null)
            {
                return Regex.Split(str, delimiterRegex, RegexOptions.Compiled).Select(x =>
                {
                    return new ColorString(color, shadowColor, source)
                    {
                        Text = x,
                        Color = color,
                        ShadowColor = shadowColor
                    };
                });
            }

            public static IEnumerable<ColorString> FromString(string str, ColorRef color, ColorRef shadowColor, TextTypes source = null)
            {
                return FromStringSplitDelimiters(str, color, shadowColor, source);
            }

            public static IEnumerable<ColorString> PlayerName(string str, ColorRef color, ColorRef shadowColor, PlayerPayload player, TextTypes source = null)
            {
                if (str.Contains('\uE500'))
                {
                    var parts = Regex.Split(str, $"(\uE500|{player.PlayerName})");

                    foreach (var p in parts)
                    {
                        if (p == "\uE500")
                        {
                            yield return MakeCrossWorldChar(null);
                        }
                        else if (p == player.PlayerName)
                        {
                            foreach (var s in FromStringSplitDelimiters(p, color, shadowColor, source))
                            {
                                yield return s;
                            }
                        }
                        else
                        {
                            foreach (var s in FromStringSplitDelimiters(p, color, shadowColor, null))
                            {
                                yield return s;
                            }
                        }
                    }
                }
                else
                {
                    foreach (var s in FromStringSplitDelimiters(str, color, shadowColor, source))
                    {
                        yield return s;
                    }
                }
            }

            public static IEnumerable<ColorString> FromMapLink(string str, ColorRef color, ColorRef shadowColor, MapLinkPayload map, TextTypes source)
            {
                yield return MakeLinkChar(source);
                foreach (var s in FromStringSplitDelimiters(str, color, shadowColor, source))
                {
                    yield return s;
                }
            }

            public static IEnumerable<ColorString> FromStatus(string str, ColorRef color, ColorRef shadowColor, StatusPayload status, TextTypes source)
            {
                 yield return MakeLinkChar(source);
                if (status.Status.Category == 1) // Buff
                {
                   yield return MakeBuffChar(source);
                }
                else if (status.Status.Category == 2) // Debuff
                {
                    yield return MakeDebuffChar(source);
                }
                foreach (var s in FromStringSplitDelimiters(str, color, shadowColor, source))
                {
                    yield return s;
                }
            }

            public static IEnumerable<ColorString> FromItem(string str, ItemPayload item, TextTypes source)
            {
                var color1 = new Vector4(1, 1, 1, 1);
                var color2 = new Vector4(0, 0, 0, 1);

                switch (item.Item.Rarity)
                {
                    case 2:
                        color1 = new Vector4(0.6549f, 0.92156f, 0.6549f, 1);
                        break;
                    case 3:
                        color1 = new Vector4(0.21568f, 0.46274f, 0.92156f, 1);
                        break;
                    case 4:
                        color1 = new Vector4(0.6f, 0.44705f, 0.92156f, 1);
                        break;
                    default:
                        break;
                }

                yield return MakeLinkChar(source);
                foreach (var s in FromStringSplitDelimiters(str, color1, color2, source))
                {
                    yield return s;
                }
                if (item.Item.IsCollectable)
                {
                    yield return MakeCollectibleChar(color1, color2, source);
                }
                if (item.IsHQ)
                {
                    yield return MakeHqChar(color1, color2, source);
                }
            }

            public override string ToString()
            {
                return Text;
            }
        }

        public class TextLogEntry
        {
            public ChatText line;

            public List<TextTypes> Text { get { return line?.Text; } }

            public int ApproximateWrappedLineCount
            {
                get
                {
                    if (Text == null)
                    {
                        return 0;
                    }
                    var fullString = ToString();

                    ImGui.PushFont(font);
                    var fullWidth = ImGui.CalcTextSize(fullString).X;
                    ImGui.PopFont();

                    return (int)Math.Ceiling(fullWidth / (windowSize.X - 50));
                }
            }

            public TextLogEntry(ChatText t)
            {
                this.line = t;
            }

            public List<List<ColorString>> GetLines()
            {
                if (activeTab == null)
                {
                    return null;
                }

                var fullLineStrings = new List<ColorString>();
                var channelColor1 = line.Channel.FontColorRef;
                var channelColor2 = line.Channel.OutlineColorRef;
                var tab = activeTab;

                if (tab.Timestamps && line.IncludePrefix)
                {
                    fullLineStrings.AddRange(ColorString.FromString(line.TimeStr, config.TimeColorRef, config.TimeColorShadowRef));
                }
                if (tab.ShowChannelTagAll && tab.ShowChannelTag[line.Channel.Name] && line.IncludePrefix)
                {
                    fullLineStrings.AddRange(ColorString.FromString(line.Channel.ShortName, channelColor1, channelColor2));
                }
                if (line.Sender != null && line.IncludePrefix)
                {
                    if (line.Sender.Payload.Type == PayloadType.Player)
                    {
                        fullLineStrings.AddRange(ColorString.PlayerName(line.Sender.Text, channelColor1, channelColor2, (PlayerPayload)line.Sender.Payload, line.Sender));
                    }
                    else
                    {
                        fullLineStrings.AddRange(ColorString.PlayerName(line.Sender.Text, channelColor1, channelColor2, null, line.Sender));
                    }
                }

                fullLineStrings.AddRange(line.Text.SelectMany(x =>
                {
                    switch (x.Type)
                    {
                        case PayloadType.RawText:
                            return ColorString.FromString(x.Text, channelColor1, channelColor2);
                        case PayloadType.Player:
                            return ColorString.PlayerName(x.Text, channelColor1, channelColor2, (PlayerPayload)x.Payload, x);
                        case PayloadType.MapLink:
                            return ColorString.FromMapLink(x.Text, channelColor1, channelColor2, (MapLinkPayload)x.Payload, x);
                        case PayloadType.Status:
                            return ColorString.FromStatus(x.Text, channelColor1, channelColor2, (StatusPayload)x.Payload, x);
                        case PayloadType.Item:
                            return ColorString.FromItem(x.Text, (ItemPayload)x.Payload, x);
                        default:
                            return new List<ColorString>();
                    }
                }));

                return ComputeWrapLines(fullLineStrings, windowSize.X - 50);
            }

            public List<List<ColorString>> ComputeWrapLines(List<ColorString> fullLine, float textWidth)
            {
                var wrappedLines = new List<List<ColorString>>();
                var currentLine = new List<ColorString>();
                var currentWidth = 0f;

                foreach (var part in fullLine)
                {
                    var partWidth = ImGui.CalcTextSize(part.Text).X;
                    if (currentWidth + partWidth < textWidth)
                    {
                        currentLine.Add(part);
                        currentWidth += partWidth;
                    }
                    else
                    {
                        if (partWidth > textWidth)
                        {
                            var partString = part.Text;
                            int i = FindIndexUnderWidth(partString, 0, partString.Length, textWidth - currentWidth);
                            part.Text = partString.Substring(0, i);
                            currentLine.Add(part);
                            wrappedLines.Add(currentLine);

                            var remainingString = partString.Substring(i);
                            while (remainingString.Length > 0)
                            {
                                i = FindIndexUnderWidth(remainingString, 0, remainingString.Length, textWidth);
                                if (i == 0)
                                {
                                    i = 1;
                                }

                                if (i >= remainingString.Length)
                                {
                                    currentWidth = ImGui.CalcTextSize(remainingString).X;
                                    currentLine = new List<ColorString> { new ColorString (part, remainingString) };
                                    remainingString = "";
                                }
                                else
                                {
                                    var nextPart = remainingString.Substring(0, i);
                                    currentLine = new List<ColorString> { new ColorString(part, nextPart) };
                                    wrappedLines.Add(currentLine);
                                    remainingString = remainingString.Substring(i);
                                }
                            }
                        }
                        else
                        {
                            wrappedLines.Add(currentLine);
                            currentLine = new List<ColorString> { part };
                            currentWidth = partWidth;
                        }
                    }
                }

                if (currentLine.Count > 0)
                {
                    wrappedLines.Add(currentLine);
                }

                return wrappedLines;
            }

            private int FindIndexUnderWidth(string s, int startIndex, int substringLength, float remainingWidth)
            {
                if (substringLength == 0 || remainingWidth <= 0 || startIndex >= s.Length)
                {
                    return startIndex;
                }

                var substring = s.Substring(startIndex, substringLength);
                var substringWidth = ImGui.CalcTextSize(substring).X;

                if (substringWidth > remainingWidth)
                {
                    return FindIndexUnderWidth(s, startIndex, substringLength / 2, remainingWidth);
                }
                else
                {
                    return FindIndexUnderWidth(s, startIndex + substringLength, substringLength / 2, remainingWidth - substringWidth);
                }
            }

            public override string ToString()
            {
                if (Text != null && activeTab != null)
                {
                    var prefix = (activeTab.Timestamps ? line.TimeStr : "") +
                        (activeTab.ShowChannelTagAll && activeTab.ShowChannelTag[line.Channel.Name] ? line.Channel.ShortName : "") +
                        line.Sender?.Text ?? "";

                    return prefix + string.Join("", line.Text.Select(x => x.Text));
                }
                return "";
            }
        }
        
        public class ChannelSettings
        {
            public string Name;
            public string ShortName;
            public Vector4 FontColor
            {
                get { return FontColorRef.Color; }
                set { FontColorRef.Color = value; }
            }
            public Vector4 OutlineColor
            {
                get { return OutlineColorRef.Color; }
                set { OutlineColorRef.Color = value; }
            }

            [JsonIgnore]
            public ColorRef FontColorRef;
            [JsonIgnore]
            public ColorRef OutlineColorRef;

            public ChannelSettings(string channelName, string shortName, Vector4 fontColor, Vector4 outlineColor)
            {
                this.Name = channelName;
                this.ShortName = shortName;
                this.FontColorRef = new ColorRef(fontColor);
                this.OutlineColorRef = new ColorRef(outlineColor);
            }

            public void Update(ChannelSettings newSettings)
            {
                this.ShortName = newSettings.ShortName;
                this.FontColor = newSettings.FontColor;
                this.OutlineColor = newSettings.OutlineColor;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public class ChatText
        {
            public string TimeStr { get { return Timestamp.ToString(config.HourTime ? "[HH:mm]" : "[hh:mm]"); } }
            public ChannelSettings Channel;
            public TextTypes Sender;
            public List<TextTypes> Text = new List<TextTypes>();
            public DateTime Timestamp;
            public uint SenderId;
            public bool IncludePrefix;
        }
    }
}
