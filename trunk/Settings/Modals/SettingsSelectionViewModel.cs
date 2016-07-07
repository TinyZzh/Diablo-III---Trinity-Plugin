using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Adventurer.Util;
using Trinity.Helpers;
using Trinity.UIComponents;

namespace Trinity.Settings
{
    public class SettingsSelectionViewModel : NotifyBase
    {
        private bool _isWindowOpen;
        private List<SettingsSelectionItem> _selections;
        private string _title;
        private string _description;
        private DialogResult _dialogResult;
        private int _windowWidth;
        private int _windowHeight;
        private string _okButtonText;

        public SettingsSelectionViewModel()
        {
            Selections = GetDefaultSelections();
        }

        public static List<SettingsSelectionItem> GetDefaultSelections()
        {
            return GetAllSections().Select(item => new SettingsSelectionItem(item)).ToList();
        }

        public static List<SettingsSection> GetAllSections()
        {
            return default(SettingsSection).ToList<SettingsSection>(true);
        }
    
        public bool IsWindowOpen
        {
            get { return _isWindowOpen; }
            set { SetField(ref _isWindowOpen, value); }
        }

        public FullyObservableCollection<SettingsSelectionItem> Items
        {
            get { return new FullyObservableCollection<SettingsSelectionItem>(Selections); }
            set { Selections = value.ToList(); }
        }

        public List<SettingsSelectionItem> Selections
        {
            get { return _selections; }
            set { SetField(ref _selections, value); }
        }

        public string Title
        {
            get { return _title; }
            set { SetField(ref _title, value); }
        }

        public string Description
        {
            get { return _description; }
            set { SetField(ref _description, value); }
        }

        public string OkButtonText
        {
            get { return _okButtonText; }
            set { SetField(ref _okButtonText, value); }
        }

        public DialogResult DialogResult
        {
            get { return _dialogResult; }
            set { SetField(ref _dialogResult, value); }
        }

        public ICommand OkCommand => new RelayCommand(param =>
        {
            DialogResult = DialogResult.OK;
            IsWindowOpen = false;
        });

        public ICommand CancelCommand => new RelayCommand(param =>
        {
            DialogResult = DialogResult.Cancel;
            IsWindowOpen = false;
        });

        public ICommand SelectAllCommand => new RelayCommand(param =>
        {

            Logger.Info("SelectAllCommand fired");

            foreach (var item in Selections)
            {
                if (item.IsEnabled)
                    item.IsSelected = true;
            }
        });

        public ICommand SelectNoneCommand => new RelayCommand(param =>
        {
            Logger.Info("SelectNoneCommand fired");

            foreach (var item in Selections)
            {
                if (item.IsEnabled)
                    item.IsSelected = false;
            }
        });
    }
}