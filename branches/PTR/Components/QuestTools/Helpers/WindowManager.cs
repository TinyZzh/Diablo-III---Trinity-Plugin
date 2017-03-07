using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Zeta.Common;
using Zeta.Common.Xml;

namespace QuestTools.Helpers
{
    /// <summary>
    /// Helper for plugins to handle window create, open and close
    /// </summary>
    class WindowManager
    {
        private static readonly Dictionary<string, Window> Windows = new Dictionary<string, Window>();

        public static Window GetDisplayWindow(string xamlFileName, XmlSettings settingsInstance, string windowTitle = "")
        {
            if (Windows.ContainsKey(xamlFileName) && Windows[xamlFileName] != null)
            {
                return Windows[xamlFileName];
            }
            return CreateDisplayWindow(xamlFileName, settingsInstance, windowTitle);
        }

        internal static Window CreateDisplayWindow (string xamlFileName, XmlSettings settingsInstance, string windowTitle)
        {
            var configWindow = new Window();

            try
            {
                var assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

                var pluginPath = Path.Combine(assemblyPath, "plugins");
                
                if (assemblyPath == null)
                    return null;

                var xamlInterfaceFile = FileManager.GetFile(pluginPath, xamlFileName);

                if (!File.Exists(xamlInterfaceFile))
                    return null;

                var xamlContent = File.ReadAllText(xamlInterfaceFile);

                configWindow.DataContext = settingsInstance;

                var mainControl = (UserControl) XamlReader.Load(new MemoryStream(Encoding.UTF8.GetBytes(xamlContent)));

                Action<object, CancelEventArgs> closingHandler = delegate(object sender, CancelEventArgs e)
                {
                    if (!CloseAllowed)
                    {
                        settingsInstance.Save();
                        configWindow.Visibility = Visibility.Hidden;
                        e.Cancel = true;
                        Logger.Debug("Hiding {0} Window", windowTitle);
                    }
                    else
                    {
                        Logger.Debug("Closing {0} Window", windowTitle);
                    }
                            
                };

                var windowBorderSize = 16;
                var windowHeaderSize = 37;

                configWindow.Content = mainControl;
                configWindow.Width = mainControl.Width + windowBorderSize;
                configWindow.Height = mainControl.Height + windowHeaderSize;
                configWindow.Title = windowTitle;
                configWindow.Closing += (s,e) => closingHandler(s,e);

                configWindow.Owner = Application.Current.MainWindow;
                configWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                Application.Current.MainWindow.Closing += Application_Closing;

                Windows.Add(xamlFileName, configWindow);

                return configWindow;
                
            }
            catch (Exception ex)
            {
                Logger.Debug("Failed to load Config UI {0}", ex);
            }
            return null;
        }

        public static bool CloseAllowed { get; set; }

        private static void Application_Closing(object sender, CancelEventArgs e)
        {
            Logger.Debug("Application_Closing");
            CloseAllowed = true;   
            Windows.ForEach(w => w.Value.Close());   
            Windows.Clear();
        }

    }

}


