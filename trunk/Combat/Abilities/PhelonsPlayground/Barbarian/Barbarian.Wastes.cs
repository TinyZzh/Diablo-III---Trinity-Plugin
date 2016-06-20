using System.Linq;
using Trinity.Reference;

namespace Trinity.Combat.Abilities.PhelonsPlayground.Barbarian
{
    partial class Barbarian
    {
        public class WrathOfTheWastes
        {
            public static TrinityPower PowerSelector()
            {
                TrinityCacheObject target;

                if (ShouldWhirlWind(out target))
                    return CastWhirlWind(target);

                return null;
            }

            public static bool ShouldWhirlWind(out TrinityCacheObject target)
            {
                target = CurrentTarget;
                if (!Skills.Barbarian.Whirlwind.CanCast())
                    return false;

                return target != null && Player.PrimaryResource > 10;
            }

            public static TrinityPower CastWhirlWind(TrinityCacheObject target)
            {
                var targetPosition = target.Distance < 10 ?
                TargetUtil.GetZigZagTarget(target.Position, 25f, true) : target.Position;
                return new TrinityPower(Skills.Barbarian.Whirlwind.SNOPower, 25f, targetPosition,
                    TrinityPlugin.CurrentWorldDynamicId, -1, 0, 1);
            }
        }
    }
}