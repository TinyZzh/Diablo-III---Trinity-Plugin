﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Trinity.Framework;
using Trinity.Technicals;
using Zeta.Common;
using Zeta.Game.Internals.Actors;

namespace Trinity.Combat.Abilities.PhelonsPlayground
{
    class PhelonUtils
    {
        internal static List<TrinityCacheObject> SafeList(bool objectsInAoe = false)
        {
            return
                TrinityPlugin.ObjectCache.Where(x => objectsInAoe || !Core.Avoidance.InAvoidance(x.Position)).ToList();
        }
        
        internal static TrinityCacheObject GetFarthestClusterUnit(float aoe_radius = 25f, float maxRange = 65f, int count = 1, bool useWeights = true, bool includeUnitsInAoe = true)
        {
            using (new PerformanceLogger("TargetUtil.GetFarthestClusterUnit"))
            {
                return 
                    (from u in SafeList(includeUnitsInAoe)
                     where ((useWeights && u.Weight > 0) || !useWeights) &&
                     u.IsUnit &&
                     u.RadiusDistance <= maxRange &&
                     u.NearbyUnitsWithinDistance(aoe_radius) >= count
                     orderby u.NearbyUnitsWithinDistance(aoe_radius),
                     u.Distance descending
                     select u).FirstOrDefault();
            }
        }

        internal static TrinityCacheObject BestPierceOrClusterUnit(float clusterRadius = 15f, float maxSearchRange = 65f,
            bool includeInAoE = true)
        {
            var clusterUnit = GetBestClusterUnit(clusterRadius, maxSearchRange, !
                includeInAoE);
            var pierceUnit = GetBestPierceTarget(maxSearchRange, !includeInAoE);

            if (clusterUnit == null && pierceUnit == null)
                return null;

            if (clusterUnit == null)
                return pierceUnit;

            if (pierceUnit == null)
                return clusterUnit;

            return clusterUnit.NearbyUnitsWithinDistance(10) > pierceUnit.CountUnitsInFront()
                ? clusterUnit
                : pierceUnit;
        }

        internal static Vector3 BestWalkLocation
        {
            get
            {
                if (ClosestHealthGlobe(35) != null)
                    return  ClosestHealthGlobe(35).Position;

                // Prevent Default Attack
                if (TrinityPlugin.CurrentTarget.Type != TrinityObjectType.Destructible)
                {
                    //Logger.Log("Prevent Primary Attack ");
                    var targetPosition = TargetUtil.GetLoiterPosition(TrinityPlugin.CurrentTarget, 20f);
                    // return new TrinityPower(SNOPower.Walk, 7f, targetPosition);
                    return targetPosition;
                }
                return TrinityPlugin.CurrentTarget.Position;
            }
        }

        internal static List<TrinityCacheObject> TargetsInFrontOfMe(float maxRange, bool ignoreUnitsInAoE = false, bool ignoreElites = false)
        {
            return (from u in SafeList(ignoreElites)
                    where u.IsUnit &&
                    u.RadiusDistance <= maxRange && u.HasBeenInLoS &&
                    !(ignoreUnitsInAoE && u.IsStandingInAvoidance) &&
                    !(ignoreElites && u.IsEliteRareUnique)
                    orderby u.CountUnitsInFront() descending
                    select u).ToList();
        }
        internal static TrinityCacheObject GetBestPierceTarget(float maxRange, bool ignoreUnitsInAoE = false, bool ignoreElites = false)
        {
            var result = TargetsInFrontOfMe(maxRange, ignoreUnitsInAoE, ignoreElites).FirstOrDefault();
            return result ?? PhelonTargeting.BestAoeUnit(!ignoreUnitsInAoE);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radius">Cluster Radius</param>
        /// <param name="maxRange">Unit Max Distance</param>
        /// <param name="count">Minimum number of mobs</param>
        /// <param name="useWeights">Include Mobs with Weight or not</param>
        /// <param name="includeUnitsInAoe">Include mobs in AoE or not</param>
        /// <param name="ignoreElites">Ingore elites or not/param>
        /// <returns></returns>
        internal static TrinityCacheObject GetBestClusterUnit(
            float clusterRadius = 15f, float maxSearchRange = 65f, bool useWeights = true, bool includeUnitsInAoe = true,
            bool ignoreElites = false, bool inLineOfSight = false)
        {
            if (clusterRadius < 1f)
                clusterRadius = 1f;
            if (maxSearchRange > 60f)
                maxSearchRange = 60;

            var clusterUnits =
                (from u in SafeList(includeUnitsInAoe)
                    where u.IsUnit &&
                          ((useWeights && u.Weight > 0) || !useWeights) &&
                          !(ignoreElites && u.IsEliteRareUnique) &&
                          (!inLineOfSight || u.IsInLineOfSight()) &&
                          u.RadiusDistance <= maxSearchRange && !u.IsSafeSpot
                    orderby
                        u.NearbyUnitsWithinDistance(clusterRadius) descending,
                        u.Distance,
                        u.HitPointsPct descending
                    select u).ToList();

            return clusterUnits.FirstOrDefault();
        }

        public static TrinityCacheObject BestEliteInRange(float range, bool objectsInAoe = false)
        {
            return (from u in SafeList(objectsInAoe)
                    where u.IsUnit &&
                    u.IsBossOrEliteRareUnique &&
                    u.Distance <= range
                    orderby
                     u.NearbyUnitsWithinDistance(range) descending,
                     u.Distance,
                     u.HitPointsPct descending
                    select u).FirstOrDefault();

        }

        internal static Vector3 ClosestOcculous
        {
            get
            {
                var trinityCacheObject = GetOculusBuffDiaObjects(35f).FirstOrDefault();
                return trinityCacheObject?.Position ?? Vector3.Zero;
            }
        }

        internal static TrinityCacheObject ClosestHealthGlobe(float distance = 45, bool objectsInAoe = false)
        {
            return (from u in SafeList(objectsInAoe)
                where u.Type == TrinityObjectType.HealthGlobe && u.RadiusDistance <= distance
                select u).FirstOrDefault();
        }

        internal static bool WithInDistance(TrinityCacheObject actor, TrinityCacheObject actor2, float distance, bool objectsInAoe = false)
        {
            return
                SafeList(objectsInAoe).Any(
                    m => m.ActorSNO == actor.ActorSNO && m.Position.Distance(actor2.Position) <= distance);
        }

        internal static bool WithInDistance(TrinityCacheObject actor, Vector3 unitLocation, float distance, bool objectsInAoe = false)
        {
            return
                SafeList(objectsInAoe).Any(
                    m => m.ActorSNO == actor.ActorSNO && m.Position.Distance(unitLocation) <= distance);
        }

        internal static List<TrinityCacheObject> GetDiaObjects(uint actorSNO, float range = 25f, bool objectsInAoe = false)
        {
            return
                (from u in SafeList(objectsInAoe)
                    where u.RadiusDistance <= range &&
                          u.ActorSNO == actorSNO
                    orderby u.Distance
                    select u).ToList();
        }

        internal static Vector3 GetDiaObjectBestClusterPoint(uint actorSNO, float radius = 15f, float maxRange = 45f,
            bool useWeights = true, bool includeUnitsInAoe = true, bool objectsInAoe = false)
        {
            var clusterUnits =
                (from u in SafeList(objectsInAoe)
                    where u.ActorSNO == actorSNO &&
                          u.RadiusDistance <= maxRange
                    orderby u.NearbyUnitsWithinDistance(radius),
                        u.Distance descending
                    select u.Position).ToList();

            return clusterUnits.Any() ? clusterUnits.FirstOrDefault() : Vector3.Zero;
        }

        internal static List<TrinityCacheObject> GetTwisterDiaObjects(float range = 25f, bool objectsInAoe = false)
        {
            return
                (from u in SafeList(objectsInAoe)
                    where u.RadiusDistance <= range &&
                          u.ActorSNO == 322236
                    orderby u.Distance
                    select u).ToList();
        }

        internal static Vector3 GetBestTwsiterClusterPoint(float radius = 15f, float maxRange = 45f,
            bool useWeights = true, bool includeUnitsInAoe = true, bool objectsInAoe = false)
        {
            if (radius < 5f)
                radius = 5f;
            if (maxRange > 75f)
                maxRange = 75f;

            var clusterUnits =
                (from u in SafeList(objectsInAoe)
                    where u.ActorSNO == 322236 &&
                          u.RadiusDistance <= maxRange
                    orderby u.NearbyUnitsWithinDistance(radius),
                        u.Distance descending
                    select u.Position).ToList();

            return clusterUnits.Any() ? clusterUnits.FirstOrDefault() : Vector3.Zero;
        }

        internal static List<TrinityCacheObject> GetOculusBuffDiaObjects(float range = 25f, bool objectsInAoe = false)
        {
            return
                (from u in SafeList(objectsInAoe)
                    where u.RadiusDistance <= range &&
                          u.ActorSNO == 433966
                    orderby u.Distance
                    select u).ToList();
        }

        public static Vector3 PointBehind(Vector3 point, bool objectsInAoe = false)
        {
            return MathEx.GetPointAt(point, -7f, TrinityPlugin.Player.Rotation);
        }
    }
}
