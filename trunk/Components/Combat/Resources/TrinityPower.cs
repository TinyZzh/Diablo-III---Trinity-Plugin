﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buddy.Coroutines;
using Trinity.Components.Combat.Abilities;
using Trinity.DbProvider;
using Trinity.Framework;
using Trinity.Framework.Actors.ActorTypes;
using Trinity.Framework.Objects.Enums;
using Trinity.Helpers;
using Trinity.Objects;
using Trinity.Reference;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Navigation;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Components.Combat
{
    /// <summary>
    /// 'TrinityPower' holds instructions on how a spell should be cast in a combat situation.
    /// Compared to the 'Skill' object which houses utilities and constant information.
    /// </summary>
    public class TrinityPower : IEquatable<TrinityPower>
    {

        public SNOPower SNOPower { get; set; }

        public float MinimumRange { get; set; }

        public Vector3 TargetPosition { get; set; }

        public int TargetDynamicWorldId { get; set; }

        public int TargetAcdId { get; set; }

        public float WaitBeforeUseMs { get; set; } = 0;

        public float WaitAfterUseMs { get; set; } = 0;

        public DateTime PowerAssignmentTime { get; set; }

        public bool IsCastOnSelf { get; set; }

        public TimeSpan TimeSinceAssigned => DateTime.UtcNow.Subtract(PowerAssignmentTime);

        public double TimeSinceUseMs => SpellHistory.TimeSinceUse(SNOPower).TotalMilliseconds;

        public bool ShouldWaitBeforeUse => TimeSinceAssigned.TotalMilliseconds < WaitBeforeUseMs;

        public bool ShouldWaitAfterUse => TimeSinceUseMs < WaitAfterUseMs;

        public double WaitTimeBeforeRemaining => Math.Max(0, WaitBeforeUseMs - TimeSinceAssigned.TotalMilliseconds);

        public double WaitTimeAfterRemaining => Math.Max(0, WaitAfterUseMs - TimeSinceUseMs);

        public bool IsInteractPower => DataDictionary.InteractPowers.Contains(SNOPower);

        public TrinityPower()
        {
            PowerAssignmentTime = DateTime.UtcNow;

            // default values
            SNOPower = SNOPower.None;
            MinimumRange = 0f;
            TargetPosition = Vector3.Zero;
            TargetDynamicWorldId = ZetaDia.WorldId;
            TargetAcdId = -1;
            IsCastOnSelf = false;
        }

        public TrinityPower(SNOPower snoPower)
        {
            IsCastOnSelf = true;
            SNOPower = snoPower;
            MinimumRange = 0f;
            TargetPosition = Vector3.Zero;
            TargetDynamicWorldId = ZetaDia.WorldId;
            TargetAcdId = -1;
            PowerAssignmentTime = DateTime.UtcNow;
        }

        public TrinityPower(Skill skill)
            : this(skill.SNOPower) { }

        public TrinityPower(SNOPower snoPower, int waitBeforeUseMs, int waitAfterUseMs)
        {
            IsCastOnSelf = true;
            SNOPower = snoPower;
            MinimumRange = 0f;
            TargetPosition = Vector3.Zero;
            TargetDynamicWorldId = ZetaDia.WorldId;
            TargetAcdId = -1;
            WaitBeforeUseMs = waitBeforeUseMs;
            WaitAfterUseMs = waitAfterUseMs;
            PowerAssignmentTime = DateTime.UtcNow;
        }

        public TrinityPower(Skill skill, int waitBeforeUseMs, int waitAfterUseMs)
            : this(skill.SNOPower, waitBeforeUseMs, waitAfterUseMs) { }

        public TrinityPower(SNOPower snoPower, float minimumRange, int targetAcdId)
        {
            IsCastOnSelf = false;
            SNOPower = snoPower;
            MinimumRange = minimumRange;
            TargetPosition = Vector3.Zero;
            TargetDynamicWorldId = ZetaDia.WorldId;
            TargetAcdId = targetAcdId;
            PowerAssignmentTime = DateTime.UtcNow;
        }

        public TrinityPower(Skill skill, float minimumRange, int targetAcdId)
            : this(skill.SNOPower, minimumRange, targetAcdId) { }

        public TrinityPower(SNOPower snoPower, float minimumRange, int targetAcdId, int waitBeforeUseMs, int waitAfterUseMs)
        {
            IsCastOnSelf = false;
            SNOPower = snoPower;
            MinimumRange = minimumRange;
            TargetPosition = Vector3.Zero;
            TargetDynamicWorldId = ZetaDia.WorldId;
            TargetAcdId = targetAcdId;
            WaitBeforeUseMs = waitBeforeUseMs;
            WaitAfterUseMs = waitAfterUseMs;
            PowerAssignmentTime = DateTime.UtcNow;
        }

        public TrinityPower(Skill skill, float minimumRange, int targetAcdId, int waitBeforeUseMs, int waitAfterUseMs)
            : this (skill.SNOPower, minimumRange, targetAcdId, waitBeforeUseMs, waitAfterUseMs) { }

        public TrinityPower(SNOPower snoPower, float minimumRange)
        {
            IsCastOnSelf = true;
            SNOPower = snoPower;
            MinimumRange = minimumRange;
            TargetPosition = Vector3.Zero;
            TargetDynamicWorldId = ZetaDia.WorldId;
            TargetAcdId = -1;
            PowerAssignmentTime = DateTime.UtcNow;
        }

        public TrinityPower(Skill skill, float minimumRange)
            : this(skill.SNOPower, minimumRange) { }

        public TrinityPower(SNOPower snoPower, float minimumRange, Vector3 position)
        {
            IsCastOnSelf = false;
            SNOPower = snoPower;
            MinimumRange = minimumRange;
            TargetPosition = position;
            TargetDynamicWorldId = ZetaDia.WorldId;
            TargetAcdId = -1;
            PowerAssignmentTime = DateTime.UtcNow;
        }

        public TrinityPower(Skill skill, float minimumRange, Vector3 position)
            : this(skill.SNOPower, minimumRange, position)  { }

        public TrinityPower(SNOPower snoPower, float minimumRange, Vector3 position, float waitBeforeUseMs, float waitAfterUseMs)
        {
            IsCastOnSelf = false;
            SNOPower = snoPower;
            MinimumRange = minimumRange;
            TargetPosition = position;
            TargetDynamicWorldId = ZetaDia.WorldId;
            TargetAcdId = -1;
            WaitBeforeUseMs = waitBeforeUseMs;
            WaitAfterUseMs = waitAfterUseMs;
            PowerAssignmentTime = DateTime.UtcNow;
        }

        public TrinityPower(Skill skill, float minimumRange, Vector3 position, float waitBeforeUseMs, float waitAfterUseMs)
            : this(skill.SNOPower, minimumRange, position, waitBeforeUseMs, waitAfterUseMs) { }

        public TrinityPower(SNOPower snoPower, float minimumRange, Vector3 position, int targetAcdId, float waitBeforeUseMs, float waitAfterUseMs)
        {
            IsCastOnSelf = false;
            SNOPower = snoPower;
            MinimumRange = minimumRange;
            TargetPosition = position;
            TargetDynamicWorldId = ZetaDia.WorldId;
            TargetAcdId = targetAcdId;
            WaitBeforeUseMs = waitBeforeUseMs;
            WaitAfterUseMs = waitAfterUseMs;
            PowerAssignmentTime = DateTime.UtcNow;
        }

        public TrinityPower(Skill skill, float minimumRange, Vector3 position, int targetAcdId, float waitBeforeUseMs, float waitAfterUseMs)
            : this(skill.SNOPower, minimumRange, position, targetAcdId, waitBeforeUseMs, waitAfterUseMs) { }

        public TrinityPower(SNOPower snoPower, float minimumRange, TrinityActor actor)
        {
            IsCastOnSelf = false;
            SNOPower = snoPower;
            MinimumRange = minimumRange;
            TargetPosition = actor.Position;
            TargetDynamicWorldId = ZetaDia.WorldId;
            TargetAcdId = actor.AcdId;
            PowerAssignmentTime = DateTime.UtcNow;
        }

        public TrinityPower(Skill skill, float minimumRange, TrinityActor actor)
            : this(skill.SNOPower, minimumRange, actor) { }

        public bool Equals(TrinityPower other)
        {
            return SNOPower == other.SNOPower &&
                TargetPosition == other.TargetPosition &&
                TargetAcdId == other.TargetAcdId &&
                WaitBeforeUseMs == other.WaitBeforeUseMs &&
                WaitAfterUseMs == other.WaitAfterUseMs &&
                TargetDynamicWorldId == other.TargetDynamicWorldId &&
                MinimumRange == other.MinimumRange;
        }

        public string RangedCastInfo => $"Range={MinimumRange:N1} {(TargetPosition == Vector3.Zero ? "" : $"<{TargetPosition.X:N2},{TargetPosition.Y:N2},{TargetPosition.Z:N2}>")} AcdId={(TargetAcdId != -1 ? $"AcdId={TargetAcdId}" : string.Empty)}";
        public override string ToString() => $"{SNOPower} {WaitBeforeUseMs}/{WaitAfterUseMs} LastUse={SpellHistory.TimeSinceUse(SNOPower).ToString("mm':'ss':'fff")} {(IsCastOnSelf ? "" : RangedCastInfo)} ";

        private readonly HashSet<SNOPower> _waitForAttackToFinishPowers = new HashSet<SNOPower>
        {
            //SNOPower.Monk_WayOfTheHundredFists,
            //SNOPower.Monk_CycloneStrike,
            //SNOPower.X1_Crusader_Bombardment,
            //SNOPower.Wizard_Archon_ArcaneStrike_Lightning,
            //SNOPower.Wizard_Archon_ArcaneStrike_Cold,
            //SNOPower.Wizard_Archon_ArcaneStrike_Fire,
            //SNOPower.Wizard_Archon_ArcaneStrike,
            //SNOPower.Wizard_Archon_ArcaneBlast_Lightning,
            //SNOPower.Wizard_Archon_ArcaneBlast_Fire,
            //SNOPower.Wizard_Archon_ArcaneBlast_Cold,
            //SNOPower.Wizard_Archon_ArcaneBlast,
            //SNOPower.Wizard_Archon_DisintegrationWave,
            //SNOPower.Wizard_Archon_DisintegrationWave_Cold,
            //SNOPower.Wizard_Archon_DisintegrationWave_Fire,
            //SNOPower.Wizard_Archon_DisintegrationWave_Lightning,
            //SNOPower.Wizard_ArcaneTorrent,
            //SNOPower.Wizard_Archon_Teleport,
        };



        public bool ShouldWaitForAttackToFinish => _waitForAttackToFinishPowers.Contains(this.SNOPower);

        public Skill GetSkill() => SkillUtils.GetSkillByPower(SNOPower);

    }
}
