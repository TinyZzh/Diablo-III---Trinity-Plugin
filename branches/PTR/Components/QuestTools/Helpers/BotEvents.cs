using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using QuestTools.ProfileTags;
using QuestTools.ProfileTags.Beta;
using QuestTools.ProfileTags.Complex;
using Zeta.Bot;
using Zeta.Bot.Profile;
using Zeta.Bot.Profile.Composites;
using Zeta.Common;
using Zeta.Game;

namespace QuestTools.Helpers
{
    public class BotEvents
    {
        public static DateTime LastBotStart { get; set; }
        public static int GameCount { get; set; }
        public static DateTime LastJoinedGame { get; set; }
        public static DateTime LastProfileReload { get; set; }

        internal static void WireUp()
        {
            BotMain.OnStart += BotMain_OnStart;
            GameEvents.OnPlayerDied += GameEvents_OnPlayerDied;
            GameEvents.OnGameChanged += GameEvents_OnGameChanged;
            GameEvents.OnGameJoined += GameEvents_OnGameJoined;
            GameEvents.OnWorldChanged += GameEvents_OnWorldChanged;
            ProfileManager.OnProfileLoaded += ProfileManager_OnProfileLoaded;
            BotBehaviorQueue.WireUp();
            CustomConditions.Initialize();
        }

        internal static void UnWire()
        {
            BotMain.OnStart -= BotMain_OnStart;
            GameEvents.OnPlayerDied -= GameEvents_OnPlayerDied;
            GameEvents.OnGameChanged -= GameEvents_OnGameChanged;
            GameEvents.OnGameJoined -= GameEvents_OnGameJoined;
            GameEvents.OnWorldChanged -= GameEvents_OnWorldChanged;
            ProfileManager.OnProfileLoaded -= ProfileManager_OnProfileLoaded;
            BotBehaviorQueue.UnWire();
        }

        private static void BotMain_OnStart(IBot bot)
        {
            Logger.Log("Bot is Starting");
            LastBotStart = DateTime.UtcNow;
            PositionCache.Cache = new HashSet<Vector3>();
            ReloadProfileTag.LastReloadLoopQuestStep = "";
            ReloadProfileTag.QuestStepReloadLoops = 0;

            UseOnceTag.UseOnceIDs.Clear();
            UseOnceTag.UseOnceCounter.Clear();

            Death.DeathCount = 0;
            Death.LastDeathTime = DateTime.MinValue;
        }

        private static void GameEvents_OnGameChanged(object sender, EventArgs e)
        {
            UseOnceTag.UseOnceIDs.Clear();
            UseOnceTag.UseOnceCounter.Clear();
            ActorHistory.Clear();
        }

        private static void GameEvents_OnGameJoined(object sender, EventArgs e)
        {
            LastJoinedGame = DateTime.UtcNow;
            Logger.Debug("LastJoinedGame is {0}", LastJoinedGame);
            GameCount++;
            Death.DeathCount = 0;
            Death.LastDeathTime = DateTime.MinValue;
            LoadOnceTag.UsedProfiles.Clear();
            ActorHistory.Clear();
        }

        private static void GameEvents_OnPlayerDied(object sender, EventArgs e)
        {
            Death.DeathCount++;
            Death.LastDeathTime = DateTime.UtcNow;

            Logger.Log("Player died! Position={0} QuestId={1} StepId={2} WorldId={3}",
                ZetaDia.Me.Position, ZetaDia.CurrentQuest.QuestSnoId, ZetaDia.CurrentQuest.StepId, ZetaDia.Globals.WorldSnoId);

            if (Death.MaxDeathsAllowed <= 0)
                return;

            if (Death.DeathCount < Death.MaxDeathsAllowed)
                return;

            Logger.Log("You have died too many times. Now restarting the game.");
            ProfileManager.Load(ProfileManager.CurrentProfile.Path);
            ZetaDia.Service.Party.LeaveGame(true);

            // This is bad, we shouldn't do this :(
            Thread.Sleep(12000);
        }

        private static void GameEvents_OnWorldChanged(object sender, EventArgs e)
        {
            PositionCache.Cache = new HashSet<Vector3>();
            ActorHistory.Clear();
        }

        private static void ProfileManager_OnProfileLoaded(object sender, EventArgs e)
        {
            ProfileUtils.LoadAdditionalGameParams();
            ProfileHistory.Add(ProfileManager.CurrentProfile);
            ProfileUtils.ProcessProfile();
        }
    }

}
