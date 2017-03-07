using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using QuestTools.Helpers;

namespace QuestTools.UI
{
    class SettingsModel
    {
        private static SettingsModel _instance;
        public static SettingsModel Instance
        {
            get { return _instance ?? (_instance = new SettingsModel()); }
        }

        public QuestToolsSettings Settings { get { return QuestToolsSettings.Instance; } }

        public Dictionary<int, string> LegendaryGems { get { return DataDictionary.LegendaryGems; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsModel"/> class.
        /// </summary>
        public SettingsModel()
        {
            RiftKeyOrderUp = new RelayCommand(RiftKeyOrderUpAction);
            RiftKeyOrderDown = new RelayCommand(RiftKeyOrderDownAction);
            GemOrderUp = new RelayCommand(GemOrderUpAction);
            GemOrderDown = new RelayCommand(GemOrderDownAction);
            ResetAllSettingsCommand = new RelayCommand(ResetAllSettingsAction);
        }

        private void ResetAllSettingsAction(object parameter)
        {
            var confirmed = MessageBox.Show("Are you sure you want to reset all settings?", "Settings Reset Confirmation",
                                             MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
            if (confirmed == MessageBoxResult.Yes)
            {
                Settings.SetDefaults();
                Settings.SetDefaultGemPriority();
                Settings.SetDefaultRiftKeyPriority();
            }
        }

        public ICommand RiftKeyOrderUp { get; set; }
        public ICommand RiftKeyOrderDown { get; set; }
        public ICommand GemOrderUp { get; set; }
        public ICommand GemOrderDown { get; set; }
        public ICommand ResetAllSettingsCommand { get; set; }

        public void RiftKeyOrderUpAction(object selectedItem)
        {
            try
            {
                var keyType = (RiftKeyUsePriority)selectedItem;

                var priorityList = Instance.Settings.RiftKeyPriority.ToList();
                int currentIndex = priorityList.IndexOf(keyType);
                if (currentIndex == 0)
                    return;

                var newIndex = currentIndex - 1;
                priorityList.Remove(keyType);
                priorityList.Insert(newIndex, keyType);

                Instance.Settings.RiftKeyPriority = priorityList;
                Instance.Settings.Save();
            }
            catch (Exception ex)
            {
                Logger.Error("Error ordering rift key priority up for {0}, {1}", selectedItem, ex);
            }
        }
        public void RiftKeyOrderDownAction(object selectedItem)
        {
            try
            {
                var keyType = (RiftKeyUsePriority)selectedItem;

                var priorityList = Instance.Settings.RiftKeyPriority.ToList();
                int total = priorityList.Count;
                int currentIndex = priorityList.IndexOf(keyType);
                if (currentIndex == total - 1)
                    return;

                var newIndex = currentIndex + 1;

                priorityList.Remove(keyType);
                if (newIndex == total - 1)
                    priorityList.Add(keyType);
                else
                    priorityList.Insert(newIndex, keyType);

                Instance.Settings.RiftKeyPriority = priorityList;
                Instance.Settings.Save();
            }
            catch (Exception ex)
            {
                Logger.Error("Error ordering rift key priority down for {0}, {1}", selectedItem, ex);
            }

        }
        public void GemOrderUpAction(object selectedItem)
        {
            try
            {
                var gem = (string)selectedItem;

                if (!LegendaryGems.ContainsValue((string)selectedItem))
                {
                    Logger.Error("Unknown gem when parsing gem name, order up action {0}", gem);
                    return;
                }

                var priorityList = Instance.Settings.GemPriority.ToList();
                int currentIndex = priorityList.IndexOf(gem);
                if (currentIndex == 0)
                    return;

                priorityList.Remove(gem);
                priorityList.Insert(currentIndex - 1, gem);

                Instance.Settings.GemPriority = priorityList;
                Instance.Settings.Save();
            }
            catch (Exception ex)
            {
                Logger.Error("Error ordering gem priority up for {0}, {1}", selectedItem, ex);
            }
        }
        public void GemOrderDownAction(object selectedItem)
        {
            try
            {
                var gem = (string)selectedItem;

                if (!LegendaryGems.ContainsValue(gem))
                {
                    Logger.Error("Unknown gem when parsing gem name, order up action {0}", gem);
                    return;
                }

                var priorityList = Instance.Settings.GemPriority.ToList();
                int total = priorityList.Count;
                int currentIndex = priorityList.IndexOf(gem);
                if (currentIndex == total - 1)
                    return;

                int newIndex = currentIndex + 1;

                priorityList.Remove(gem);
                if (newIndex == total - 1)
                    priorityList.Add(gem);
                else
                    priorityList.Insert(newIndex, gem);

                Instance.Settings.GemPriority = priorityList;
                Instance.Settings.Save();
            }
            catch (Exception ex)
            {
                Logger.Error("Error ordering gem priority up for {0}, {1}", selectedItem, ex);
            }
        }
    }
}
