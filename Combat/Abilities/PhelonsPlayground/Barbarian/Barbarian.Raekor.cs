using System;
using Trinity.Movement;
using Trinity.Reference;
using Zeta.Game.Internals.Actors;

namespace Trinity.Combat.Abilities.PhelonsPlayground.Barbarian
{
    partial class Barbarian
    {
        private class Raekor
        {
            public static TrinityPower PowerSelector()
            {
                if (Player.IsIncapacitated) return null;
                TrinityCacheObject target = null;
                if (ShouldAncientSpear(out target))
                    return CastAncientSpear(target);
                if (ShouldUseFuriousCharge(out target))
                    return CastFuriousCharge(target);
                return null;
            }

            private static bool ShouldUseFuriousCharge(out TrinityCacheObject target)
            {
                target = null;
                if (!Skills.Barbarian.FuriousCharge.CanCast())
                    return false;
                var targetGoal = Math.Floor(5*TrinityPlugin.Player.CooldownReductionPct);
                TrinityCacheObject bestPierce = PhelonUtils.GetBestClusterUnit(45);
                var bestPierceCount = bestPierce?.NearbyUnitsWithinDistance(7) ?? 0;
                TrinityCacheObject bestTarget = PhelonTargeting.BestAoeUnit(45, true);
                var bestTargetCount = bestTarget?.NearbyUnitsWithinDistance(7) ?? 0;
                TrinityCacheObject bestCluster = PhelonUtils.GetBestClusterUnit(7, 45);
                var bestClusterCount = bestCluster?.NearbyUnitsWithinDistance(7) ?? 0;
                if (!ClassMover.HasInfiniteCasting)
                {
                    if (bestTargetCount == 1 || bestTargetCount >= targetGoal)
                    {
                        target = bestTarget;
                        return true;
                    }
                    if (bestPierceCount == 1 || bestPierceCount >= targetGoal &&
                        bestClusterCount == 1 || bestClusterCount >= targetGoal)
                    {
                        if (bestClusterCount > bestPierceCount)
                        {
                            target = bestCluster;
                            return true;
                        }
                        target = bestPierce;
                        return true;
                    }
                    if (bestPierceCount != 1 && bestPierceCount < targetGoal &&
                        (bestClusterCount == 1 || bestClusterCount >= targetGoal))
                    {
                        target = bestCluster;
                        return true;
                    }
                    if (bestClusterCount != 1 && bestClusterCount < targetGoal &&
                        (bestPierceCount == 1 || bestPierceCount >= targetGoal))
                    {
                        target = bestPierce;
                        return true;
                    }
                }
                if (CurrentTarget.IsBossOrEliteRareUnique)
                {
                    target = CurrentTarget;
                    return true;
                }

                return false;
            }

            private static TrinityPower CastFuriousCharge(TrinityCacheObject target)
            {
                return new TrinityPower(SNOPower.Barbarian_FuriousCharge, 45,
                    PhelonUtils.PointBehind(target.Position));
            }

            private static bool ShouldAncientSpear(out TrinityCacheObject target)
            {
                target = null;

                if (!Skills.Barbarian.AncientSpear.CanCast())
                    return false;

                if (Sets.TheLegacyOfRaekor.IsFullyEquipped && GetBuffStacks(SNOPower.P2_ItemPassive_Unique_Ring_026) < 5)
                    return false;

                target = PhelonTargeting.BestAoeUnit(60, true).IsInLineOfSight()
                    ? PhelonTargeting.BestAoeUnit(60, true)
                    : PhelonUtils.GetBestClusterUnit(10, 60, false, true, false, true);

                if (target == null)
                    return false;
                //if (Skills.Barbarian.FuriousCharge.Charges > 0 &&
                //    GetBuffStacks(SNOPower.P2_ItemPassive_Unique_Ring_026) <
                //    Math.Floor(Skills.Barbarian.FuriousCharge.Charges*2.5))
                //    return false;

                return target.Distance <= 60 && Player.PrimaryResourcePct > 0.95;
            }

            private static TrinityPower CastAncientSpear(TrinityCacheObject target)
            {
                return new TrinityPower(SNOPower.X1_Barbarian_AncientSpear, 60f,
                    target.Position);
            }
        }
    }
}