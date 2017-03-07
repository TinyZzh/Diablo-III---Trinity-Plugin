using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using QuestTools.Helpers;
using QuestTools.Navigation;
using Zeta.Common.Xml;
using Zeta.Game;
using Zeta.XmlEngine;

namespace QuestTools
{
    public enum RiftUpgradePriority
    {
        RiftKey,
        Gem
    }

    public enum RiftKeyUsePriority
    {
        Normal,
        Greater
    }

    [XmlElement("QuestToolsSettings")]
    public class QuestToolsSettings : XmlSettings
    {
        private bool _debugEnabled;
        private bool _allowProfileReloading;
        private bool _allowProfileRestarts;
        private bool _enableBetaFeatures;
        private bool _skipCutScenes;
        private bool _forceRouteMode;
        private RouteMode _routeMode;

        private static string _battleTagName;
        public static string BattleTagName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_battleTagName) && ZetaDia.Service.Hero.IsValid)
                    _battleTagName = ZetaDia.Service.Hero.BattleTagName;
                return _battleTagName;
            }
        }

        internal List<RiftKeyUsePriority> GetDefaultRiftKeyPriority()
        {
            return new List<RiftKeyUsePriority>
                {
                    RiftKeyUsePriority.Greater,
                    RiftKeyUsePriority.Normal
                };
        }

        public QuestToolsSettings() :
            base(Path.Combine(SettingsDirectory, BattleTagName, "QuestTools", "QuestToolsSettings.xml"))
        {
        }

        internal void SetDefaultGemPriority()
        {
            GemPriority = DataDictionary.LegendaryGems.Select(g => g.Value).ToList();
        }

        internal void SetDefaultRiftKeyPriority()
        {
            RiftKeyPriority = GetDefaultRiftKeyPriority();

        }

        private static QuestToolsSettings _instance;
        public static QuestToolsSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new QuestToolsSettings();

                    if (_instance.RiftKeyPriority == null)
                        _instance.SetDefaultRiftKeyPriority();

                    if (_instance.GemPriority == null)
                        _instance.SetDefaultGemPriority();
                }
                return _instance;
            }
        }

        [XmlElement("ForceRouteMode")]
        [DefaultValue(false)]
        [Setting]
        public bool ForceRouteMode
        {
            get
            {
                return _forceRouteMode;
            }
            set
            {
                if (_forceRouteMode == value)
                    return;
                _forceRouteMode = value;
                OnPropertyChanged("ForceRouteMode");
            }
        }

        [XmlElement("RouteMode")]
        [DefaultValue(RouteMode.Default)]
        [Setting]
        public RouteMode RouteMode
        {
            get
            {
                return _routeMode;
            }
            set
            {
                if (_routeMode == value)
                    return;
                _routeMode = value;
                OnPropertyChanged("RouteMode");
            }
        }

        [XmlElement("DebugEnabled")]
        [DefaultValue(true)]
        [Setting]
        public bool DebugEnabled
        {
            get
            {
                return _debugEnabled;
            }
            set
            {
                if (_debugEnabled == value)
                    return;
                _debugEnabled = value;
                OnPropertyChanged("DebugEnabled");
            }
        }

        [XmlElement("AllowProfileReloading")]
        [DefaultValue(true)]
        [Setting]
        public bool AllowProfileReloading
        {
            get
            {
                return _allowProfileReloading;
            }
            set
            {
                if (_allowProfileReloading == value)
                    return;
                _allowProfileReloading = value;
                OnPropertyChanged("AllowProfileReloading");
            }
        }

        [XmlElement("AllowProfileRestarts")]
        [DefaultValue(true)]
        [Setting]
        public bool AllowProfileRestarts
        {
            get
            {
                return _allowProfileRestarts;
            }
            set
            {
                if (_allowProfileRestarts == value)
                    return;
                _allowProfileRestarts = value;
                OnPropertyChanged("AllowProfileRestarts");
            }
        }

        [XmlElement("SkipCutScenes")]
        [DefaultValue(true)]
        [Setting]
        public bool SkipCutScenes
        {
            get
            {
                return _skipCutScenes;
            }
            set
            {
                if (_skipCutScenes == value)
                    return;
                _skipCutScenes = value;
                OnPropertyChanged("SkipCutScenes");
            }
        }

        // 2.1 Rift Settings below

        private List<RiftKeyUsePriority> _riftKeyUsePriority;
        [XmlElement("RiftKeyPriority23")]
        public List<RiftKeyUsePriority> RiftKeyPriority
        {
            get
            {
                return _riftKeyUsePriority;
            }
            set
            {
                if (_riftKeyUsePriority == value)
                    return;
                _riftKeyUsePriority = value;
                OnPropertyChanged("RiftKeyPriority");
            }
        }

        private List<string> _gemPriority;
        [XmlElement("GemPriority")]
        public List<string> GemPriority
        {
            get
            {
                return _gemPriority;
            }
            set
            {
                if (_gemPriority == value)
                    return;
                _gemPriority = value;
                OnPropertyChanged("GemPriority");
            }
        }

        private int _trialRiftMaxLevel;
        [XmlElement("TrialRiftMaxLevel")]
        [DefaultValue(40)]
        public int TrialRiftMaxLevel
        {
            get
            {
                if (_trialRiftMaxLevel == 0)
                    _trialRiftMaxLevel = 100;
                return _trialRiftMaxLevel;
            }
            set
            {
                if (_trialRiftMaxLevel == value)
                    return;
                _trialRiftMaxLevel = value;
                OnPropertyChanged("TrialRiftMaxLevel");
            }
        }

        private bool _upgradeKeyStones;
        [XmlElement("UpgradeKeyStones")]
        [DefaultValue(false)]
        public bool UpgradeKeyStones
        {
            get
            {
                return _upgradeKeyStones;
            }
            set
            {
                if (_upgradeKeyStones == value)
                    return;
                _upgradeKeyStones = value;
                OnPropertyChanged("UpgradeKeyStones");
            }
        }

        private bool _useHighestKeystone;
        [XmlElement("UseHighestKeystone")]
        [DefaultValue(false)]
        public bool UseHighestKeystone
        {
            get
            {
                return _useHighestKeystone;
            }
            set
            {
                if (_useHighestKeystone == value)
                    return;
                _useHighestKeystone = value;
                OnPropertyChanged("UseHighestKeystone");
            }
        }

        private bool _enableTrialRiftMaxLevel;
        [XmlElement("EnableTrialRiftMaxLevel")]
        [DefaultValue(false)]
        public bool EnableTrialRiftMaxLevel
        {
            get
            {
                return _enableTrialRiftMaxLevel;
            }
            set
            {
                if (_enableTrialRiftMaxLevel == value)
                    return;
                _enableTrialRiftMaxLevel = value;
                OnPropertyChanged("EnableTrialRiftMaxLevel");
            }
        }

        private bool _enableLimitRiftLevel;
        [XmlElement("EnableLimitRiftLevel")]
        [DefaultValue(false)]
        public bool EnableLimitRiftLevel
        {
            get
            {
                return _enableLimitRiftLevel;
            }

            set
            {
                if (_enableLimitRiftLevel == value)
                    return;
                _enableLimitRiftLevel = value;
                OnPropertyChanged("EnableLimitRiftLevel");
            }
        }

        private int _limitRiftLevel;
        [XmlElement("LimitRiftLevel")]
        [DefaultValue(999)]
        [Setting]
        public int LimitRiftLevel
        {
            get { return _limitRiftLevel; }
            set
            {
                if (value > 999)
                    value = 999;
                if (value < 1)
                    value = 1;

                _limitRiftLevel = value;
                OnPropertyChanged("LimitRiftLevel");
            }
        }

        private float _minimumGemChance;
        [XmlElement("MinimumGemChance")]
        [DefaultValue(0.6f)]
        public float MinimumGemChance
        {
            get
            {
                return _minimumGemChance;
            }
            set
            {
                if (_minimumGemChance == value)
                    return;
                _minimumGemChance = value;
                OnPropertyChanged("MinimumGemChance");
            }
        }
    }
}
