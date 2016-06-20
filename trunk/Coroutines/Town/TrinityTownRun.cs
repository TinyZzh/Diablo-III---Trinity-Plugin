﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Buddy.Coroutines;
using Trinity.Combat.Abilities;
using Trinity.DbProvider;
using Trinity.Framework;
using Trinity.Framework.Actors;
using Trinity.Framework.Utilities;
using Trinity.Helpers;
using Trinity.Items;
using Trinity.Notifications;
using Trinity.Technicals;
using TrinityCoroutines;
using TrinityCoroutines.Resources;
using Zeta.Bot;
using Zeta.Bot.Coroutines;
using Zeta.Bot.Logic;
using Zeta.Bot.Navigation;
using Zeta.Bot.Settings;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using ActorManager = Trinity.Framework.Actors.ActorManager;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Coroutines.Town
{
    public class TrinityTownRun
    {
        public static bool StartedOutOfTown { get; set; }

        public static bool IsWantingTownRun { get; set; }

        public static bool IsInTownVendoring { get; set; }

        public static DateTime DontAttemptTownRunUntil = DateTime.MinValue;

        private static int _catastrophicErrorCount;

        static TrinityTownRun()
        {
            GameEvents.OnGameJoined += (sender, args) => _catastrophicErrorCount = 0;
        }

        public static async Task<bool> Execute()
        {
            try
            {
                //Logger.Log("VendorRun Hook Executing");

                if (await ClearArea.Execute())
                {
                    Logger.LogDebug("Clearing");
                    return false;  
                }

                CheckForDBVendoringBug();

                if (DateTime.UtcNow < DontAttemptTownRunUntil)
                {
                    Logger.LogDebug("Town run cooldown");
                    IsVendoring = false;
                    return false;
                }

                if (Core.CastStatus.StoneOfRecall.LastResult == CastResult.Casting)
                {
                    Logger.LogDebug("Casting");
                    return true;
                }

                if (!ShouldStartTownRun())
                {
                    //Logger.LogDebug("Don't town run");
                    IsVendoring = false;
                    return false;
                }

                IsWantingTownRun = true;
                Logger.LogDebug("Town run started");
                //IsVendoring = true;

                if (ZetaDia.IsLoadingWorld)
                {
                    return true;
                }

                if (!ZetaDia.IsInTown)
                {
                    if(ZetaDia.Me.IsInCombat && !ClearArea.IsClearing && ZetaDia.Actors.GetActorsOfType<DiaUnit>().Any(u => u?.CommonData != null && u.CommonData.IsValid && u.IsAlive && u.IsHostile && u.Distance < 16f))
                    {
                        ClearArea.Start();
                    }

                    Logger.LogDebug("Pre Go to town");

                    await GoToTown();

                    if (!ZetaDia.IsInTown)
                    {
                        if (Core.CastStatus.StoneOfRecall.LastResult == CastResult.Failed)
                        {
                            Logger.LogDebug("Setting Town Run Cooldown because of cast failure");
                            DontAttemptTownRunUntil = DateTime.UtcNow.AddSeconds(5);
                        }

                        Logger.LogDebug("Not in town yet..");
                        return true;
                    }
                }

                Logger.LogVerbose($"Starting Townrun");


                IsInTownVendoring = true;

                while (DateTime.UtcNow.Subtract(TrinityPlugin.LastWorldChangeTime).TotalMilliseconds < 2000 || ZetaDia.IsLoadingWorld || ZetaDia.CurrentWorldSnoId <= 0)
                {
                    await Coroutine.Sleep(2000);
                }

                await Coroutine.Wait(8000, () => ActorManager.Items.Any());
                await Coroutine.Sleep(1000);

                var checkCycles = 2;
                while (!TrinityPlugin.Player.IsInventoryLockedForGreaterRift)
                {
                    if (!ActorManager.Items.Any())
                    {
                        Logger.LogError("Something went terribly wrong, no items found");
                        _catastrophicErrorCount++;
                        ActorManager.Reset();

                        if (_catastrophicErrorCount > 2)
                        {
                            Logger.LogError("Unable to recover from error state, still cant read items properly");
                            ZetaDia.Service.Party.LeaveGame(true);
                            return false;
                        }
                        continue;
                    }
                    else
                    {
                        ActorManager.Items.ForEach(i => Logger.LogDebug($"Backpack Item: {i.Name} ({i.ActorSnoId} / {i.InternalName}) RawItemType={i.RawItemType} TrinityItemType={i.TrinityItemType}"));
                    }

                    await Coroutine.Yield();

                    GameUI.CloseVendorWindow();

                    await IdentifyItems.Execute();

                    if (!ZetaDia.IsInGame)
                    {
                        StartedOutOfTown = false;
                        IsWantingTownRun = false;
                        IsInTownVendoring = false;
                        IsVendoring = false;
                        return false;
                    }

                    if (!await ExtractLegendaryPowers.Execute())
                        continue;

                    if (TrinityItemManager.IsAnyTwoSlotBackpackLocation)
                    {
                        if (!await Gamble.Execute())
                            continue;
                    }

                    if (!await CubeRaresToLegendary.Execute())
                        continue;

                    if (!await CubeItemsToMaterials.Execute())
                        continue;

                    if (await Any(
                        DropItems.Execute,
                        () => StashItems.Execute(true),
                        SellItems.Execute,
                        SalvageItems.Execute))
                        continue;

                    checkCycles--;
                    if (checkCycles == 0)
                    {
                        break;
                    }
                }

                await StashItems.Execute();                
                //await StashItems.SortStashPages();
                await UseCraftingRecipes.Execute();
                await RepairItems.Execute();

                Logger.Log("Finished Town Run woo!");
                DontAttemptTownRunUntil = DateTime.UtcNow + TimeSpan.FromSeconds(15);
                Helpers.Notifications.SendEmailNotification();
                Helpers.Notifications.SendMobileNotifications();
                //TownRun.LastTownRunFinishTime = DateTime.UtcNow;

                if (StartedOutOfTown)
                {
                    StartedOutOfTown = false;
                    await TakeReturnPortal();
                }
                IsWantingTownRun = false;
            }
            finally
            {
                IsInTownVendoring = false;
                IsVendoring = false;
            }
            return false;
        }



        public static async Task<bool> Any(params Func<Task<bool>>[] taskProducers)
        {
            var result = false;
            foreach (var task in taskProducers)
            {
                if (await task())
                    result = true;
            }
            return result;
        }

        private static bool ShouldStartTownRun()
        {
            if (ZetaDia.IsInTown && BrainBehavior.IsVendoring)
            {
                return !IsInTownVendoring;
            }

            if (!CanTownRun())
            {
                //Logger.LogVerbose("Can't town run.");
                return false;
            }

            if (IsWantingTownRun)
            {
                Logger.LogDebug("Is wanting to town run.");
                return true;
            }

            // Close Normal rift before doing a town run.
            if (ZetaDia.IsInTown && !StartedOutOfTown && ZetaDia.CurrentRift != null && ZetaDia.CurrentRift.IsCompleted && ZetaDia.CurrentRift.IsStarted)
            {
                var orek = TownInfo.Orek?.GetActor() as DiaUnit;
                if (orek != null && orek.IsQuestGiver)
                {
                    return false;
                }
            }

            var validLocation = TrinityItemManager.FindValidBackpackLocation(true);
            if (validLocation.X < 0 || validLocation.Y < 0)
            {
                Logger.Log("No more space to pickup a 2-slot item, now running town-run routine. (TownRun)");
                return true;
            }

            var needRepair = RepairItems.EquipmentNeedsRepair();
            if (needRepair)
            {
                Logger.LogDebug("Townrun for repair.");
                return true;
            }

            return BrainBehavior.IsVendoring;
        }

        public static bool CanTownRun()
        {
            string cantUseTPreason;
            if (!ZetaDia.Me.CanUseTownPortal(out cantUseTPreason) && !ZetaDia.IsInTown)
            {
                Logger.LogVerbose("Can't townrun because '{0}'", cantUseTPreason);
                DontAttemptTownRunUntil = DateTime.UtcNow + TimeSpan.FromSeconds(10);
                return false;
            }

            if (TrinityPlugin.Player.IsDead)
                return false;

            if (!RepairItems.EquipmentNeedsRepair())
            {
                if (TrinityPlugin.Player.IsInventoryLockedForGreaterRift)
                {
                    Logger.LogVerbose("Can't townrun while in greater rift!");
                    DontAttemptTownRunUntil = DateTime.UtcNow + TimeSpan.FromSeconds(5);
                    return false;
                }

                // Close Greater rift before doing a town run.
                if (!TrinityPlugin.Settings.Loot.TownRun.KeepLegendaryUnid && TrinityPlugin.Player.ParticipatingInTieredLootRun)
                {
                    return false;
                }
            }

            if (ErrorDialog.IsVisible)
            {
                Logger.Log("Can't townrun with an error dialog present!");
                DontAttemptTownRunUntil = DateTime.UtcNow + TimeSpan.FromSeconds(10);
                return false;
            }

            if (TrinityPlugin.Player.LevelAreaId == 19947 && ZetaDia.CurrentQuest.QuestSnoId == 87700 && new Vector3(2959.893f, 2806.495f, 24.04533f).Distance(ZetaDia.Me.Position) > 180f)
            {
                Logger.Log("Can't townrun with the current quest (A1 New Game) !");
                DontAttemptTownRunUntil = DateTime.UtcNow + TimeSpan.FromSeconds(30);
                return false;
            }

            if (DataDictionary.BossLevelAreaIDs.Contains(TrinityPlugin.Player.LevelAreaId))
            {
                Logger.Log("Unable to Town Portal - Boss Area!");
                DontAttemptTownRunUntil = DateTime.UtcNow + TimeSpan.FromSeconds(10);
                return false;
            }

            if (DataDictionary.NeverTownPortalLevelAreaIds.Contains(TrinityPlugin.Player.LevelAreaId))
            {
                Logger.Log("Unable to Town Portal in this area!");
                DontAttemptTownRunUntil = DateTime.UtcNow + TimeSpan.FromSeconds(10);
                return false;
            }

            return true;
        }

        private static readonly Lazy<PropertyInfo> VendorProperty = new Lazy<PropertyInfo>(() => typeof(BrainBehavior).GetProperty("IsVendoring"));

        public static bool IsVendoring
        {
            get { return BrainBehavior.IsVendoring; }
            set { VendorProperty.Value.SetValue(null, value); }
        }

        public static async Task<bool> GoToTown()
        {
            if (ZetaDia.IsInTown || !ZetaDia.Me.IsFullyValid() || !UIElements.BackgroundScreenPCButtonRecall.IsEnabled)
            {
                Logger.Log("Not portaling because in combat.");
                return false;
            }

            if (CombatBase.IsInCombat)
            {
                Logger.Log("Not portaling because in combat.");
                return false;
            }

            Navigator.PlayerMover.MoveStop();
            await Coroutine.Wait(2000, () => !ZetaDia.Me.Movement.IsMoving);
            
            Logger.Warn("Starting Town Run");
            StartedOutOfTown = true;

            if (!ZetaDia.IsInTown && !ZetaDia.IsLoadingWorld)
            {
                ZetaDia.Me.UseTownPortal();
            }

            await Coroutine.Sleep(500);
            await Coroutine.Wait(5000, () => !Core.CastStatus.StoneOfRecall.IsCasting);
            //await CommonBehaviors.CreateUseTownPortal().ExecuteCoroutine();
            //await CommonCoroutines.UseTownPortal("TrinityPlugin can haz town now plz?");
            return true;
        }

        private static bool lastTownPortalCheckResult;
        private static DateTime lastTownPortalCheckTime = DateTime.MinValue;

        public static bool IsTryingToTownPortal()
        {
            if (DateTime.UtcNow.Subtract(lastTownPortalCheckTime).TotalMilliseconds < 100)
                return lastTownPortalCheckResult;

            if (!ZetaDia.Me.CanUseTownPortal() || DataDictionary.NeverTownPortalLevelAreaIds.Contains(ZetaDia.CurrentLevelAreaSnoId))
            {
                lastTownPortalCheckTime = DateTime.UtcNow;
                lastTownPortalCheckResult = false;
                return false;
            }

            if (IsWantingTownRun)
            {
                lastTownPortalCheckTime = DateTime.UtcNow;
                lastTownPortalCheckResult = true;
                return true;
            }

            if (ClearArea.IsClearing)
            {
                lastTownPortalCheckTime = DateTime.UtcNow;
                lastTownPortalCheckResult = true;
                return true;
            }

            if (BrainBehavior.IsVendoring)
            {
                lastTownPortalCheckTime = DateTime.UtcNow;
                lastTownPortalCheckResult = true;
                return true;
            }

            if (TrinityPlugin.Player.IsCastingPortal)
            {
                lastTownPortalCheckTime = DateTime.UtcNow;
                lastTownPortalCheckResult = true;
                return true;
            }

            lastTownPortalCheckTime = DateTime.UtcNow;
            lastTownPortalCheckResult = false;
            return TrinityPlugin.WantToTownRun;
        }

        private async static Task<bool> TakeReturnPortal()
        {
            if (!ZetaDia.IsInTown)
                return false;

            var portal = TownInfo.ReturnPortal;
            if (portal == null)
            {
                Logger.Log("Couldn't find a return portal");
                return false;
            }

            Logger.Log("Found a hearth portal, lets use it.");

            if (!await MoveToAndInteract.Execute(portal, 2f, 10))
            {
                Logger.Log("Failed to move to return portal :(");
                return false;
            }

            var actor = portal.GetActor();
            if (actor != null)
            {
                if (!actor.Interact())
                {
                    Logger.Log("Failed to interact with return portal.");
                }
            }

            await Coroutine.Sleep(1000);

            if (ZetaDia.IsInTown && !ZetaDia.IsLoadingWorld)
            {
                Logger.Log("Trying again to use return portal.");
                var gizmo = ZetaDia.Actors.GetActorsOfType<DiaGizmo>().FirstOrDefault(g => g.ActorInfo.GizmoType == GizmoType.HearthPortal);
                if (gizmo != null)
                {
                    await CommonCoroutines.MoveAndStop(gizmo.Position, 2f, "Portal Position");
                    await Coroutine.Sleep(1000);
                    gizmo.Interact();
                    gizmo.Interact();
                }
            }

            await Coroutine.Wait(5000, () => !ZetaDia.IsLoadingWorld);

            return true;
        }

        #region Remove when DB bug is fixed

        private static DateTime _brainVendoringStarted;
        private static bool _isVendoring;
        private static void CheckForDBVendoringBug()
        {
            // An exception in DB core during town run will cause IsVendoring to never be set to false.

            var isVendoring = BrainBehavior.IsVendoring;
            if (isVendoring != _isVendoring)
            {
                if (isVendoring)
                {
                    _brainVendoringStarted = DateTime.UtcNow;
                    return;
                }
            }

            if (isVendoring && DateTime.UtcNow.Subtract(_brainVendoringStarted).TotalSeconds > 200)
            {
                IsVendoring = false;
            }
        }

        #endregion
    }
}

