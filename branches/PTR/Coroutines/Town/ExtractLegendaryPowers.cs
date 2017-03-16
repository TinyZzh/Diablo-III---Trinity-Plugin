using Buddy.Coroutines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trinity.Components.Combat;
using Trinity.Coroutines.Resources;
using Trinity.Framework;
using Trinity.Framework.Actors.ActorTypes;
using Trinity.Framework.Helpers;
using Trinity.Framework.Objects;
using Trinity.Framework.Objects.Enums;
using Trinity.Reference;
using Trinity.Settings;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Framework.Helpers.Logger;

namespace Trinity.Coroutines.Town
{
    /// <summary>
    /// Convert rares into legendaries with Kanai's cube
    /// </summary>
    public class ExtractLegendaryPowers
    {
        public static bool HasUnlockedCube = true;
        private static DateTime _disabledUntil = DateTime.MinValue;
        private static readonly TimeSpan DisableDuration = TimeSpan.FromMinutes(1);


        public static HashSet<RawItemType> DoNotExtractRawItemTypes = new HashSet<RawItemType>
        {
            RawItemType.EnchantressSpecial,
            RawItemType.ScoundrelSpecial,
            RawItemType.FollowerSpecial,
            RawItemType.TemplarSpecial,
        };

        public static HashSet<int> DoNotExtractItemIds = new HashSet<int>()
        {
            Legendary.PigSticker.Id,
            Legendary.CorruptedAshbringer.Id
        };

        private static List<TrinityItem> _backpackCandidates;
        private static List<TrinityItem> _stashCandidates;
        private static List<int> _itemsTakenFromStashAnnId = new List<int>();
        private static HashSet<int> _blacklistedActorSnoIds = new HashSet<int>();

        public static bool HasCurrencyRequired 
            => Core.Inventory.Currency.HasCurrency(TransmuteRecipe.ExtractLegendaryPower);

        public static bool CanRun()
        {
            if (!ZetaDia.IsInGame || !ZetaDia.IsInTown || ZetaDia.Storage.CurrentWorldType != Act.OpenWorld)
                return false;

            if (Core.Settings.KanaisCube.ExtractLegendaryPowers == CubeExtractOption.None)
                return false;

            if (DateTime.UtcNow < _disabledUntil)
                return false;

            var kule = TownInfo.ZultonKule?.GetActor() as DiaUnit;
            if (kule != null)
            {
                if (kule.IsQuestGiver)
                {
                    Logger.LogVerbose("[ExtractLegendaryPowers] Cube is not unlocked yet");
                    _disabledUntil = DateTime.UtcNow.Add(DisableDuration);
                    HasUnlockedCube = false;
                    return false;
                }
                HasUnlockedCube = true;
            }

            if (!HasUnlockedCube)
                return false;

            if (!HasCurrencyRequired)
            {
                Logger.LogVerbose("[ExtractLegendaryPowers] Unable to find the required materials!");
                return false;
            }

            _backpackCandidates = GetLegendaryExtractionCandidates(InventorySlot.BackpackItems).DistinctBy(i => i.ActorSnoId).ToList();

            _stashCandidates = Core.Settings.KanaisCube.CubeExtractFromStash
                ? GetLegendaryExtractionCandidates(InventorySlot.SharedStash).DistinctBy(i => i.ActorSnoId).ToList()
                : new List<TrinityItem>();

            if (!_backpackCandidates.Any() && !_stashCandidates.Any())
            {
                Logger.LogVerbose("[ExtractLegendaryPowers] There are no items that need extraction!");
                _disabledUntil = DateTime.UtcNow.Add(DisableDuration);
                return false;
            }

            return true;
        }

        private static List<TrinityItem> GetLegendaryExtractionCandidates(InventorySlot slot)
        {
            var result = new List<TrinityItem>();

            if (Core.Settings.KanaisCube.ExtractLegendaryPowers == CubeExtractOption.None)
                return result;

            var source = Core.Inventory.Where(i => i.InventorySlot == slot);
            var alreadyCubedIds = new HashSet<int>(ZetaDia.Storage.PlayerDataManager.ActivePlayerData.KanaisPowersExtractedActorSnoIds);

            foreach (var item in source)
            {
                if (!item.IsValid)
                    continue;

                if (item.TrinityItemType == TrinityItemType.HealthPotion)
                    continue;

                if (item.FollowerType != FollowerType.None)
                    continue;

                if (DoNotExtractRawItemTypes.Contains(item.RawItemType))
                    continue;

                if (DoNotExtractItemIds.Contains(item.ActorSnoId))
                    continue;

                if (alreadyCubedIds.Contains(item.ActorSnoId))
                    continue;

                if (_blacklistedActorSnoIds.Contains(item.ActorSnoId))
                    continue;

                if (Core.Settings.KanaisCube.ExtractLegendaryPowers == CubeExtractOption.OnlyTrashed && Combat.Loot.ShouldStash(item))
                    continue;

                if (Core.Settings.KanaisCube.ExtractLegendaryPowers == CubeExtractOption.OnlyNonAncient && !item.IsAncient)
                    continue;

                if (string.IsNullOrEmpty(Legendary.GetItem(item)?.LegendaryAffix))
                    continue;

                result.Add(item);
            }

            if (result.Any())
            {
                Logger.Log("Found {0} Items to Extract Powers From in {1}:", result.Count, slot);

                foreach (var i in result)
                {
                    if (i == null) continue;
                    Logger.LogVerbose("{0} ({1}) - {2}", i.Name, i.ActorSnoId, Legendary.GetItem(i).LegendaryAffix);
                }
            }

            return result;
        }

        public static async Task<bool> Execute()
        {
            if (Core.Player.IsInventoryLockedForGreaterRift)
            {
                Logger.LogVerbose("Can't extract powers: inventory locked by greater rift");
                return false;
            }

            var result = await Main();

            // Make sure we put back anything we removed. Its possible for example that we ran out of materials
            // and the current backpack contents do no longer match the loot rules. Don't want them to be lost.

            if (_itemsTakenFromStashAnnId.Any())
            {
                await PutItemsInStash.Execute(_itemsTakenFromStashAnnId);
                _itemsTakenFromStashAnnId.Clear();
            }
            return result;
        }

        public static async Task<bool> ExtractAllBackpack()
        {
            while (true)
            {
                if (HasCurrencyRequired && _backpackCandidates.Any())
                {
                    if (!await MoveToCube())
                    {
                        Logger.LogVerbose("[ExtractLegendaryPowers] MoveToCube() failed");
                        return true;
                    }

                    if (!await ExtractPowers())
                    {
                        Logger.LogVerbose("[ExtractLegendaryPowers] ExtractPowers() failed");
                        return true;
                    }
                }
                else
                {
                    Logger.Log("[ExtractLegendaryPowers] Oh no! Out of materials!");
                    return true;
                }

                await Coroutine.Sleep(500);
                await Coroutine.Yield();
            }
        }

        public static async Task<bool> Main()
        {
            var started = false;
            while (CanRun())
            {
                if (!started)
                {
                    Logger.Log("[ExtractLegendaryPowers] Extraction is currently set to: {0}", Core.Settings.KanaisCube.ExtractLegendaryPowers);                    
                    Logger.Log("[ExtractLegendaryPowers] We begin the extractions.");
                    started = true;
                }

                if (HasCurrencyRequired && _backpackCandidates.Any())
                {
                    if (!await MoveToCube())
                    {
                        Logger.LogVerbose("[ExtractLegendaryPowers] MoveToCube() failed");
                        return false;
                    }

                    if (!await ExtractPowers())
                    {
                        Logger.LogVerbose("[ExtractLegendaryPowers] ExtractPowers() failed");
                        return false;
                    }
                }
                else if (_stashCandidates.Any() && Core.Settings.KanaisCube.CubeExtractFromStash)
                {
                    Logger.Log("[ExtractLegendaryPowers] Getting Legendaries from Stash");

                    if (!await TakeItemsFromStash.Execute(_stashCandidates))
                        return false;

                    _itemsTakenFromStashAnnId.AddRange(_stashCandidates.Select(i => i.AnnId));
                }
                else
                {
                    Logger.Log("[ExtractLegendaryPowers] Oh no! Out of materials!");
                    return false;
                }

                await Coroutine.Sleep(500);
                await Coroutine.Yield();
            }
            return true;
        }

        private static async Task<bool> ExtractPowers()
        {
            if (!_backpackCandidates.Any())
            {
                Logger.Log("[ExtractLegendaryPowers] Something went wrong in ExtractPowers(), very very wrong.");
                return false;
            }

            Logger.Log("[ExtractLegendaryPowers] Ready to go, Lets transmute!");

            var item = _backpackCandidates.First();
            var itemName = item.Name;
            var itemDynamicId = item.AnnId;
            var itemInternalName = item.InternalName;
            var itemSnoId = item.ActorSnoId;

            await Transmute.Execute(item, TransmuteRecipe.ExtractLegendaryPower);
            await Coroutine.Sleep(1500);

            var shouldBeDestroyedItem = InventoryManager.Backpack.FirstOrDefault(i => i.AnnId == itemDynamicId);
            if (shouldBeDestroyedItem == null && ZetaDia.Storage.PlayerDataManager.ActivePlayerData.KanaisPowersExtractedActorSnoIds.Contains(itemSnoId))
            {
                Logger.Log("[ExtractLegendaryPowers] Item Power Extracted! '{0}' ({1})",itemName, itemSnoId);
                Core.Inventory.InvalidAnnIds.Add(itemDynamicId);
                _itemsTakenFromStashAnnId.Remove(itemDynamicId);
            }
            else
            {
                Logger.Log("[ExtractLegendaryPowers] Failed to Extract Power! '{0}' {1} DynId={2} HasBackpackMaterials={3}", itemName, itemInternalName, itemDynamicId, HasCurrencyRequired);
                _blacklistedActorSnoIds.Add(itemSnoId);
                return false;
            }

            return true;
        }

        private static async Task<bool> MoveToCube()
        {
            if (GameUI.KanaisCubeWindow.IsVisible)
                return true;

            if (TownInfo.KanaisCube.Distance < 10f)
                return true;

            if (TownInfo.KanaisCube.Distance > 350f || TownInfo.KanaisCube.Position == Vector3.Zero)
            {
                Logger.Log("Cube location is too far away or invalid");
                return false;
            }

            if (!await MoveToAndInteract.Execute(TownInfo.KanaisCube))
            {
                Logger.Log("Failed to move to the cube, quite unfortunate.");
            }

            return true;
        }
    }
}