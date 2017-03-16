﻿//using Buddy.Coroutines;
//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using Trinity.Coroutines.Resources;
//using Trinity.Framework;
//using Trinity.Framework.Objects.Enums;
//using Trinity.Reference;
//using Zeta.Game;
//using Logger = Trinity.Framework.Helpers.Logger;

//namespace Trinity.Coroutines.Town
//{
//    public class UseCraftingRecipes
//    {
//        public static DateTime LastStartTime = DateTime.MinValue;
//        public static int TimeoutSeconds = 45;
//        public static DateTime CooldownExpires;

//        public static async Task<bool> Execute()
//        {
//            if (DateTime.UtcNow < CooldownExpires)
//            {
//                Logger.LogVerbose("[UseCraftingRecipes] On cooldown after behavior timeout");
//                return true;
//            }

//            LastStartTime = DateTime.UtcNow;

//            while (true)
//            {
//                await Coroutine.Yield();

//                if (!ZetaDia.IsInTown)
//                    break;

//                // No Plans
//                if (!Inventory.OfType(InventoryItemType.BlackSmithPlan, InventoryItemType.JewelerPlan).Any())
//                {
//                    Logger.LogVerbose("[UseCraftingRecipes] No Jeweler or Blacksmith Plans");
//                    break;
//                }

//                // Timeout
//                if (DateTime.UtcNow.Subtract(LastStartTime).TotalSeconds > TimeoutSeconds)
//                {
//                    Logger.LogVerbose("[UseCraftingRecipes] {0} Second Timeout", TimeoutSeconds);
//                    CooldownExpires = DateTime.UtcNow.AddSeconds(60);
//                    break;
//                }

//                // Use all the blacksmith plans
//                while (GameUI.IsBlackSmithWindowOpen && Inventory.Backpack.OfType(InventoryItemType.BlackSmithPlan).Any())
//                {
//                    Logger.LogVerbose("[UseCraftingRecipes] Using Blacksmith Plans");

//                    InventoryManager.UseItem(Inventory.Backpack.OfType(InventoryItemType.BlackSmithPlan).First().AnnId);
//                    await Coroutine.Sleep(25);
//                }

//                // Move to Blacksmith.
//                if (!GameUI.IsBlackSmithWindowOpen && Inventory.Backpack.OfType(InventoryItemType.BlackSmithPlan).Any())
//                {
//                    Logger.LogVerbose("[UseCraftingRecipes] Moving to Blacksmith");

//                    if (!await MoveToAndInteract.Execute(TownInfo.BlacksmithAny))
//                    {
//                        Logger.LogVerbose("[UseCraftingRecipes] Failed to move to Blacksmith");
//                        return true;
//                    }

//                    continue;
//                }

//                // Use all the Jeweller plans
//                while (GameUI.IsJewelerWindowOpen && Core.Inventory.Backpack.Any(i => i.RawItemType == RawItemType.CraftingPlan_Jeweler))
//                {
//                    Logger.LogVerbose("[UseCraftingRecipes] Using Jeweler Plans");
//                    InventoryManager.UseItem(Core.Inventory.Backpack.First(i => i.RawItemType == RawItemType.CraftingPlan_Jeweler).AnnId);
//                    await Coroutine.Sleep(25);
//                }

//                // Move to Jeweller.
//                if (!GameUI.IsJewelerWindowOpen && Core.Inventory.Backpack.Any(i => i.RawItemType == RawItemType.CraftingPlan_Jeweler))
//                {
//                    Logger.LogVerbose("[UseCraftingRecipes] Moving to Jeweler");

//                    if (!await MoveToAndInteract.Execute(TownInfo.Jeweler))
//                    {
//                        Logger.LogVerbose("[UseCraftingRecipes] Failed to move to Jeweler");
//                        return true;
//                    }

//                    continue;
//                }

//                // Move to Stash
//                if (Inventory.Stash.OfType(InventoryItemType.BlackSmithPlan, InventoryItemType.JewelerPlan).Any())
//                {
//                    Logger.LogVerbose("[UseCraftingRecipes] Getting Plans from Stash");

//                    var plans = Inventory.OfType(InventoryItemType.BlackSmithPlan, InventoryItemType.JewelerPlan);
//                    var amount = Math.Min(InventoryManager.NumFreeBackpackSlots, plans.Count);
//                    var planIds = plans.Select(i => i.ActorSnoId).Distinct();
//                    if (!await TakeItemsFromStash.Execute(planIds, amount))
//                    {
//                        Logger.LogVerbose("[UseCraftingRecipes] Failed to get items from Stash");
//                        return true;
//                    }
//                }
//            }

//            Logger.Log("[UseCraftingRecipes] Finished!");
//            return true;
//        }
//    }
//}