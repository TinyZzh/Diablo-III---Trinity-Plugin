using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity.Framework
{
    public static class ActorExtensions
    {
        public static InventorySlot GetInventorySlot(this ACD acd)
        {
            return ZetaDia.Memory.Read<InventorySlot>(acd.BaseAddress + 0x114);
        }
    }
}
