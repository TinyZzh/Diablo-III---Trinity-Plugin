using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using QuestTools.Helpers;

namespace QuestTools.UI
{
    public class ConfigWindow
    {
        private static Window _configWindow;

        public static void CloseWindow()
        {
            _configWindow.Close();
        }

        private static string replaceNamespace(string xaml, string xmlns)
        {
            var asmName = Assembly.GetExecutingAssembly().GetName().Name;
            string newxmlns = xmlns.Insert(xmlns.Length - 1, ";assembly=" + asmName);
            return xaml.Replace(xmlns, newxmlns);
        }

        private static string _xamlContent;
        private static UserControl _mainControl;

        public static Window GetDisplayWindow()
        {
            if (_configWindow == null)
            {
                _configWindow = new Window();
            }
            try
            {
                string assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                if (assemblyPath != null)
                {
                    // This hooks up our object with our UserControl DataBinding
                    _configWindow.DataContext = SettingsModel.Instance;
                    _configWindow.Resources["LegendaryGems"] = DataDictionary.LegendaryGems;

                    if (String.IsNullOrWhiteSpace(_xamlContent))
                    {
                        _xamlContent = File.ReadAllText(Path.Combine(assemblyPath, "Plugins", "QuestTools", "UI", "ConfigWindow.xaml"));

                        _xamlContent = replaceNamespace(_xamlContent, "xmlns:qt=\"clr-namespace:QuestTools\"");
                        _xamlContent = replaceNamespace(_xamlContent, "xmlns:ui=\"clr-namespace:QuestTools.UI\"");
                        _xamlContent = replaceNamespace(_xamlContent, "xmlns:nav=\"clr-namespace:QuestTools.Navigation\"");
                        _xamlContent = replaceNamespace(_xamlContent, "xmlns:h=\"clr-namespace:QuestTools.Helpers\"");
                    }

                    if (_mainControl == null)
                        _mainControl = (UserControl)XamlReader.Load(new MemoryStream(Encoding.UTF8.GetBytes(_xamlContent)));

                    _configWindow.Content = _mainControl;
                }
                _configWindow.MinWidth = 600;
                _configWindow.Width = 600;
                _configWindow.MinHeight = 650;
                _configWindow.Height = 650;
                _configWindow.ResizeMode = ResizeMode.CanResize;

                _configWindow.Title = "QuestTools";

                if (SettingsModel.Instance.Settings.GemPriority == null)
                    SettingsModel.Instance.Settings.SetDefaultGemPriority();

                if (SettingsModel.Instance.Settings.GemPriority.Count() != DataDictionary.LegendaryGems.Count)
                {
                    foreach (var gem in DataDictionary.LegendaryGems)
                    {
                        if (!SettingsModel.Instance.Settings.GemPriority.Contains(gem.Value))
                        {
                            Logger.Log("Adding {0} to legendary gems list", gem.Value);
                            SettingsModel.Instance.Settings.GemPriority.Add(gem.Value);
                        }
                    }
                }

                if (SettingsModel.Instance.Settings.RiftKeyPriority == null)
                    SettingsModel.Instance.Settings.SetDefaultRiftKeyPriority();

                _configWindow.Closed += ConfigWindow_Closed;
                Application.Current.Exit += ConfigWindow_Closed;
            }
            catch (Exception ex)
            {
                Logger.Error("Error opening QuestTools Config Window: {0}", ex);
            }
            return _configWindow;
        }

        static void ConfigWindow_Closed(object sender, System.EventArgs e)
        {
            QuestToolsSettings.Instance.Save();
            if (_configWindow == null)
                return;
            _configWindow.Closed -= ConfigWindow_Closed;
            Application.Current.Exit -= ConfigWindow_Closed;
            _configWindow = null;
        }
    }
}
