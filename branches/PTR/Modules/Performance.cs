using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Framework;
using Trinity.Framework.Helpers;
using Trinity.Framework.Objects;
using Zeta.Bot;
using Logger = Trinity.Framework.Helpers.Logger;

namespace Trinity.Modules
{
    public class Performance : Module
    {
        protected override int UpdateIntervalMs => 1000;

        protected override void OnPulse() => UpdateTicksPerSecond();

        public int DefaultTPS { get; } = 1000;

        private void UpdateTicksPerSecond()
        {
            if (Core.Settings.Advanced.TpsEnabled)
            {
                if (BotMain.TicksPerSecond != Core.Settings.Advanced.TpsLimit)
                {
                    BotMain.TicksPerSecond = Core.Settings.Advanced.TpsLimit;
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Bot TPS set to {0}", Core.Settings.Advanced.TpsLimit);
                }
            }
            else
            {
                if (BotMain.TicksPerSecond != DefaultTPS)
                {
                    BotMain.TicksPerSecond = DefaultTPS;
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Reset bot TPS to default: {0}", BotMain.TicksPerSecond);
                }
            }
        }
    }
}
