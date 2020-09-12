using Dalamud;
using Dalamud.Data;
using Dalamud.Game.Chat;
using Dalamud.Game.Chat.SeStringHandling;
using Dalamud.Game.Chat.SeStringHandling.Payloads;
using DalamudPlugin;
using ImGuiNET;
using ImGuiScene;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using static DalamudPlugin.ChatExtenderPlugin;
using static SDL2.SDL;

namespace ChatExtenderTest
{
    /// Test class for debugging UI issues without needing FFXIV
    class Program
    {
        static DataManager dataManager;
        static ChatExtenderPlugin plugin;
        static MethodInfo renderUI;
        static MethodInfo onChat;
        static IEnumerable<OnChatMessageArgs> messages;
        static IEnumerator<OnChatMessageArgs> messageEnumerator;
        static string appDataPath;

        static void Main(string[] args)
        {
            appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            dataManager = new DataManager(ClientLanguage.English);

            var t = Task.Run(() =>
            {
                // The executing directory of this program needs a directory junction to the FFXIV sqpack folder so that Lumina can find the files
                // (You can also copy the files over, but it is around 45GB of data)
                dataManager.Initialize(appDataPath + @"\XIVLauncher\addon\Hooks");
            });
            t.Wait();

            plugin = new ChatExtenderPlugin();
            config = new ChatExtenderPluginConfiguration();
            renderUI = plugin.GetType().GetMethod("RenderUI", BindingFlags.NonPublic | BindingFlags.Instance);
            onChat = plugin.GetType().GetMethod("Chat_OnChatMessage", BindingFlags.NonPublic | BindingFlags.Instance);


            var tab = new TabBase();
            tab.Title = "New Tab";
            tabs.Add(tab);
            activeTab = tab;

            tab.EnabledChannels = new Dictionary<string, BoolRef>();
            tab.ShowChannelTag = new Dictionary<string, BoolRef>();

            foreach(var key in ChannelSettingsTable.Keys)
            {
                var channelName = ChannelSettingsTable[key].Name;
                tab.EnabledChannels.Add(channelName, true);
                tab.ShowChannelTag.Add(channelName, true);
            }

            var configWindow = plugin.GetType().GetField("configWindow", BindingFlags.NonPublic | BindingFlags.Instance);

            var messagePaths = Directory.GetFiles(appDataPath + @"\XIVLauncher\installedPlugins\ChatExtender\2.0.2.4\Payloads\All\2020\09\11");
            messages = messagePaths.Select(x =>
            {
                var a = JsonConvert.DeserializeObject<OnChatMessageArgs>(File.ReadAllText(x), new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    TypeNameHandling = TypeNameHandling.Auto,
                    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
                });

                foreach (var payload in a.Sender.Payloads)
                {
                    payload.DataResolver = dataManager;
                }
                foreach (var payload in a.Message.Payloads)
                {
                    payload.DataResolver = dataManager;
                }

                return a;
            });
            //.Where(x => x.Sender.Payloads.Count() > 0);
            //.Where(x => x.Type == XivChatType.Party);
            messageEnumerator = messages.GetEnumerator();

            using (var scene = SimpleImGuiScene.CreateOverlay(RendererFactory.RendererBackend.DirectX11))
            {
                scene.Window.OnSDLEvent += (ref SDL_Event sdlEvent) =>
                {
                    if (sdlEvent.type == SDL_EventType.SDL_KEYDOWN && sdlEvent.key.keysym.scancode == SDL_Scancode.SDL_SCANCODE_ESCAPE)
                    {
                        scene.ShouldQuit = true;
                    }
                    if (sdlEvent.type == SDL_EventType.SDL_KEYDOWN && sdlEvent.key.keysym.scancode == SDL_Scancode.SDL_SCANCODE_SPACE)
                    {
                        SendMessage();
                    }
                };
                
                var addFonts = plugin.GetType().GetMethod("AddFonts", BindingFlags.NonPublic | BindingFlags.Instance);
                addFonts.Invoke(plugin, new object[] { });

                configWindow.SetValue(plugin, true);

                plugin.Alpha = 1;

                scene.Renderer.ClearColor = new Vector4(0, 0, 0, 1);
                scene.OnBuildUI += Display;
                scene.Run();
            }

            var messageList = messages.ToList();
        }

        static void SendMessage()
        {
            for (int i = 0; i < 100; i++)
            {
                if (messageEnumerator.MoveNext())
                {
                    var message = messageEnumerator.Current;
                    onChat.Invoke(plugin, new object[] { message.Type, message.SenderId, message.Sender, message.Message, false });
                }
            }
        }

        static void Display()
        {
            ImGui.ShowStyleEditor();
            ImGui.ShowDemoWindow();
            renderUI.Invoke(plugin, new object[] { });
        }
    }
}
