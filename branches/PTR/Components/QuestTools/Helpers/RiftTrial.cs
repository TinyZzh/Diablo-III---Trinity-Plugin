using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using QuestTools.ProfileTags;
using QuestTools.ProfileTags.Complex;
using QuestTools.ProfileTags.Movement;
using Zeta.Bot;
using Zeta.Bot.Profile;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.TreeSharp;
using Action = Zeta.TreeSharp.Action;

namespace QuestTools.Helpers
{
    public static class RiftTrial
    {
        public static int GetCurrentWave()
        {
            if (!ZetaDia.IsInGame || ZetaDia.WorldInfo.SNOId != 405684)
                return 0;

            if (Quest == null || Quest.QuestStep != 1)
                return 0;

            return Quest.BonusCount + 1;
        }

        public static QuestInfo Quest
        {
            get;
            private set;
        }

        private static void UpdateQuest()
        {
            Quest = ZetaDia.ActInfo.ActiveQuests.FirstOrDefault(q => q.QuestSNO == 405695);
        }

        static readonly List<ProfileBehavior> EndTrialSequence = new List<ProfileBehavior>
        {
            new SafeMoveToTag()
            {
                PathPrecision = 5,
                PathPointLimit = 250,
                X = 393,
                Y = 237,
                Z = -11
            },
            new TownPortalTag(),
            new CompositeTag()
            {
                IsDoneWhen = ret => ConditionParser.IsActiveQuestAndStep(405695,9),
                Composite = new Action(ret =>
                {
                    Logger.Log("Waiting for Trial to Finish...");
                    return RunStatus.Success;
                })
            }
        };

        static readonly List<ProfileBehavior> StartTrialSequence = new List<ProfileBehavior>
        {
            new SafeMoveToTag()
            {
                PathPrecision = 5,
                PathPointLimit = 250,
                X = 311,
                Y = 323,
                Z = -11
            },
            new CompositeTag()
            {
                IsDoneWhen = ret => Zeta.Bot.ConditionParser.IsActiveQuestAndStep(405695,1) || ZetaDia.IsInTown,
                Composite = new Action(ret =>
                {
                    Logger.Log("Waiting for Trial to Start...");
                    return RunStatus.Success;
                })
            }
         };

        /*
        [1840ED0C] [Greater Rift Trial] QuestSnoId: 405695, QuestMeter: 0.8175, QuestState: InProgress, QuestStep: 1, KillCount: 0, BonusCount: 0
        [Step] IntroTimer, Id: 13
        [Objective] Type: TimedEventExpired
        [Step] MonsterWaves, Id: 1
        [Objective] TieredRiftChallengeEnd, Type: EventReceived
        [Step] TalkToNPC, Id: 9
        [Objective] Type: HadConversation
        */

        public static void PulseRiftTrial()
        {
            if (!ZetaDia.IsInGame)
            {
                Quest = null;
                return;
            }

            if (!QuestToolsSettings.Instance.EnableTrialRiftMaxLevel)
                return;

            UpdateQuest();
            if (Quest == null)
                return;

            if (Quest.QuestStep == 9 && IsAborting)
            {
                SetIsCombatAllowed(true);
                IsAborting = false;
                return;
            }

            if (ZetaDia.WorldInfo.SNOId != 405684)
                return;

            var maxWave = QuestToolsSettings.Instance.TrialRiftMaxLevel;
            var currentWave = GetCurrentWave();

            if (currentWave <= 1 && Quest.QuestStep == 13)
            {
                BotBehaviorQueue.Queue(StartTrialSequence, "Trial Start Sequence");
            }

            if (currentWave > 0)
            {
                Logger.Debug("Trial In Progress: Waves Complete = {0}", Quest.BonusCount);
            }

            if (currentWave >= maxWave && !IsAborting)
            {
                Logger.Log("Reached Max Wave {0}", currentWave);
                SetIsCombatAllowed(false);
                BotBehaviorQueue.Queue(EndTrialSequence, "Trial Abort Sequence");
                IsAborting = true;
            }
        }

        private static void SetIsCombatAllowed(bool isAllowed)
        {
            TrinityApi.SetProperty("Trinity.Combat.Abilities.CombatBase", "IsCombatAllowed", isAllowed);
        }


        public static bool IsAborting { get; set; }
    }
}
