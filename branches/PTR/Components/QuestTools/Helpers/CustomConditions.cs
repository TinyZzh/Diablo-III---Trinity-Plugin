using System;
using System.Collections.Generic;
using QuestTools.Helpers;
using QuestTools.ProfileTags;
using System.Linq;
using Zeta.Bot.Settings;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using ConditionParser = Zeta.Bot.ConditionParser;

namespace QuestTools
{

    public static class CustomConditions
    {
        public static void Initialize()
        {
            ScriptManager.RegisterShortcutsDefinitions((typeof(CustomConditions)));
        }

        public static bool IsCastingOrLoading()
        {
            return

                ZetaDia.Me != null &&
                ZetaDia.Me.IsValid &&
                ZetaDia.Me.CommonData != null &&
                ZetaDia.Me.CommonData.IsValid &&
                !ZetaDia.Me.IsDead &&
                (
                    ZetaDia.Globals.IsLoadingWorld ||
                    ZetaDia.Me.CommonData.AnimationState == AnimationState.Casting ||
                    ZetaDia.Me.CommonData.AnimationState == AnimationState.Channeling ||
                    ZetaDia.Me.CommonData.AnimationState == AnimationState.Transform ||
                    ZetaDia.Me.CommonData.AnimationState.ToString() == "13"
                );
        }

        public static bool CurrentWave(int waveNumber)
        {
            return RiftTrial.GetCurrentWave() == waveNumber;
        }

        public static bool CurrentSceneName(string sceneName)
        {
            return ZetaDia.Me.CurrentScene.Name.ToLowerInvariant().Contains(sceneName.ToLowerInvariant());
        }

        public static bool CurrentDifficulty(string difficulty)
        {
            GameDifficulty d;
            return Enum.TryParse(difficulty, true, out d) && CharacterSettings.Instance.GameDifficulty == d;
        }

        public static bool CurrentDifficultyLessThan(string difficulty)
        {
            GameDifficulty d;
            if (Enum.TryParse(difficulty, true, out d))
            {
                var currentIndex = (int)CharacterSettings.Instance.GameDifficulty;
                var testIndex = (int)d;

                return currentIndex < testIndex;
            }
            return false;
        }

        public static bool CurrentDifficultyGreaterThan(string difficulty)
        {
            GameDifficulty d;
            if (Enum.TryParse(difficulty, true, out d))
            {
                var currentIndex = (int)CharacterSettings.Instance.GameDifficulty;
                var testIndex = (int)d;

                return currentIndex > testIndex;
            }
            return false;
        }

        public static bool CurrentClass(string actorClass)
        {
            ActorClass a;
            return Enum.TryParse(actorClass, true, out a) && ZetaDia.Service.Hero.Class == a;
        }

        public static bool CurrentHeroLevel(int level)
        {
            return ZetaDia.Service.Hero.Level == level;
        }

        public static bool HighestKeyCountId(int id)
        {
            return !Keys.IsAllSameCount && Keys.HighestKeyId == id;
        }

        public static bool LowestKeyCountId(int id)
        {
            return !Keys.IsAllSameCount && Keys.LowestKeyId == id;
        }

        public static long ItemCount(int actorId)
        {
            var items = ZetaDia.Me.Inventory.StashItems.Where(item => actorId == item.ActorSnoId)
                .Concat(ZetaDia.Me.Inventory.Backpack.Where(item => actorId == item.ActorSnoId)).ToList();

            if (!items.Any())
                return 0;

            if (items.First().ItemStackQuantity > 0)
                return items.Select(i => i.ItemStackQuantity).Aggregate((a, b) => a + b);
            
            return items.Count;
        }

        public static long BackpackCount(int actorId)
        {
            var items = ZetaDia.Me.Inventory.Backpack.Where(item => actorId == item.ActorSnoId).ToList();

            if (!items.Any())
                return 0;

            if (items.First().ItemStackQuantity > 0)
                return items.Select(i => i.ItemStackQuantity).Aggregate((a, b) => a + b);

            return items.Count;
        }

        public static long StashCount(int actorId)
        {
            var items = ZetaDia.Me.Inventory.StashItems.Where(item => actorId == item.ActorSnoId).ToList();

            if (!items.Any())
                return 0;

            if (items.First().ItemStackQuantity > 0)
                return items.Select(i => i.ItemStackQuantity).Aggregate((a, b) => a + b);

            return items.Count;
        }

        public static bool ItemCountGreaterThan(int actorId, int amount)
        {
            return ItemCount(actorId) > amount;
        }

        public static bool ItemCountLessThan(int actorId, int amount)
        {
            return ItemCount(actorId) < amount;
        }

        public static string ProfileSetting(string key)
        {
            string value;
            return ProfileSettingTag.ProfileSettings.TryGetValue(key, out value) ? value : string.Empty;
        }

        public static int Counter(string key)
        {
            int value;
            return IncrementCounterTag.Counters.TryGetValue(key, out value) ? value : 0;
        }

        public static bool ActorIsAlive(int actorId)
        {
            var actor = ZetaDia.Actors.GetActorsOfType<DiaUnit>().FirstOrDefault(a => a.IsValid && a.ActorSnoId == actorId && a.IsAlive);

            // Its possible that by the time other tags run this actor will have disappeared from actors collection
            // to make sure we dont lose track of it, it needs to be recorded in the history.
            ActorHistory.UpdateActor(actor);

            return actor != null;
        }

        public static bool ActorFound(int actorId)
        {
            return ActorHistory.HasBeenSeen(actorId) || ActorIsAlive(actorId);
        }

        public static bool ActorExistsNearMe(int actorId, float range)
        {
            var nearbyActors = ZetaDia.Actors.GetActorsOfType<DiaObject>(true).Where(i => i.IsValid && i.ActorSnoId == actorId && Vector3.Distance(i.Position, ZetaDia.Me.Position) <= range).ToList();
            if (DataDictionary.GuardedBountyGizmoIds.Contains(actorId))
            {
                try
                {
                    nearbyActors = nearbyActors.Where(i => i is DiaGizmo && !((DiaGizmo)i).HasBeenOperated).ToList();
                }
                catch (Exception)
                {
                }
            }
            return nearbyActors.Count > 0;
        }

        public static bool KeyAboveMedian(int actorId)
        {
            return Keys.GetKeyCount(actorId) > Keys.Median;
        }

        public static bool KeyBelowMedian(int actorId)
        {
            return Keys.GetKeyCount(actorId) < Keys.Median;
        }

        public static bool KeyAboveUpperFence(int actorId)
        {
            return Keys.GetKeyCount(actorId) > Keys.UpperFence;
        }
        public static bool KeyBelowLowerFence(int actorId)
        {
            return Keys.GetKeyCount(actorId) < Keys.LowerFence;
        }

        public static bool KeyAboveUpperQuartile(int actorId)
        {
            return Keys.GetKeyCount(actorId) > Keys.UpperQuartile;
        }

        public static bool KeyBelowLowerQuartile(int actorId)
        {
            return Keys.GetKeyCount(actorId) < Keys.LowerQuartile;
        }

        public static bool IsKeyOutlier(int actorId)
        {
            return Keys.GetKeyIdNotWithinRange(1) == actorId;
        }

        public static bool IsAnyKeyOutlier()
        {
            return Keys.GetKeyIdNotWithinRange(1) != 0;
        }

        public static bool UsedOnce(string id)
        {
            return UseOnceTag.UseOnceIDs.Contains(id);
        }

        public static bool HasBeenOperated(int actorId)
        {
            var actor = ZetaDia.Actors.GetActorsOfType<DiaGizmo>(true).FirstOrDefault(a => a.ActorSnoId == actorId);
            return actor != null && actor.HasBeenOperated;
        }

        public static bool CurrentAnimation(int actorId, string animationName)
        {
            var actor = ZetaDia.Actors.GetActorsOfType<DiaGizmo>(true).FirstOrDefault(a => a.ActorSnoId == actorId);

            if (actor == null || actor.CommonData == null)
                return false;

            var result = actor.CommonData.CurrentAnimation.ToString() == animationName;

            Logger.Debug("Animation for {0} ({1}) is {2} State={3} ({4})",
                actor.Name,
                actor.ActorSnoId,
                actor.CommonData.CurrentAnimation,
                actor.CommonData.AnimationState,
                result);

            return result;
        }

        public static bool IsBountyLevelArea(int questId)
        {
            var result = ZetaDia.Storage.Quests.Bounties.FirstOrDefault(q => q.Quest == (SNOQuest)questId && q.LevelAreas.Contains((SNOLevelArea)ZetaDia.CurrentLevelAreaSnoId) || q.StartingLevelArea == (SNOLevelArea)ZetaDia.CurrentLevelAreaSnoId);

            return result != null;
        }

        public static bool IsVendorWindowOpen()
        {
            return UIElements.VendorWindow != null && UIElements.VendorWindow.IsValid && UIElements.VendorWindow.IsVisible;
        }

        public static bool QuestComplete(int questId)
        {
            return ZetaDia.Storage.Quests.AllQuests.Any(q => q.QuestSNO == questId && q.State == QuestState.Completed);
        }

        public static bool HasQuestOnStep(int questId, int stepId)
        {
            return ZetaDia.Storage.Quests.AllQuests.Any(q => q.QuestSNO == questId && q.QuestStep == stepId);
        }

        public static bool HasAnyUsableRiftKeysForQTOpenRiftWrapper()
        {
            var riftkeys = ZetaDia.Actors.ACDList.Where(i => i is ACDItem && i.IsFullyValid()).Cast<ACDItem>().Where(i =>
                        i.ItemType == ItemType.KeystoneFragment ||
                        i.InternalName.StartsWith("LootRunKey") ||
                        i.InternalName.StartsWith("TieredLootrunKey"))
                        .ToList();

            return riftkeys.Any(i => i.TieredLootRunKeyLevel < QuestToolsSettings.Instance.LimitRiftLevel);
        }
    }
}
