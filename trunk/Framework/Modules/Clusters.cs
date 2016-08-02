﻿using System.Collections.Generic;
using System.Linq;
using Trinity.Components.Combat;
using Trinity.Framework.Actors.ActorTypes;
using Zeta.Common;
using Zeta.Game.Internals.Actors;

namespace Trinity.Framework.Modules
{
    public class Clusters : Module
    {
        protected override int UpdateIntervalMs => 500;

        public TargetArea Nearby { get; private set; } = new TargetArea(80f);
        public TargetArea CloseNearby { get; private set; } = new TargetArea(16f);
        public TargetCluster BestCluster { get; private set; } = new TargetCluster(20f);
        public TargetCluster BestLargeCluster { get; private set; } = new TargetCluster(24f, 8);
        public TargetCluster BestRiftValueCluster { get; private set; } = new TargetCluster(50f);

        protected override void OnPulse()
        {
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
                Position = Core.Player.Position;

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

        public bool Exists => TargetUtil.ClusterExists(Radius, Size);

        public new void Update()
        {            
            Position = Exists ?
                TargetUtil.GetClusterPoint(Radius, Size) :
                TargetUtil.GetBestClusterPoint(Radius);
                TargetUtil.GetBestRiftValueClusterPoint(Radius, Core.Settings.Combat.Misc.RiftValueAlwaysKillUnitsAbove);

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