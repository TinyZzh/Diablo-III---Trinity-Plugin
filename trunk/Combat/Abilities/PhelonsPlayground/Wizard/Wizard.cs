using Trinity.Reference;
using Trinity.Technicals;
using Zeta.Game.Internals.Actors;

namespace Trinity.Combat.Abilities.PhelonsPlayground.Wizard
{
    partial class Wizard : CombatBase
    {
        private static int FirebirdsCount = Sets.FirebirdsFinery.CurrentBonuses;
        private static int VyrsCount = Sets.VyrsAmazingArcana.CurrentBonuses;
        private static int TalRashasCount = Sets.TalRashasElements.CurrentBonuses;
        private static int DMOCount = Sets.DelseresMagnumOpus.CurrentBonuses;
        private static int TalRashaStackCount = GetBuffStacks(SNOPower.P2_ItemPassive_Unique_Ring_052);

        public static bool IsFlashfire = TalRashasCount == 3 && Legendary.WandOfWoh.IsEquipped;
        public static bool isTalVys = TalRashasCount == 3 && VyrsCount >= 1;
        public static bool IsTwister = TalRashasCount == 3 && Legendary.TheTwistedSword.IsEquipped;


        public static TrinityPower GetPower()
        {
            if (Player.IsInTown)
                return null;
            TrinityPower power = Unconditional.PowerSelector();
            if (power == null && CurrentTarget != null && CurrentTarget.IsUnit)
            {
                if (FirebirdsCount == 3 || VyrsCount == 3)
                {
                    if (Unconditional.CanTeleport)
                    {
                        var twisterPosition = IsInParty && PhelonGroupSupport.Monk != null
                            ? PhelonGroupSupport.Monk.Position
                            : PhelonUtils.BestDpsPosition(35f, 14f, true);

                        power = twisterPosition.Distance(Player.Position) > 5
                            ? new TrinityPower(SNOPower.Walk, 3f, twisterPosition)
                            : Firebirds.PowerSelector();
                    }
                    else
                        power = Firebirds.PowerSelector();
                }
                if (TalRashasCount == 3)
                {
                    if (isTalVys)
                        power = TalRasha.VyrArchon.PowerSelector();

                    if (IsTwister)
                    {
                        var twisterPosition = IsInParty && PhelonGroupSupport.Monk != null
                        ? PhelonGroupSupport.Monk.Position
                        : PhelonUtils.BestDpsPosition(35f, 14f, true);

                        power = twisterPosition.Distance(Player.Position) > 5
                            ? new TrinityPower(SNOPower.Walk, 3f, twisterPosition)
                            : TalRasha.EnergyTwister.PowerSelector();
                    }

                    if (IsFlashfire)
                        power = new TrinityPower(SNOPower.Walk, 3f, CurrentTarget.Position);
                }
            }
            return power;
        }
    }
}