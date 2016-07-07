﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Framework;
using Trinity.Framework.Actors.ActorTypes;
using Trinity.Framework.Objects.Memory;
using Zeta.Game;

namespace Trinity.Cache
{
    public static class RiftProgression
    {
        public static float CurrentProgressionPct => Core.MemoryModel.Globals.RiftProgressionPct;

        public static bool IsInRift
        {
            get
            {
                if (ZetaDia.CurrentRift == null || ZetaDia.IsLoadingWorld)
                    return false;

                return ZetaDia.CurrentRift.IsStarted && DataDictionary.RiftWorldIds.Contains(ZetaDia.CurrentWorldSnoId);
            }
        }

        public static bool IsGaurdianSpawned
        {
            get { return IsInRift && ZetaDia.CurrentRift.HasGuardianSpawned; }
        }

        public static bool RiftComplete
        {
            get
            {
                if (ZetaDia.CurrentRift == null)
                    return false;

                return ZetaDia.CurrentRift.IsCompleted;
            }
        }

        public static void SetRiftValue(TrinityActor actor)
        {
            if (IsInRift)
            {
                double riftValue;
                TryGetRiftValue(actor, out riftValue);
                actor.RiftValuePct = riftValue;
            }
        }

        public static double GetRiftValue(TrinityActor actor)
        {
            var riftValue = 0d;
            if (IsInRift)
            {
                TryGetRiftValue(actor, out riftValue);
            }
            return riftValue;
        }

        public static bool TryGetRiftValue(TrinityActor actor, out double riftValuePct)
        {
            riftValuePct = -1;
            if (actor.IsMinion)
            {
                return true;
            }
            if (actor.IsBoss && !actor.IsSummoned)
            {
                riftValuePct = 10d;
                return true;
            }
            if (actor.IsMinion)
            {
                riftValuePct = 0.25d;
                return true;
            }
            if (actor.IsElite)
            {
                riftValuePct = 1d;
                return true;
            }
            if (Values.ContainsKey(actor.ActorSnoId))
            {
                var baseValue = Values[actor.ActorSnoId];
                riftValuePct = actor.IsElite ? baseValue * 4 : baseValue;
                return true;
            }
            return false;
        }

        public static Dictionary<int, double> Values = new Dictionary<int, double>()
        {            
            { 297708, 0.395064 },
            { 3847, 0.3029622 },
            { 6043, 0.4436438 },
            { 343183, 0.2662049 },
            { 4098, 0.1774855 },
            { 5211, 0.07101808 },
            { 5432, 0.1774855 },
            { 6042, 0.4436438 },
            { 6640, 0.1597416 },
            { 4550, 0.3549244 },
            { 6053, 0.3549244 },
            { 6028, 0.05327827 },
            { 56784, 0.008918582 },
            { 408485, 0.2662049 },
            { 5428, 0.1774855 },
            { 284713, 0.008918582 },
            { 5411, 0.2218452 },
            { 5275, 0.08876603 },
            { 409589, 0.008918582 },
            { 5388, 0.1774855 },
            { 423035, 0.4436438 },
            { 5371, 0.07101808 },
            { 239014, 0.4436438 },
            { 4984, 0.07101808 },
            { 106709, 0.9759604 },
            { 5235, 0.05327417 },
            { 218795, 0.08876603 },
            { 254175, 0.0266584 },
            { 367925, 0.05327827 },
            { 5278, 0.08876603 },
            { 6644, 0.08876603 },
            { 116299, 0.5323632 },
            { 4203, 0.03553442 },
            { 3893, 0.008918582 },
            { 205767, 0.6210827 },
            { 5346, 0.07101808 },
            { 192850, 0.6210827 },
            { 106711, 0.2041013 },
            { 4282, 0.2218452 },
            { 418902, 0.8872409 },
            { 3361, 0.05327827 },
            { 106710, 0.230713 },
            { 169615, 0.1774855 },
            { 131280, 0.1774855 },
            { 133669, 0.3549244 },
            { 315922, 0.1331258 },
            { 4738, 0.3105647 },
            { 278136, 0.0266584 },
            { 304307, 0.2041013 },
            { 202856, 0.008918582 },
            { 4551, 0.3549244 },
            { 4083, 0.0266584 },
            { 5347, 0.07101808 },
            { 6646, 0.07101808 },
            { 261545, 0.1774855 },
            { 4099, 0.1774855 },
            { 4090, 0.3549244 },
            { 4106, 0.1065058 },
            { 4299, 0.05327827 },
            { 129000, 0.2218452 },
            { 60049, 0.008918582 },
            { 262442, 0.3549244 },
            { 4286, 0.07102218 },
            { 4303, 0.2218452 },
            { 106712, 0.1774855 },
            { 208962, 0.6210827 },
            { 6359, 0.7098021 },
            { 137992, 0.7098021 },
            { 5513, 0.1774855 },
            { 5209, 0.07101808 },
            { 5238, 0.06215026 },
            { 5433, 0.1774855 },
            { 4760, 0.3105647 },
            { 4105, 0.1065058 },
            { 4201, 0.07101808 },
            { 6647, 0.08876603 },
            { 5467, 0.008918582 },
            { 4287, 0.07102218 },
            { 237876, 0.6210827 },
            { 5191, 0.5323632 },
            { 144315, 0.03553442 },
            { 106708, 0.2662049 },
            { 418911, 0.1774855 },
            { 296283, 0.3016927 },
            { 5276, 0.08876603 },
            { 6027, 0.05327827 },
            { 4085, 0.0266584 },
            { 326670, 0.3105647 },
            { 282027, 0.2218452 },
            { 5381, 0.08876603 },
            { 4100, 0.1774855 },
            { 4091, 0.3549244 },
            { 5434, 0.1774855 },
            { 5430, 0.1774855 },
            { 418922, 0.1774855 },
            { 219673, 0.1774855 },
            { 224636, 0.008918582 },
            { 82764, 0.008918582 },
            { 305579, 0.1774855 },
            { 418918, 0.1774855 },
            { 5390, 0.1774855 },
            { 4089, 0.3549244 },
            { 152679, 0.1774855 },
            { 3342, 0.8872409 },
            { 282789, 0.08876603 },
            { 60722, 0.6210827 },
            { 336555, 0.1774855 },
            { 4153, 0.5323632 },
            { 4080, 0.0266584 },
            { 418923, 0.1774855 },
            { 4072, 0.5323632 },
            { 135611, 0.008918582 },
            { 374384, 0.05327827 },
            { 237333, 0.008918582 },
            { 6038, 0.1331258 },
            { 6653, 0.1331258 },
            { 5236, 0.09763802 },
            { 6036, 0.1331258 },
            { 175833, 0.1774855 },
            { 4204, 0.03553442 },
            { 4073, 0.5323632 },
            { 170781, 0.07101808 },
            { 5393, 0.07101808 },
            { 4071, 0.5323632 },
            { 3362, 0.008918582 },
            { 141194, 0.1331258 },
            { 169456, 0.08876603 },
            { 332679, 0.9759604 },
            { 5468, 0.008918582 },
            { 365, 0.1774855 },
            { 6039, 0.1331258 },
            { 5436, 0.1774855 },
            { 4982, 0.07101808 },
            { 310894, 0.1774855 },
            { 4746, 0.04440634 },
            { 5395, 0.07101808 },
            { 5474, 0.2662049 },
            { 4763, 0.3105647 },
            { 6052, 0.3549244 },
            { 4084, 0.0266584 },
            { 4158, 0.04440225 },
            { 5375, 0.05327827 },
            { 283269, 0.008918582 },
            { 418907, 0.1774855 },
            { 4157, 0.03553033 },
            { 4290, 0.1774855 },
            { 4156, 0.07101808 },
            { 3982, 0.3549244 },
            { 5387, 0.1774855 },
            { 294136, 0.08876603 },
            { 179343, 0.07101808 },
            { 5367, 0.03553033 },
            { 5372, 0.1171522 },
            { 6054, 0.3549244 },
            { 4202, 0.03553442 },
            { 4295, 0.2218452 },
            { 5208, 0.07101808 },
            { 288691, 0.7098021 },
            { 258678, 0.9759604 },
            { 4300, 0.05327827 },
            { 4296, 0.2218452 },
            { 418924, 0.1331258 },
            { 272330, 0.4436438 },
            { 6639, 0.1597416 },
            { 90453, 0.08876603 },
            { 4093, 0.0266584 },
            { 6654, 0.1508696 },
            { 4747, 0.04440634 },
            { 6035, 0.1331258 },
            { 5396, 0.07457098 },
            { 5412, 0.2218452 },
            { 4070, 0.5323632 },
            { 310893, 0.07101808 },
            { 5475, 0.1774855 },
            { 5512, 0.08876603 },
            { 5348, 0.07101808 },
            { 4104, 0.1065058 },
            { 276309, 0.1508696 },
            { 6655, 0.2218452 },
            { 4564, 0.05327827 },
            { 121353, 0.3549244 },
            { 6046, 0.05327827 },
            { 131278, 0.08876603 },
            { 3338, 0.8872409 },
            { 6356, 0.5323632 },
            { 5187, 0.04440225 },
            { 210502, 0.1331258 },
            { 218638, 0.1331258 },
            { 218639, 0.1331258 },
            { 434, 0.2218452 },
            { 6024, 0.05327827 },
            { 6059, 0.1774855 },
            { 4542, 0.1774855 },
            { 5581, 0.6210827 },
            { 4541, 0.1065058 },
            { 136864, 0.1774855 },
            { 6651, 0.08876603 },
            { 310888, 0.04440225 },
            { 6025, 0.05327827 },
            { 241288, 0.3549244 },
            { 77796, 0.1331258 },
            { 373145, 0.6654423 },
            { 271579, 0.5323632 },
            { 327403, 0.1774855 },
            { 279052, 0.0266584 },
            { 334324, 0.1331258 },

        };

    }
}
