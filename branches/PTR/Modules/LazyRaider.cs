using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.Framework;
using Trinity.Framework.Helpers;
using Trinity.Framework.Objects;
using Zeta.Bot;
using Logger = Trinity.Framework.Helpers.Logger;

namespace Trinity.Modules
{
    public class LazyRaider : Module
    {
        protected override int UpdateIntervalMs => 250;

        protected override void OnPulse() => PauseWhileMouseDown();

        private void PauseWhileMouseDown()
        {
            if (Core.Settings.Advanced.LazyRaider && !BotMain.IsPaused && MouseLeft())
            {
                BotMain.PauseWhile(MouseLeft);
            }
        }

        private static bool MouseLeft()
        {
            var result = (Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left;
            if (result)
            {
                Logger.Log("Mouse Left Down LazyRaider Pause");
            }
            return result;
        }
    }
}


