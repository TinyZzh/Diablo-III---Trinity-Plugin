﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using Trinity.Configuration;
using Trinity.Framework.Actors.ActorTypes;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity.Combat
{
    public static class Enemies
    {
        public static List<TrinityActor> Alive = new List<TrinityActor>();
        public static List<TrinityActor> Dead = new List<TrinityActor>();
        public static HashSet<int> DeadGuids = new HashSet<int>();
        public static HashSet<int> AliveGuids = new HashSet<int>();
        public static TargetArea Nearby = new TargetArea(80f);
        public static TargetArea CloseNearby = new TargetArea(16f);
        public static TargetCluster BestCluster = new TargetCluster(20f);
        public static TargetCluster BestLargeCluster = new TargetCluster(24f, 8);
        public static TargetCluster BestRiftValueCluster = new TargetCluster(50f);

        public static void Update()
        {

            if (!ZetaDia.IsInGame || !ZetaDia.Me.IsValid)
                return;

            List<TrinityActor> units = TrinityPlugin.Targets.Where(o => o.IsUnit && o.IsValid || o.IsElite).ToList();
            var unitsGuids = new HashSet<int>(units.Select(e => e.AcdId));

            // Find Newly Dead Units
            List<TrinityActor> newlyDead = Alive.Where(a => !unitsGuids.Contains(a.AcdId) && !DeadGuids.Contains(a.AcdId)).ToList();
            newlyDead.ForEach(u => Events.OnUnitAliveHandler.Invoke(u));
            Dead.AddRange(newlyDead);
            Dead.RemoveAll(e => DateTime.UtcNow.Subtract(e.LastSeenTime).TotalSeconds > 60);
            DeadGuids = new HashSet<int>(Dead.Select(e => e.AcdId));

            // Find Newly Alive Units
            var newlyAliveGuids = new HashSet<int>(units.Where(a => !AliveGuids.Contains(a.AcdId)).Select(a => a.AcdId));
            Alive = units;
            AliveGuids = unitsGuids;
            Alive.Where(u => newlyAliveGuids.Contains(u.AcdId)).ForEach(u => Events.OnUnitDeathHandler.Invoke(u));

            Nearby.Update();
            CloseNearby.Update();
            BestCluster.Update();
            BestLargeCluster.Update();
            BestRiftValueCluster.Update();
        }
    }

    public class TargetArea
    {
        public TargetArea (float range = 20f, Vector3 position = new Vector3())
        {
            if (position == Vector3.Zero)
                NearMe = true;

            Units = new List<TrinityActor>();
            UnitsAcdId = new HashSet<int>();
            Position = position;
            Range = range;
            Update();
        }

        public Vector3 Position { get; set; }
        public float Range { get; set; }
        public int EliteCount { get; set; }
        public int BossCount { get; set; }
        public int UnitCount { get; set; }
        public bool NearMe { get; set; }
        public List<TrinityActor> Units { get; set; }
        public HashSet<int> UnitsAcdId { get; set; }

        public double AverageHealthPct
        {
            get { return Units.Any() ? Units.Average(u => u.HitPointsPct) : 0; }
        }

        public void Update()
        {
            if (NearMe)
                Position = TrinityPlugin.Player.Position;

            if (Position == Vector3.Zero)
                return;

            Units = TargetUtil.ListUnitsInRangeOfPosition(Position, Range);
            UnitsAcdId = new HashSet<int>(Units.Select(u => u.AcdId));
            EliteCount = TargetUtil.NumElitesInRangeOfPosition(Position, Range);
            UnitCount = TargetUtil.NumMobsInRangeOfPosition(Position, Range);
            BossCount = TargetUtil.NumBossInRangeOfPosition(Position, Range);
        }

        public int TotalDebuffCount(SNOPower power)
        {
            return Units.Any() ? TargetUtil.DebuffCount(new List<SNOPower> { power }, Units) : 0;
        }

        public int TotalDebuffCount (IEnumerable<SNOPower> powers)
        {
            return Units.Any() ? TargetUtil.DebuffCount(powers, Units) : 0;
        }

        public int DebuffedCount (IEnumerable<SNOPower> powers)
        {
            return Units.Any() ? TargetUtil.MobsWithDebuff(powers, Units) : 0;
        }

        public float DebuffedPercent (IEnumerable<SNOPower> powers)
        {
            return Units.Any() ? DebuffedCount(powers)/Units.Count : 0;
        }
    }

    public class TargetCluster : TargetArea
    {
        public TargetCluster (float radiusOfCluster = 20f, int minUnitsInCluster = 1)
        {
            Radius = radiusOfCluster > 5 ? radiusOfCluster : 5;
            Size = minUnitsInCluster > 1 ? minUnitsInCluster : 1;
            Update();
        }

        public float Radius { get; set; }
        public int Size { get; set; }
        public TargetArea TargetArea { get; set; }

        public bool Exists
        {
            get { return TargetUtil.ClusterExists(Radius, Size); }
        }

        public new void Update()
        {
            Position = Exists ?
                TargetUtil.GetClusterPoint(Radius, Size) :
                TargetUtil.GetBestClusterPoint(Radius);
                TargetUtil.GetBestRiftValueClusterPoint(Radius, TrinityPlugin.Settings.Combat.Misc.RiftValueAlwaysKillUnitsAbove);

            NearMe = false;
            Range = Radius;
            base.Update();

            TargetArea = new TargetArea(Radius, Position);
        }

        internal TrinityActor GetTargetWithoutDebuffs (IEnumerable<SNOPower> debuffs)
        {
            return TargetUtil.BestTargetWithoutDebuffs(Range, debuffs, Position);
        }
    }
}