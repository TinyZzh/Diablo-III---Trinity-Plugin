﻿using QuestTools;
using System;
using Trinity.Components.Adventurer;
using Trinity.DbProvider;
using Trinity.Framework.Actors;
using Trinity.Framework.Avoidance;
using Trinity.Framework.Helpers;
using Trinity.Framework.Objects.Enums;
using Trinity.Modules;
using Trinity.Settings;
using Zeta.Bot;
using Zeta.Bot.Navigation;
using Zeta.Game;
using Logger = Trinity.Framework.Helpers.Logger;

namespace Trinity.Framework
{
    public static class Core
    {
        public static void Init()
        {
            Logger.LogNormal("Framework Core Initialized");
        }

        public static bool IsEnabled { get; private set; }
        public static RoutineManager Routines => RoutineManager.Instance;
        public static Adventurer Adventurer { get; } = Adventurer.Instance;
        public static Plugin QuestTools { get; } = new Plugin();
        public static InventoryCache Inventory { get; } = new InventoryCache();
        public static ActorCache Actors { get; } = new ActorCache();
        public static HotbarCache Hotbar { get; } = new HotbarCache();
        public static PlayerCache Player { get; } = new PlayerCache();
        public static BuffsCache Buffs { get; } = new BuffsCache();
        public static TargetsCache Targets { get; } = new TargetsCache();
        public static AvoidanceManager Avoidance { get; } = new AvoidanceManager();
        public static CastStatus CastStatus { get; } = new CastStatus();
        public static Cooldowns Cooldowns { get; } = new Cooldowns();
        public static PlayerHistory PlayerHistory { get; } = new PlayerHistory();
        public static Paragon Paragon { get; } = new Paragon();
        public static StatusBar StatusBar { get; } = new StatusBar();
        public static GameStopper GameStopper { get; } = new GameStopper();
        public static MarkersCache Markers { get; } = new MarkersCache();
        public static MinimapCache Minimap { get; } = new MinimapCache();
        public static WorldCache World { get; } = new WorldCache();
        public static Clusters Clusters { get; } = new Clusters();
        public static SessionLogger SessionLogger { get; } = new SessionLogger();
        public static ItemLogger ItemLogger { get; } = new ItemLogger();
        public static HeroDataCache HeroData { get; } = new HeroDataCache();
        public static QuestCache Quests { get; } = new QuestCache();
        public static GridHelper Grids { get; } = new GridHelper();
        public static PlayerMover PlayerMover { get; } = new PlayerMover();
        public static StuckHandler StuckHandler { get; } = new StuckHandler();
        public static BlockedCheck BlockedCheck { get; } = new BlockedCheck();
        public static ChangeMonitor ChangeMonitor { get; } = new ChangeMonitor();
        public static InactivityMonitor InactivityMonitor { get; } = new InactivityMonitor();
        public static ProfileSettings ProfileSettings { get; } = new ProfileSettings();
        public static SettingsModel Settings => TrinitySettings.Settings;
        public static TrinityStorage Storage => TrinitySettings.Storage;
        public static MainGridProvider DBGridProvider => (MainGridProvider)Navigator.SearchGridProvider;
        public static DefaultNavigationProvider DBNavProvider => (DefaultNavigationProvider)Navigator.NavigationProvider;
        public static bool GameIsReady => ZetaDia.IsInGame && ZetaDia.Me.IsValid && !ZetaDia.Globals.IsLoadingWorld && !ZetaDia.Globals.IsPlayingCutscene;

        internal static void Update()
        {
            ModuleManager.Pulse();
        }
    }
}