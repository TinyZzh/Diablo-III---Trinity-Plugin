using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Framework;
using Trinity.Framework.Helpers;
using Trinity.Framework.Objects;

namespace Trinity.Modules
{
    public class ResetGameState : Module
    {
        protected override void OnWorldChanged(ChangeEventArgs<int> args)
        {
            Reset();
        }

        protected override void OnBotStart()
        {
            Reset();
        }

        public void Reset()
        {
            Core.Actors.Clear();
            Core.Hotbar.Clear();
            Core.Inventory.Clear();
            Core.Buffs.Clear();
            Core.Targets.Clear();
        }

    }
}
