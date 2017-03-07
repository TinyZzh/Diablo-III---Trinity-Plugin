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
    public class Plugin : Component//IPlugin
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

        protected override void OnShutdown() { }

        //public string Author
        //{
        //    get { return AUTHOR; }
        //}

        //public string Description
        //{
        //    get { return DESCRIPTION; }
        //}

        //public System.Windows.Window DisplayWindow
        //{
        //    get { return ConfigWindow.GetDisplayWindow(); }
        //}

        //public string Name
        //{
        //    get { return NAME; }
        //}

        //public void OnInitialize()
        //{
        //    //Application.Current.Dispatcher.Invoke(UpdateDemonBuddyInterface);
        //}


        //public void UpdateDemonBuddyInterface()
        //{

        //    var botSettingsButton = DemonbuddyUI.SettingsButton.ButtonMenuItemsSource.First() as MenuItem;
        //    if (botSettingsButton == null)
        //        return;

        //    DemonbuddyUI.SettingsButton.Click += (sender, args) => botSettingsButton.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        //    // Add version number to status bar
        //    var versionItem = new StatusBarItem
        //    {
        //        HorizontalAlignment = HorizontalAlignment.Stretch,
        //        HorizontalContentAlignment = HorizontalAlignment.Right,
        //        Content = FileVersionInfo.GetVersionInfo(Application.ResourceAssembly.Location).FileVersion,
        //        Margin = new Thickness(-85, 0, 0, 0),                
        //        Padding = new Thickness(8, 3, 5, 3),
        //        BorderBrush = DemonbuddyUI.StatusBarText.BorderBrush,
        //        BorderThickness = DemonbuddyUI.StatusBarText.BorderThickness,
        //        Foreground = DemonbuddyUI.StatusBarText.Foreground,
        //        Background = DemonbuddyUI.StatusBarText.Background
        //    };
        //    DemonbuddyUI.StatusBarText.Margin = new Thickness(0, 0, 85, 0);
        //    DemonbuddyUI.StatusBarText.HorizontalAlignment = HorizontalAlignment.Left;
        //    DemonbuddyUI.StatusBarText.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        //    DemonbuddyUI.StatusBar.Items.Add(versionItem);
        //}

        public bool Equals(IPlugin other) { return (other.Name == Name) && (other.Version == Version); }
    }

}
