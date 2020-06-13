using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebformTemplate.Utilities;
using WebformTemplate.Model;

namespace WebformTemplate.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            try
            {
                CurrentUser = new User("");                

                CurrentView = new LeftMenuViewModel(MenuOptionAttr.STANDARD);

                try
                {
                    Task.Run(() => LoadStaticCollections());

                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.ToString());
                }

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }

        public override RelayCommand SaveEditCommand { get { return null; } }
    }
}
