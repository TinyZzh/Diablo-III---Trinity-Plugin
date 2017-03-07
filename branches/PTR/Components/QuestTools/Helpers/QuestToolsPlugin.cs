using System;
using System.Windows;
using Zeta.Common.Plugins;
using Zeta.Common.Xml;

namespace QuestTools.Helpers
{
    /// <summary>
    /// Wrapper to simplify IPlugin declaration with optional members
    /// </summary>
    public abstract class QuestToolsPlugin : IPlugin
    {
        public string Name { get; set; }
        public Version Version { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }

        public Window DisplayWindow
        {
            get
            {
                if (SettingsXaml == null || SettingsClass == null) return null;
                return WindowManager.GetDisplayWindow(SettingsXaml, SettingsClass, Name);                                    
            }
        }

        public XmlSettings SettingsClass { get; set; }
        public string SettingsXaml { get; set; }

        public bool Equals(IPlugin other) { return (other.Name == Name) && (other.Version == Version); }

        public virtual void OnInitialize() { }
        public virtual void OnEnabled() { }
        public virtual void OnDisabled() { }
        public virtual void OnShutdown() { }
        public virtual void OnPulse() { }
    }

}
