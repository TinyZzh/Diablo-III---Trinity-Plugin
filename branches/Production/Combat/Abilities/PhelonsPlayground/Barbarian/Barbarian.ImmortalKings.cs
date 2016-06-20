﻿namespace Trinity.Combat.Abilities.PhelonsPlayground.Barbarian
{
    partial class Barbarian
    {
        public class ImmortalKingsCall
        {
            public static TrinityPower PowerSelector()
            {
                TrinityPower power = UnconditionalPower();
                power = CurrentTarget == null ? OutofCombatPower() : InCombatPower();
                return power;
            }
            public static TrinityPower OutofCombatPower()
            {
                return null;
            }

            public static TrinityPower InCombatPower()
            {
                return null;
            }

            public static TrinityPower UnconditionalPower()
            {
                return null;
            }
        }
    }
}