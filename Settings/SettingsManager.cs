﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Serialization;
using Trinity.Config;
using Trinity.Technicals;
using Trinity.UI;
using Trinity.UI.RadarUI;
using Trinity.UI.UIComponents;
using Trinity.UIComponents;

namespace Trinity.Settings
{
    public class SettingsManager
    {
        public static string SaveDirectory => Path.Combine(FileManager.SettingsPath, "Saved");

        /// <summary>
        /// Handle the process of exporting a settings file
        /// Fired when user clicks the Export button on Trinity settings window.
        /// </summary>
        public static ICommand ExportSettingsCommand => new RelayCommand(param =>
        {
            try
            {
                SettingsSelectionViewModel selectionViewModel;
                if (TryGetExportSelections(out selectionViewModel))
                {
                    var filePath = GetSaveFilePath();
                    if (string.IsNullOrEmpty(filePath))
                        return;

                    Logger.LogNormal($"Saving file to {filePath}");
                    var exportSettings = new TrinitySetting();
                    UILoader.DataContext.ViewModel.CopyTo(exportSettings);

                    RemoveSections(exportSettings, selectionViewModel);
                    exportSettings.SaveToFile(filePath);
                }                
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception in LoadSettingsCommand {ex}");
            }
        });

        /// <summary>
        /// Handle the process of importing a settings file
        /// Fired when user clicks the Import button on Trinity settings window.
        /// </summary>
        public static ICommand ImportSettingsCommand => new RelayCommand(param =>
        {
            try
            {
                var filePath = GetLoadFilePath();
                if (string.IsNullOrEmpty(filePath))
                    return;

                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"{filePath} not found");

                Logger.LogNormal($"Loading file: {filePath}");

                // Load settings into Settings window view model only
                // User still has to click save for it to actually be applied.    

                var settings = TrinitySetting.GetSettingsFromFile(filePath);
                var importedSections = GetSections(settings);
                SettingsSelectionViewModel selectionViewModel;

                if (TryGetImportSelections(importedSections, out selectionViewModel))
                {
                    RemoveSections(settings, selectionViewModel);
                    UILoader.DataContext.LoadSettings(settings);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception in LoadSettingsCommand {ex}");
            }
        });

        /// <summary>
        /// Get user to pick a filename and location for saving.
        /// </summary>
        private static string GetSaveFilePath()
        {
            if (!Directory.Exists(SaveDirectory))
                Directory.CreateDirectory(SaveDirectory);

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Xml Files (.xml)|*.xml|All Files (*.*)|*.*",
                FilterIndex = 1,
                Title = "Save Settings Xml File",
                InitialDirectory = SaveDirectory,
                OverwritePrompt = true,
            };

            var userClickedOk = saveFileDialog.ShowDialog();
            if (userClickedOk == DialogResult.OK)
            {
                return saveFileDialog.FileName;
            }
            return null;
        }

        /// <summary>
        /// Get user to pick a file to load
        /// </summary>
        private static string GetLoadFilePath()
        {
            if (!Directory.Exists(SaveDirectory))
                Directory.CreateDirectory(SaveDirectory);

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Xml Files (.xml)|*.xml|All Files (*.*)|*.*",
                FilterIndex = 1,
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
                InitialDirectory = SaveDirectory
            };
            var userClickedOk = openFileDialog.ShowDialog();
            if (userClickedOk == DialogResult.OK)
            {
                return openFileDialog.FileName;
            }
            return null;
        }

        /// <summary>
        /// Try to get user to pick settings sections for import.
        /// </summary>
        public static bool TryGetImportSelections(HashSet<SettingsSection> importedParts, out SettingsSelectionViewModel selectionViewModel)
        {
            var dataContext = new SettingsSelectionViewModel
            {
                Title = "Sections to Import",
                Description = "Only the sections selected below will be imported.",
                OkButtonText = "Import",
            };

            EnableSections(importedParts, dataContext);

            var window = UILoader.CreateNonModalWindow(
                "Modals\\SettingsSelection.xaml",
                "Import Settings",
                dataContext,
                350,
                225
            );

            return OpenSelectionsDialog(out selectionViewModel, dataContext, window);
        }

        /// <summary>
        /// Try to get user to pick settings sections for export.
        /// </summary>
        public static bool TryGetExportSelections(out SettingsSelectionViewModel selectionViewModel)
        {
            var dataContext = new SettingsSelectionViewModel
            {
                Title = "Sections to Export",
                Description = "Only the sections selected below will be exported.",
                OkButtonText = "Export",
            };

            EnableSections(SettingsSelectionViewModel.GetAllSections(), dataContext);

            var window = UILoader.CreateNonModalWindow(
                "Modals\\SettingsSelection.xaml",
                "Export Settings",
                dataContext,
                350,
                225
            );

            return OpenSelectionsDialog(out selectionViewModel, dataContext, window);
        }

        /// <summary>
        /// Open dialog for user to pick which sections of the settings to do something with.
        /// </summary>
        private static bool OpenSelectionsDialog(out SettingsSelectionViewModel selectionViewModel, SettingsSelectionViewModel dataContext, Window window)
        {
            dataContext.Selections = dataContext.Selections.OrderByDescending(s => s.IsEnabled).ToList();
            window.Owner = UILoader.ConfigWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            dataContext.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(dataContext.IsWindowOpen) && !dataContext.IsWindowOpen)
                    window.Close();
            };

            window.ContentRendered += (o, args) => dataContext.IsWindowOpen = true;
            window.Closed += (o, args) => dataContext.IsWindowOpen = false;
            window.ShowDialog();
            selectionViewModel = dataContext;
            return selectionViewModel.DialogResult == DialogResult.OK;
        }

        /// <summary>
        /// Look through a TrinitySetting object and return a list of the sections that are populated with data.
        /// </summary>
        private static HashSet<SettingsSection> GetSections(TrinitySetting settings)
        {
            var result = new HashSet<SettingsSection>();
            if (settings.Combat != null)
                result.Add(SettingsSection.Combat);
            if (settings.Loot?.ItemList != null)
                result.Add(SettingsSection.ItemList);
            if (settings.Gambling != null)
                result.Add(SettingsSection.Gambling);
            if (settings.KanaisCube != null)
                result.Add(SettingsSection.KanaisCube);
            if (settings.Loot?.TownRun != null)
                result.Add(SettingsSection.TownRun);
            if (settings.WorldObject != null)
                result.Add(SettingsSection.Objects);
            if (settings.Paragon != null)
                result.Add(SettingsSection.Paragon);
            if (settings.Advanced != null)
                result.Add(SettingsSection.Advanced);
            if (settings.Avoidance != null)
                result.Add(SettingsSection.Avoidance);
            if (settings.Loot?.Pickup != null)
                 result.Add(SettingsSection.ItemPickup);
            if (settings.Loot?.ItemRules != null)
                result.Add(SettingsSection.ItemRules);

            Logger.Log($"File contains {result.Count} sections: {string.Join(",", result)}");
            return result;
        }

        /// <summary>
        /// Clear the specified parts of a TrinitySetting object so that they have no data.
        /// </summary>
        public static void RemoveSections(TrinitySetting settings, SettingsSelectionViewModel selectionsViewModel)
        {
            // always remove notification section because of sensitive information.
            settings.Notification = null;

            foreach (var sectionEntry in selectionsViewModel.Selections)
            {
                if (sectionEntry.IsSelected)
                {
                    Logger.Log($"Importing Section: {sectionEntry.Section}");
                    continue;
                }

                switch (sectionEntry.Section)
                {
                    case SettingsSection.Combat:
                        settings.Combat = null;
                        break;
                    case SettingsSection.ItemList:
                        if(settings.Loot != null)
                            settings.Loot.ItemList = null;
                        break;
                    case SettingsSection.Gambling:
                        settings.Gambling = null;
                        break;
                    case SettingsSection.KanaisCube:
                        settings.KanaisCube = null;
                        break;
                    case SettingsSection.ItemPickup:
                        if (settings.Loot != null)
                            settings.Loot.Pickup = null;
                        break;
                    case SettingsSection.TownRun:
                        if (settings.Loot != null)
                            settings.Loot.TownRun = null;
                        break;
                    case SettingsSection.Objects:
                        settings.WorldObject = null;
                        break;
                    case SettingsSection.Paragon:
                        settings.Paragon = null;
                        break;
                    case SettingsSection.Advanced:
                        settings.Advanced = null;
                        break;
                    case SettingsSection.Avoidance:
                        settings.Avoidance = null;
                        break;
                    case SettingsSection.ItemRules:
                        if (settings.Loot != null)
                            settings.Loot.ItemRules = null;
                        break;
                }
            }
        }

        /// <summary>
        /// Make checkboxes on the selections dialog enabled and clickable. (they default to disabled).
        /// </summary>
        private static void EnableSections(ICollection<SettingsSection> validSections, SettingsSelectionViewModel selectionViewModel)
        {
            foreach (var item in selectionViewModel.Selections)
            {
                if (validSections.Contains(item.Section))
                {
                    item.IsEnabled = true;
                    item.IsSelected = true;
                }
            }
        }

    }
}