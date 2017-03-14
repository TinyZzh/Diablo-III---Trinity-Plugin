using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Framework;
using Trinity.Framework.Helpers;
using Trinity.Framework.Objects;
using Trinity.Reference;
using Zeta.Bot;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Framework.Helpers.Logger;

namespace Trinity.Modules
{
    public class BuildLogger : Module
    {
        private bool _hasLoggedCurrentBuild;

        protected override int UpdateIntervalMs => 1000;

        protected override void OnPulse()
        {
            if (!_hasLoggedCurrentBuild && BotMain.IsRunning && Core.Inventory.PlayerEquippedIds.Any())
            {
                DebugUtil.LogBuildAndItems();
                _hasLoggedCurrentBuild = true;
            }
        }

        public static void LogBuildAndItems(TrinityLogLevel level = TrinityLogLevel.Info)
        {
            try
            {
                Action<Item, TrinityLogLevel> logItem = (i, l) =>
                {
                    Logger.Log(l, LogCategory.UserInformation, string.Format("Item: {0}: {1} ({2}) is Equipped",
                        i.ItemType, i.Name, i.Id));
                };

                Action<ACDItem, TrinityLogLevel> logACDItem = (i, l) =>
                {
                    Logger.Log(l, LogCategory.UserInformation, string.Format("Item: {0}: {1} ({2}) is Equipped",
                        i.ItemType, i.Name, i.ActorSnoId));
                };

                if (ZetaDia.Me == null || !ZetaDia.Me.IsValid)
                {
                    Logger.Log("Error: Not in game");
                    return;
                }

                var equipped = InventoryManager.Equipped;
                if (!equipped.Any())
                {
                    Logger.Log("Error: No equipped items detected");
                    return;
                }

                LogNewItems();

                var equippedItems = Legendary.Equipped.Where(c => (!c.IsSetItem || !c.Set.IsEquipped) && !c.IsEquippedInCube).ToList();
                Logger.Log(level, LogCategory.UserInformation, "------ Equipped Non-Set Legendaries: Items={0}, Sets={1} ------", equippedItems.Count, Sets.Equipped.Count);
                equippedItems.ForEach(i => logItem(i, level));

                var cubeItems = Legendary.Equipped.Where(c => c.IsEquippedInCube).ToList();
                Logger.Log(level, LogCategory.UserInformation, "------ Equipped in Kanai's Cube: Items={0} ------", cubeItems.Count, Sets.Equipped.Count);
                cubeItems.ForEach(i => logItem(i, level));

                Sets.Equipped.ForEach(s =>
                {
                    Logger.Log(level, LogCategory.UserInformation, "------ Set: {0} {1}: {2}/{3} Equipped. ActiveBonuses={4}/{5} ------",
                        s.Name,
                        s.IsClassRestricted ? "(" + s.ClassRestriction + ")" : string.Empty,
                        s.EquippedItems.Count,
                        s.Items.Count,
                        s.CurrentBonuses,
                        s.MaxBonuses);

                    s.Items.Where(i => i.IsEquipped).ForEach(i => logItem(i, level));
                });

                Logger.Log(level, LogCategory.UserInformation, "------ Active Skills / Runes ------", SkillUtils.Active.Count, SkillUtils.Active.Count);

                Action<Skill> logSkill = s =>
                {
                    Logger.Log(level, LogCategory.UserInformation, "Skill: {0} Rune={1} Type={2}",
                        s.Name,
                        s.CurrentRune.Name,
                        (s.IsAttackSpender) ? "Spender" : (s.IsGeneratorOrPrimary) ? "Generator" : "Other"
                        );
                };

                SkillUtils.Active.ForEach(logSkill);

                Logger.Log(level, LogCategory.UserInformation, "------ Passives ------", SkillUtils.Active.Count, SkillUtils.Active.Count);

                Action<Passive> logPassive = p => Logger.Log(level, LogCategory.UserInformation, "Passive: {0}", p.Name);

                PassiveUtils.Active.ForEach(logPassive);
            }
            catch (Exception ex)
            {
                Logger.Log("Exception in DebugUtil > LogBuildAndItems: {0} {1} {2}", ex.Message, ex.InnerException, ex);
            }
        }

        internal static void LogNewItems()
        {
            //var knownIds = Legendary.ItemIds;

            //using (new AquireFrameHelper())
            //{
            //    if (ZetaDia.Me == null || !ZetaDia.Me.IsValid)
            //    {
            //        Logger.Log("Not in game");
            //        return;
            //    }

            //    var allItems = new List<ACDItem>();
            //    allItems.AddRange(InventoryManager.StashItems);
            //    allItems.AddRange(InventoryManager.Equipped);
            //    allItems.AddRange(InventoryManager.Backpack);

            //    if (!allItems.Any())
            //        return;

            //    var newItems = allItems.Where(i => i != null && i.IsValid && i.ItemQualityLevel == ItemQuality.Legendary && (i.ItemBaseType == ItemBaseType.Jewelry || i.ItemBaseType == ItemBaseType.Armor || i.ItemBaseType == ItemBaseType.Weapon) && !knownIds.Contains(i.ActorSnoId)).DistinctBy(p => p.ActorSnoId).OrderBy(i => i.ItemType).ToList();

            //    if (!newItems.Any())
            //        return;

            //    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "------ New/Unknown Items {0} ------", newItems.Count);

            //    newItems.ForEach(i =>
            //    {
            //        Logger.Log(string.Format("Item: {0}: {1} ({2})", i.ItemType, i.Name, i.ActorSnoId));
            //    });
            //}
        }

    }
}
