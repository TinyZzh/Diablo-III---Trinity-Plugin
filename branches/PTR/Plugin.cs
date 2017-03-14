//!CompilerOption:AddRef:System.Management.dll
//!CompilerOption:AddRef:System.Web.Extensions.dll

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using Buddy.Overlay;
using Trinity.Coroutines.Town;
using Trinity.DbProvider;
using Trinity.Framework;
using Trinity.Framework.Helpers;
using Trinity.Framework.Objects.Enums;
using Trinity.Items;
using Trinity.ProfileTags;
using Trinity.Reference;
using Trinity.Settings;
using Trinity.UI;
using Trinity.UI.Visualizer;
using Zeta.Bot;
using Zeta.Bot.Navigation;
using Zeta.Bot.Settings;
using Zeta.Common.Plugins;
using Zeta.Game;
using Application = System.Windows.Application;
using Zeta.Game.Internals;

namespace Trinity
{
    public class TrinityPlugin : IPlugin
    {
        private static TrinityPlugin _instance;
        public string Name => "Trinity PTR";
        public Version Version => new Version(2, 250, 738);
        public string Author => "xzjv, TarasBulba, rrrix, jubisman, Phelon and many more";
        public string Description => $"v{Version} provides combat, exploration and much more";
        public Window DisplayWindow => UILoader.GetDisplayWindow(Path.Combine(FileManager.PluginPath, "UI"));

        public TrinityPlugin()
        {
            _instance = this;

            UILoader.Preload();

            if (CharacterSettings.Instance.EnabledPlugins == null)
                CharacterSettings.Instance.EnabledPlugins = new List<string>();

            if (!CharacterSettings.Instance.EnabledPlugins.Contains("Trinity"))
                CharacterSettings.Instance.EnabledPlugins.Add("Trinity");
        }

        public DateTime LastPulse { get; set; }
        public static bool IsEnabled { get; private set; }
        public bool IsInitialized { get; private set; }
        public static TrinityPlugin Instance => _instance ?? (_instance = new TrinityPlugin());

        public void OnPulse()
        {
            try
            {
                LastPulse = DateTime.UtcNow;
                HookManager.CheckHooks();

                if (ZetaDia.Me == null)
                    return;

                if (!ZetaDia.IsInGame || !ZetaDia.Me.IsValid || ZetaDia.Globals.IsLoadingWorld)
                    return;

                GameUI.SafeClickUIButtons();
                VisualizerViewModel.Instance.UpdateVisualizer();               
            }
            catch (AccessViolationException)
            {
                // woof! 
            }
            catch (Exception ex)
            {
                Logger.LogDebug(LogCategory.UserInformation, $"Exception in Pulse: {ex}");
            }
        }

        public void OnEnabled()
        {
            if (IsEnabled)
                return;

            // When DB is started via CMD line argument and includes a login then plugins 
            // are Enabled by BotMain thread instead of UI thread, causing problems.
            // This only effects regloggers. YARBot handles re-enable from the correct thread.
            if (!Application.Current.CheckAccess())
                return;

            try
            {
                Core.Init();
                TrinitySettings.InitializeSettings();
                SkillUtils.UpdateActiveSkills();
                TabUi.InstallTab();

                // Turn off DB's inactivity detection.
                GlobalSettings.Instance.LogoutInactivityTime = 0;

                if (!Directory.Exists(FileManager.PluginPath))
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Cannot enable plugin. Invalid path: {0}", FileManager.PluginPath);
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Please check you have installed the plugin to the correct location, and then restart DemonBuddy and re-enable the plugin.");
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, @"Plugin should be installed to \<DemonBuddyFolder>\Plugins\TrinityPlugin\");
                }
                else
                {
                    
                    Navigator.PlayerMover = Core.PlayerMover;
                    Navigator.StuckHandler = Core.StuckHandler;
                    ItemManager.Current = new BlankItemManager();
                    CombatTargeting.Instance.Provider = new TrinityCombatProvider();
                    LootTargeting.Instance.Provider = new BlankLootProvider();
                    ObstacleTargeting.Instance.Provider = new BlankObstacleProvider();
                    Zeta.Bot.RoutineManager.Current = new TrinityRoutine();
                    UILoader.PreLoadWindowContent();
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "ENABLED: {0} now in action!", Description);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error in OnEnable: " + ex);
            }

            IsEnabled = true;
            ModuleManager.Enable();
        }

        public void OnDisabled()
        {
            IsEnabled = false;
            TabUi.RemoveTab();
            HookManager.ReplaceTreeHooks();
            Navigator.PlayerMover = new DefaultPlayerMover();
            Navigator.StuckHandler = new DefaultStuckHandler();
            CombatTargeting.Instance.Provider = new DefaultCombatTargetingProvider();
            LootTargeting.Instance.Provider = new DefaultLootTargetingProvider();
            ObstacleTargeting.Instance.Provider = new DefaultObstacleTargetingProvider();
            ItemManager.Current = new BlankItemManager();
            Zeta.Bot.RoutineManager.Current = null;
            ModuleManager.Disable();
        }

        public void OnShutdown()
        {

        }

        public void OnInitialize()
        {
            if (IsInitialized)
                return;

            // When DB is started via CMD line argument and includes a login then plugins 
            // are initialized by BotMain thread instead of UI thread, causing problems.
            // This only effects regloggers. YARBot handles re-initialization from the correct thread.
            if (!Application.Current.CheckAccess())
                return;

            // DB requires a \Routines\ folder to exist or it shows an error dialog.
            if (!Directory.Exists(FileManager.RoutinesDirectory))
                Directory.CreateDirectory(FileManager.RoutinesDirectory);

            TrinityConditions.Initialize();
            IsInitialized = true;
        }

        public bool Equals(IPlugin other)
        {
            return (other.Name == Name) && (other.Version == Version);
        }

        internal static void Exit()
        {
            ZetaDia.Memory.Process.Kill();

            try
            {
                if (Thread.CurrentThread != Application.Current.Dispatcher.Thread)
                {
                    Application.Current.Dispatcher.Invoke(Exit);
                    return;
                }
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
        }
    }
}