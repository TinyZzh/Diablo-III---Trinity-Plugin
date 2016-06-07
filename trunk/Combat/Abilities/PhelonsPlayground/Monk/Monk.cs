﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Reference;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity.Combat.Abilities.PhelonsPlayground.Monk
{
    partial class Monk : CombatBase
    {
        public static bool IszDPS
        {
            get
            {
                return (Sets.IstvansPairedBlades.IsEquipped ||
                        Sets.Innas.CurrentBonuses == 2 && Sets.ThousandStorms.CurrentBonuses >= 1) &&
                       Skills.Monk.CycloneStrike.IsActive;
            }
        }

        public static TrinityPower GetPower()
        {
            if (Player.IsInTown)
                return null;
            TrinityPower power = Unconditional.PowerSelector();

            if (power == null && CurrentTarget != null && CurrentTarget.IsUnit)
            {
                if (IszDPS)
                {
                    Vector3 bestBuffPosition;
                    return PhelonUtils.BestBuffPosition(12, Player.Position, true, out bestBuffPosition) &&
                           bestBuffPosition.Distance(Player.Position) < 5
                        ? ZDps.PowerSelector()
                        : new TrinityPower(SNOPower.Walk, 3f, bestBuffPosition);
                }
                //power = ZDps.PowerSelector() ?? new TrinityPower(SNOPower.Walk, 0f, PhelonUtils.BestDpsPosition(35f, true));
            }
            return power;
        }
    }
}