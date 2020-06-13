using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using WebformTemplate.Model;
using WebformTemplate.Utilities;

namespace WebformTemplate.ViewModel
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(caller)); }
        }
        public ViewModelBase()
        {

        }
        private bool loading, saving;
        public bool Loading { get => loading; set { loading = value; RaisePropertyChanged(); } }
        public bool Saving { get => saving; set { saving = value; RaisePropertyChanged(); } }

        private ObservableCollection<MenuOption> menuOptions;
        public ObservableCollection<MenuOption> MenuOptions { get => menuOptions; set { menuOptions = value; RaisePropertyChanged(); } }

        private ObservableCollection<ViewModelBase> tabs;
        public ObservableCollection<ViewModelBase> Tabs { get => tabs; set { tabs = value; RaisePropertyChanged(); } }

        private string tabHeader;
        public string TabHeader { get => tabHeader; set { tabHeader = value; RaisePropertyChanged(); } }

        public static User CurrentUser { get; set; }

        public abstract RelayCommand SaveEditCommand { get; }
        public RelayCommand RefreshEditCommand;

        private ViewModelBase currentView;
        public ViewModelBase CurrentView { get => currentView; set { currentView = value; RaisePropertyChanged(); } }

        public RelayCommand ChangeViewCommand { get { return new RelayCommand(ChangeView); } }
        public RelayCommand AddTabCommand { get { return new RelayCommand(AddTab); } }

        private void ChangeView(object obj)
        {
            try
            {
                if (obj is string)
                {
                    MenuOption m = MenuOptions.First(x => x.CommandParameter.Equals(obj.ToString()));

                    if (!string.IsNullOrEmpty(m.ViewModelName))
                    {
                        var v = m.GetObject<ViewModelBase>();
                        //if (v.GetType() != CurrentView?.GetType())


                        if (CurrentView == null || v.GetType() != CurrentView.GetType())
                        {
                            CurrentView = v;

                        }

                        //if (CurrentView is LeftMenuViewModel && v is LeftMenuViewModel)
                        //{
                        //    CurrentView = v;
                        //}

                    }
                }
                else
                {
                    MenuOption m = obj as MenuOption;
                    CurrentView = m.GetObject<ViewModelBase>(m.ObjParameter);
                }


            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }

        private void AddTab(object obj)
        {
            try
            {
                if (obj is string)
                {
                    MenuOption m = MenuOptions.First(x => x.CommandParameter.Equals(obj.ToString()));

                    if (!string.IsNullOrEmpty(m.ViewModelName))
                    {
                        var v = m.GetObject<ViewModelBase>();
                        EventAggregator.BroadCast(v);                        
                    }
                }
                else
                {
                    MenuOption m = obj as MenuOption;
                    var v = m.GetObject<ViewModelBase>(m.ObjParameter);
                    CurrentView.Tabs.Add(v);
                }


            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }

        public async void LoadStaticCollections()
        {
            try
            {
                //Stores = await Store.StoredProc.CollectData<Store>();
            }
            catch (Exception ex)
            {
                //System.Windows.MessageBox.Show(ex.ToString());
            }

        }
    }

    static class ViewModelBaseExtras
    {
        public static ViewModelBase GetObject<ViewModelBase>(this MenuOption m, object par = null)
        {
            try
            {
                Type type = Type.GetType(m.ViewModelName);
                ViewModelBase obj = default(ViewModelBase);
                if (par != null)
                {
                    if (m.Option.Equals(MenuOptionAttr.UNDEFINED))
                        obj = (ViewModelBase)Activator.CreateInstance(type, par);
                    else
                        obj = (ViewModelBase)Activator.CreateInstance(type, m.Option, par);
                }
                else
                {
                    if (m.Option.Equals(MenuOptionAttr.UNDEFINED))
                        obj = (ViewModelBase)Activator.CreateInstance(type, true);
                    else
                        obj = (ViewModelBase)Activator.CreateInstance(type, m.Option);
                }

                return (ViewModelBase)obj;
            }
            catch (Exception ex)
            {
                //System.Windows.MessageBox.Show(ex.ToString());
                throw ex;
            }

        }
    }
}
