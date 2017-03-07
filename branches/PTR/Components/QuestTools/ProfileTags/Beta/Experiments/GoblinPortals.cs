using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuestTools.Navigation;
using QuestTools.ProfileTags;
using QuestTools.ProfileTags.Complex;
using QuestTools.ProfileTags.Movement;
using Zeta.Bot;
using Zeta.Bot.Profile;
using Zeta.Bot.Profile.Common;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using Zeta.TreeSharp;

namespace QuestTools.Helpers
{
    class GoblinPortals
    {
        private static DateTime LastPulse = DateTime.MinValue;

        private static List<int> CompletedPortalLocations = new List<int>();

        public static void Clear()
        {
            CompletedPortalLocations.Clear();
        }

        public static void Pulse()
        {

            //if (!QuestToolsSettings.Instance.EnableBetaFeatures)
            //    return;

            //if (DateTime.UtcNow.Subtract(LastPulse).TotalMilliseconds < 3000)
            //    return;

            //LastPulse = DateTime.UtcNow;

            //var whymsyportal = ActorHistory.GetActor(405590);

            //if (whymsyportal != null && whymsyportal.IsNearby && !CompletedPortalLocations.Contains(whymsyportal.LevelAreaSnoIdId))
            //{
            //    // Entrance
            //    // <!-- Quest: X1_Bounty_A2_Oasis_Event_LostTreasureKhanDakab (346067) World: caOUT_Town (70885) LevelArea: A2_caOut_Oasis (57425) -->
            //    // <MoveToActor questId="346067" x="3472" y="4295" z="100"  actorId="405590" interactRange="9" name="p1_Portal_Tentacle_goblin-19946"  isPortal="True" destinationWorldId="-1" pathPrecision="5" pathPointLimit="250" statusText="" /> 

            //    CompletedPortalLocations.Add(whymsyportal.LevelAreaSnoIdId);

            //    ActorHistory.CachedActor portal = null;

            //    BotBehaviorQueue.Queue(new List<ProfileBehavior>
            //    {
            //        new MoveToActor { ActorId = 405590, IsPortal = true, DestinationWorldId = -1 },
            //        new ExploreDungeonTag
            //        {
            //            EndType = ExploreDungeonTag.ExploreEndType.FullyExplored,
            //            ActorId = 208659,
            //            QuestId = 1,
            //            BoxSize = 40,
            //            PathPrecision = 60,
            //            BoxTolerance = 0.1f
            //        },                              
            //        new CompositeTag
            //        {
            //            BehaviorDelegate = new PrioritySelector(
            //                new MoveToActor
            //                {
            //                    ActorId = 208659, 
            //                    IsPortal = true, 
            //                    DestinationWorldId = -1

            //                }.Run(),
            //                new TownPortalTag().Run()
            //            )                        
            //        }
            //    });

            //    // Exit
            //    //<!-- Quest: x1_OpenWorld_quest (312429) World: p1_a1Dun_Random_Level_Goblin (409093) LevelArea: p1_a1Dun_Random_Level_Goblin (409094) -->
            //    //<MoveToActor questId="312429" stepId="2" actorId="208659" interactRange="9" name="g_Portal_Tentacle-19904"  isPortal="True" destinationWorldId="-1" pathPrecision="5" pathPointLimit="250" statusText="" /> 
            //}

            //if (ConditionParser.Evaluate("ActorExistsNearMe(393030,100)"))
            //{
            //    BotBehaviorQueue.Queue(new List<ProfileBehavior>
            //    {
            //        new MoveToActor { ActorId = 393030, IsPortal = true, DestinationWorldId = -1 },
            //        new ExploreDungeonTag
            //        {
            //            EndType = ExploreDungeonTag.ExploreEndType.ExitFound,
            //            QuestId = 1,
            //            BoxSize = 40,
            //            PathPrecision = 60,
            //            BoxTolerance = 0.1f
            //        }
            //    });
            //}
        }
    }
}
