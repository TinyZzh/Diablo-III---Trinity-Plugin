﻿using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Framework;
using Trinity.Reference;
using Trinity.Technicals;
using Zeta.Common;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Components.Combat.Abilities
{
    public static class SpellHistory
    {
        private const int SpellHistorySize = 300;
        private static List<SpellHistoryItem> _history = new List<SpellHistoryItem>(SpellHistorySize * 2);

        private static DateTime _lastSpenderCast = DateTime.MinValue;
        public static double TimeSinceSpenderCast 
        {
            get { return DateTime.UtcNow.Subtract(_lastSpenderCast).TotalMilliseconds; }
        }

        private static DateTime _lastGeneratorCast = DateTime.MinValue;
        public static double TimeSinceGeneratorCast
        {
            get { return DateTime.UtcNow.Subtract(_lastGeneratorCast).TotalMilliseconds; }
        }

        internal static List<SpellHistoryItem> History
        {
            get { return _history; }
            set { _history = value; }
        }

        public static void RecordSpell(TrinityPower power)
        {
            if (_history.Count >= SpellHistorySize)
                _history.RemoveAt(0);

            var skill = SkillUtils.ById(power.SNOPower);
            
            if(skill.IsAttackSpender)
                _lastSpenderCast = DateTime.UtcNow;

            if (skill.IsGeneratorOrPrimary)
                _lastGeneratorCast = DateTime.UtcNow;

            _history.Add(new SpellHistoryItem
            {
                Power = power,
                UseTime = DateTime.UtcNow,
                MyPosition = Core.Player.Position,
                TargetPosition = power.TargetPosition
            });

            CombatManager.TargetHandler.LastActionTimes.Add(DateTime.UtcNow);
            Trinity.TrinityPlugin.LastPowerUsed = power.SNOPower;

            LastSpellUseTime = DateTime.UtcNow;
            LastPowerUsed = power.SNOPower;

            Logger.LogVerbose(LogCategory.Targetting, "Recorded {0}", power);
            //CacheData.AbilityLastUsed[power.SNOPower] = DateTime.UtcNow;            
        }

        public static DateTime LastSpellUseTime { get; set; }

        public static SNOPower LastPowerUsed { get; set; }

        public static TrinityPower LastPower => History.OrderBy(s => s.TimeSinceUse).FirstOrDefault(s => s.Power.SNOPower != SNOPower.Walk)?.Power;

        public static void RecordSpell(SNOPower power)
        {
            RecordSpell(new TrinityPower(power));
        }


        public static DateTime PowerLastUsedTime(SNOPower power)
        {
            var useTime = DateTime.MinValue;
            if (History.Any())
            {
                var spellHistoryItem = GetLastUseHistoryItem(power);
                if (spellHistoryItem != null)
                {
                    useTime = spellHistoryItem.UseTime;
                }
                return useTime;
            }
            return useTime;
        }

        public static SpellHistoryItem GetLastUseHistoryItem(SNOPower power)
        {
            return _history.OrderByDescending(i => i.UseTime).FirstOrDefault(o => o.Power.SNOPower == power);
        }

        public static TimeSpan TimeSinceUse(SNOPower power)
        {
            var lastUsed = PowerLastUsedTime(power);
            if(lastUsed != DateTime.MinValue)
                return DateTime.UtcNow.Subtract(lastUsed);
            return TimeSpan.MaxValue;
        }

        internal static double MillisecondsSinceUse(SNOPower power)
        {
            var lastUsed = PowerLastUsedTime(power);
            if (lastUsed != DateTime.MinValue)
                return DateTime.UtcNow.Subtract(lastUsed).TotalMilliseconds;
            return -1;
        }

        public static int SpellUseCountInTime(SNOPower power, TimeSpan time)
        {
            if (_history.Any(i => i.Power.SNOPower == power))
            {
                var spellCount = _history.Count(i => i.Power.SNOPower == power && i.TimeSinceUse <= time);
                Logger.LogVerbose(LogCategory.Targetting, "Found {0}/{1} spells in {2} time for {3} power", spellCount, _history.Count(i => i.Power.SNOPower == power), time, power);
                return spellCount;
            }
            return 0;
        }

        public static bool HasUsedSpell(SNOPower power)
        {
            if (_history.Any() && _history.Any(i => i.Power.SNOPower == power))
                return true;
            return false;
        }

        public static Vector3 GetSpellLastTargetPosition(SNOPower power)
        {
            Vector3 lastUsed = Vector3.Zero;
            if (_history.Any(i => i.Power.SNOPower == power))
                lastUsed = _history.FirstOrDefault(i => i.Power.SNOPower == power).TargetPosition;
            return lastUsed;
        }

        public static Vector3 GetSpellLastMyPosition(SNOPower power)
        {
            Vector3 lastUsed = Vector3.Zero;
            if (_history.Any(i => i.Power.SNOPower == power))
                lastUsed = _history.FirstOrDefault(i => i.Power.SNOPower == power).MyPosition;
            return lastUsed;
        }

        public static float DistanceFromLastTarget(SNOPower power)
        {
            var lastUsed = GetSpellLastTargetPosition(power);
            return Core.Player.Position.Distance(lastUsed);
        }

        public static float DistanceFromLastUsePosition(SNOPower power)
        {
            var lastUsed = GetSpellLastMyPosition(power);
            return Core.Player.Position.Distance(lastUsed);
        }

    }
}
