using System;
using System.Globalization;
using System.Text;
using System.Windows;
using Trinity.Components.Combat;
using Trinity.Framework;
using Trinity.Framework.Objects;
using Zeta.Bot;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity.Modules
{
    public class WindowTitle : Module
    {
        protected override int UpdateIntervalMs => 1000;

        protected override void OnPulse()
        {
            var title = string.Empty;

            if (Core.Settings.Advanced.ShowBattleTag)
                title += $"{Core.Player.BattleTag} ";

            if (Core.Settings.Advanced.ShowHeroName)
                title += $"{Core.Player.Name} ";

            if (Core.Settings.Advanced.ShowHeroClass)
                title += $"{Core.Player.ActorClass} ";

            if (!string.IsNullOrEmpty(title))
                Application.Current.Dispatcher.BeginInvoke((Action)(() 
                    => Application.Current.MainWindow.Title = title));
        }
    }
}
