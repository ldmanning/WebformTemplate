using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebformTemplate.Utilities;
using WebformTemplate.ViewModel;

namespace WebformTemplate.Model
{
    public class MenuOption : ModelBase
    {
        private string label;
        private RelayCommand command;
        private string commandParameter;
        private ViewModelBase currentView;
        private bool isActive;
        private bool isEnabled;
        private string menuGroup;
        private string viewModelName;
        MenuOptionAttr option;
        private object objParameter;

        public MenuOption(string label)
        {

        }

        public MenuOption(string label, RelayCommand command, string viewModelName, MenuOptionAttr option, bool isEnabled, string menuGroup = null)
        {
            Label = label;
            Command = command;
            CommandParameter = label.Trim();
            ViewModelName = viewModelName;
            MenuGroup = menuGroup;
            IsEnabled = isEnabled;
            Option = option;
        }

        public MenuOption(string label, RelayCommand command, string viewModelName, bool isEnabled, string menuGroup = null)
        {
            Label = label;
            Command = command;
            CommandParameter = label.Trim();
            ViewModelName = viewModelName;
            MenuGroup = menuGroup;
            IsEnabled = isEnabled;
        }

        public MenuOption(string label, RelayCommand command, string viewModelName, bool isEnabled, string menuGroup = null, object obj = null)
        {
            Label = label;
            Command = command;
            CommandParameter = label.Trim();
            ViewModelName = viewModelName;
            MenuGroup = menuGroup;
            IsEnabled = isEnabled;
            ObjParameter = obj;
        }

        public MenuOption(string label, RelayCommand command, string viewModelName, MenuOptionAttr option, bool isEnabled, string menuGroup, object commandParameter = null)
        {
            Label = label;
            Command = command;
            CommandParameter = label.Trim();
            ViewModelName = viewModelName;
            MenuGroup = menuGroup;
            IsEnabled = isEnabled;
            Option = option;
            ObjParameter = commandParameter;
        }


        public string Label { get => label; set { label = value; RaisePropertyChanged(); } }
        public RelayCommand Command { get => command; set { command = value; RaisePropertyChanged(); } }
        public string CommandParameter { get => commandParameter; set { commandParameter = value; RaisePropertyChanged(); } }
        public ViewModelBase CurrentView { get => currentView; set { currentView = value; RaisePropertyChanged(); } }
        public bool IsActive { get => isActive; set { isActive = value; RaisePropertyChanged(); } }
        public bool IsEnabled { get => isEnabled; set { isEnabled = value; RaisePropertyChanged(); } }
        public string MenuGroup { get => menuGroup; set { menuGroup = value; RaisePropertyChanged(); } }
        public string ViewModelName { get => viewModelName; set { viewModelName = value; RaisePropertyChanged(); } }
        public MenuOptionAttr Option { get => option; set { option = value; RaisePropertyChanged(); } }
        public object ObjParameter { get => objParameter; set { objParameter = value; RaisePropertyChanged(); } }
    }

    public static class MenuOptionType
    {
        public const string EnterCashCount = "CashControl.ViewModel.EnterCashCountViewModel, CashControl";        
        public const string SearchCashierVariance = "CashControl.ViewModel.SearchCashierVarianceViewModel, CashControl";        
    }

    public enum MenuOptionAttr
    {
        UNDEFINED,
        STANDARD
    }
}
