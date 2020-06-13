using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using WebformTemplate.Utilities;

namespace WebformTemplate.ViewModel
{
    public class MainTabControlViewModel : ViewModelBase
    {
        private ViewModelBase selectedViewModel;
        public ViewModelBase SelectedViewModel { get => selectedViewModel; set { selectedViewModel = value; RaisePropertyChanged(); } }

        private int selectedTab;
        public int SelectedTab { get => selectedTab; set { selectedTab = value; RaisePropertyChanged(); } }
        public MainTabControlViewModel()
        {
            try
            {
                Tabs = new ObservableCollection<ViewModelBase>();
                EventAggregator.OnCommandTransmitted += OnCommandReceived;
                EventAggregator.OnMessageTransmitted += OnMessageReceived;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }

        }

        private void OnMessageReceived(object obj)
        {
            try
            {
                if (obj == null)
                    return;

                if (!(obj is ViewModelBase))
                    return;

                ViewModelBase vmb = obj as ViewModelBase;

                Tabs.Add(vmb);
                SelectedTab = Tabs.IndexOf(vmb);
                SelectedViewModel = vmb;

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }


        }

        private void OnCommandReceived(BroadcastCommand cmd, ViewModelBase v)
        {
            if (cmd == BroadcastCommand.CloseTab)
            {
                try
                {
                    Tabs.Remove(v);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Close Tab " + ex.ToString());
                }

            }
        }

        public override RelayCommand SaveEditCommand { get { return new RelayCommand(SaveEdit); } }
        public RelayCommand RefreshEditCommand { get { return new RelayCommand(RefreshEdit); } }

        private void SaveEdit(object obj)
        {
            try
            {
                if (SelectedViewModel != null)
                {
                    SelectedViewModel.SaveEditCommand.Execute(null);
                }
                string str = "";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }

        }

        private void RefreshEdit(object obj)
        {
            try
            {
                if (SelectedViewModel != null)
                {
                    SelectedViewModel.RefreshEditCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }


        }
    }
}
