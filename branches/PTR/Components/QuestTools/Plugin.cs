using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using QuestTools.Helpers;
using QuestTools.UI;
using Zeta.Common.Plugins;
using Trinity.Framework.Objects;

namespace QuestTools
{
    public class Plugin : Component
    {
        private const string NAME = "QuestTools";
        private const string AUTHOR = "rrrix, xzjv";
        private const string DESCRIPTION = "Advanced Demonbuddy Profile Support";

        public Version Version { get { return QuestTools.PluginVersion; } }

        internal static DateTime LastPluginPulse = DateTime.MinValue;

        public static double GetMillisecondsSincePulse()
        {
            return DateTime.UtcNow.Subtract(LastPluginPulse).TotalMilliseconds;
        }

        protected override void OnPulse()
        {
            LastPluginPulse = DateTime.UtcNow;
            QuestTools.Pulse();
        }

        protected override void OnPluginEnabled()
        {
            Logger.Log("v{0} Enabled", Version);
            BotEvents.WireUp();
            TabUi.InstallTab();
        }

        protected override void OnPluginDisabled()
        {
            Logger.Log("v{0} Disabled", Version);
            BotEvents.UnWire();
            TabUi.RemoveTab();
        }
    }

}
