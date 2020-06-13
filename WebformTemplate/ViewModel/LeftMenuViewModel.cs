using System.Collections.ObjectModel;
using WebformTemplate.Model;
using WebformTemplate.Utilities;

namespace WebformTemplate.ViewModel
{
    public class LeftMenuViewModel : ViewModelBase
    {
        public LeftMenuViewModel(MenuOptionAttr menu)
        {
            try
            {
                switch (menu)
                {
                    case MenuOptionAttr.STANDARD:
                        MenuOptions = StandardMenu();
                        CurrentView = new MainTabControlViewModel();
                        break;

                    default:
                        break;
                }
                if (MenuOptions != null && MenuOptions.Count == 1) MenuOptions[0].Command.Execute(MenuOptions[0].CommandParameter);
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }

        }

        public override RelayCommand SaveEditCommand { get { return null; } }

        private ObservableCollection<MenuOption> StandardMenu()
        {
            return new ObservableCollection<MenuOption>()
            {
                new MenuOption("Enter Cash Count", AddTabCommand, MenuOptionType.EnterCashCount, CurrentUser.CanLoad,"Tasks"),                
                new MenuOption("Search Cashier Variances", AddTabCommand, MenuOptionType.SearchCashierVariance, CurrentUser.CanLoad, "Search"),
               
            };
        }
    }
}
