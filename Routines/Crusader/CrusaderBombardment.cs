﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using Trinity.Components.Combat;
using Trinity.Components.Combat.Resources;
using Trinity.Coroutines.Town;
using Trinity.Framework;
using Trinity.Framework.Actors.ActorTypes;
using Trinity.Framework.Helpers;
using Trinity.Framework.Objects;
using Trinity.Reference;
using Trinity.Settings;
using Trinity.UI;
using Zeta.Bot.Navigation;
using Zeta.Common;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Framework.Helpers.Logger;

namespace Trinity.Routines.Crusader
{
    public sealed class CrusaderBombardment : CrusaderBase, IRoutine
    {
        #region Definition

        public string DisplayName => "Crusader LoN Bombardment";
        public string Description => " A special routine that casts spells at particular rotation times.";
        public string Author => "phelon";
        public string Version => "0.1";
        public string Url => "http://www.icy-veins.com/d3/crusader-endgame-bombardment-build-with-the-legacy-of-nightmares-set-patch-2-4-2-season-7";

        public Build BuildRequirements => new Build
        {
            Sets = new Dictionary<Set, SetBonus>
            {
                { Sets.LegacyOfNightmares, SetBonus.First },
            },
            Skills = new Dictionary<Skill, Rune>
            {
                { Skills.Crusader.Bombardment, null },
            },
        };

        #endregion

        public override bool SetWeight(TrinityActor cacheObject)
        {
            if (Settings.IgnoreTrash && cacheObject.IsTrashMob && RiftProgression.IsInRift && !cacheObject.IsTreasureGoblin && !cacheObject.IsMinimapActive && (RiftProgression.IsGreaterRift || !TrinityTownRun.IsTryingToTownPortal()))
            {
                cacheObject.WeightInfo += $"Routine(IgnoreTrash)";
                cacheObject.Weight = 0;
                return true;
            }
            return false;
        }

        public TrinityPower GetOffensivePower()
        {
            TrinityPower power;
            TrinityActor target;

            // Credit: Phelon's LoN Bombardment routine.

            if (AllowedToUse(Settings.Akarats, Skills.Crusader.AkaratsChampion) && ShouldAkaratsChampion())
                return AkaratsChampion();

            if (ShouldCondemn())
                return Condemn();

            if (ShouldProvoke())
                return Provoke();

            if (ShouldJudgement())
                return Judgement();

            if (TryBombard(out power))
                return power;

            if (ShouldSteedCharge())
                return SteedCharge();

            if (!IsCurrentlyAvoiding)
            {
                //Logger.Log("Steed Charge Damage");
                return TargetUtil.BestAoeUnit(45, true).Distance < 15
                    ? new TrinityPower(SNOPower.Walk, 7f,
                        TargetUtil.GetZigZagTarget(TargetUtil.BestAoeUnit(45, true).Position, 15f, false)
                        , -1, 0, 1)
                    : new TrinityPower(SNOPower.Walk, 3f, TargetUtil.BestAoeUnit(45, true).Position);
            }

            return null;
        }

        private bool TryBombard(out TrinityPower trinityPower)
        {
            trinityPower = null;

            // Make sure we cast Bombardment when IronSkin and CoE is Up.
            // Note iron skin is gated below by Convention of Elements check,            
            if (Player.HasBuff(SNOPower.X1_Crusader_IronSkin))
            {
                TrinityActor target;

                if (ShouldBombardment(out target))
                {
                    trinityPower = Bombardment(target);
                    return true;
                }

                if (ShouldShieldGlare(out target))
                {
                    trinityPower = ShieldGlare(target);
                    return true;
                }

                if (IsInCombat && ShouldConsecration())
                {
                    trinityPower = Consecration();
                    return true;
                }
            }

            // Wait for CoE to Cast Damage CD's
            var isCastWindow = !ShouldWaitForConventionofElements(Skills.Crusader.Bombardment, Element.Physical, 1500, 1000);
            var eliteExists = HostileMonsters.Any(u => u.IsElite && !Combat.Weighting.ShouldIgnore(u));
            var isTargetCloseEnough = !eliteExists || CurrentTarget != null && CurrentTarget.IsElite && CurrentTarget.Distance < 20f;

            if (Skills.Crusader.Bombardment.CanCast() && (ShouldBombardWheneverPossible && isTargetCloseEnough || isCastWindow))
            {
                if (ShouldIronSkin())
                {
                    trinityPower = IronSkin();
                    return true;
                }
            }

            return false;
        }

        private bool ShouldBombardWheneverPossible
        {
            get
            {
                if (Settings.Bombardment.WaitForConvention == ConventionMode.GreaterRift && RiftProgression.IsGreaterRift)
                    return false;

                if (Settings.Bombardment.WaitForConvention == ConventionMode.Always)
                    return false;

                if (Settings.Bombardment.WaitForConvention == ConventionMode.RiftBoss && RiftProgression.IsInRift && HostileMonsters.Any(u => u.IsBoss))
                    return false;

                return true;
            }
        }

        protected override bool ShouldBombardment(out TrinityActor target)
        {
            target = null;

            if (!Skills.Crusader.Bombardment.CanCast())
                return false;

            target = TargetUtil.GetBestClusterUnit() ?? HostileMonsters.OrderBy(u => u.Distance).FirstOrDefault();
            return target != null;
        }

        protected override bool ShouldJudgement()
        {
            if (!Skills.Crusader.Judgment.CanCast())
                return false;

            return TargetUtil.GetBestClusterUnit()?.Distance < 10f;
        }

        protected override bool ShouldSteedCharge()
        {
            // Steed disables all skills for a second so make sure it doesn't prevent bombardment.
            if (TimeToElementStart(Element.Physical) < 3000)
                return false;

            return base.ShouldSteedCharge();
        }

        protected override bool ShouldIronSkin()
        {
            if (!Skills.Crusader.IronSkin.CanCast())
                return false;

            if (Player.HasBuff(SNOPower.X1_Crusader_IronSkin))
                return false;

            if (!HostileMonsters.Any(u => u.Distance < 80f))
                return false;

            return true;
        }


        public TrinityPower GetBuffPower()
        {
            TrinityPower power;

            if (TryLaw(out power))
                return power;

            if (!Player.IsInTown && Settings.BombardmentOOC && HostileMonsters.Any(u => u.Distance < 150f))
            {
                // Break Steed to bombard OOC only if waiting for CoE
                var goodTimetoCast = !ShouldBombardWheneverPossible || !IsSteedCharging;
                if (goodTimetoCast && TryBombard(out power))
                    return power;
            }

            return null;
        }

        protected override bool ShouldLawsOfHope()
        {
            // Spam for movement buff.
            return Skills.Crusader.LawsOfHope.CanCast();
        }

        public TrinityPower GetDefensivePower() => GetBuffPower();
        public TrinityPower GetDestructiblePower() => DefaultDestructiblePower();

        public TrinityPower GetMovementPower(Vector3 destination)
        {
            if (ShouldSteedCharge())
                return SteedCharge();

            return Walk(destination);
        }

        #region Settings

        public override int ClusterSize => Settings.ClusterSize;
        public override float EmergencyHealthPct => Settings.EmergencyHealthPct;

        IDynamicSetting IRoutine.RoutineSettings => Settings;
        public CrusaderBombardmentSettings Settings { get; } = new CrusaderBombardmentSettings();

        public sealed class CrusaderBombardmentSettings : NotifyBase, IDynamicSetting
        {
            private SkillSettings _akarats;
            private SkillSettings _bombardment;
            private int _clusterSize;
            private float _emergencyHealthPct;
            private bool _ignoreTrash;
            private bool _bombardmentOOC;

            [DefaultValue(false)]
            public bool IgnoreTrash
            {
                get { return _ignoreTrash; }
                set { SetField(ref _ignoreTrash, value); }
            }

            [DefaultValue(25)]
            public int ClusterSize
            {
                get { return _clusterSize; }
                set { SetField(ref _clusterSize, value); }
            }

            [DefaultValue(0.4f)]
            public float EmergencyHealthPct
            {
                get { return _emergencyHealthPct; }
                set { SetField(ref _emergencyHealthPct, value); }
            }

            public SkillSettings Akarats
            {
                get { return _akarats; }
                set { SetField(ref _akarats, value); }
            }

            public SkillSettings Bombardment
            {
                get { return _bombardment; }
                set { SetField(ref _bombardment, value); }
            }


            [DefaultValue(false)]
            public bool BombardmentOOC
            {
                get { return _bombardmentOOC; }
                set { SetField(ref _bombardmentOOC, value); }
            }


            #region Skill Defaults

            private static readonly SkillSettings AkaratsDefaults = new SkillSettings
            {
                UseMode = UseTime.Always,
            };

            private static readonly SkillSettings BombardmentDefaults = new SkillSettings
            {
                WaitForConvention = ConventionMode.Always,              
            };

            #endregion

            public override void LoadDefaults()
            {
                base.LoadDefaults();
                Akarats = AkaratsDefaults.Clone();
                Bombardment = BombardmentDefaults.Clone();
            }

            #region IDynamicSetting

            public string GetName() => GetType().Name;
            public UserControl GetControl() => UILoader.LoadXamlByFileName<UserControl>(GetName() + ".xaml");
            public object GetDataContext() => this;
            public string GetCode() => JsonSerializer.Serialize(this);
            public void ApplyCode(string code) => JsonSerializer.Deserialize(code, this, true);
            public void Reset() => LoadDefaults();
            public void Save() { }

            #endregion
        }

        #endregion
    }
}

