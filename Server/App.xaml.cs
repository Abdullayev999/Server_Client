using CommonLib.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Server.ViewModels;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Server
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Container Services { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            RegisterServices();
            Start<HomeViewModel>(); ;
        }

        private void Start<T>() where T : ViewModelBase
        {
            var windowViewModel = Services.GetInstance<MainViewModel>();
            windowViewModel.CurrentViewModel = Services.GetInstance<T>();

            var mainWindow = new MainWindow { DataContext = windowViewModel };
            mainWindow.ShowDialog();
        }
        public void RegisterServices()
        {
            Services = new Container();
            Services.RegisterSingleton<IMessenger, Messenger>();
            Services.RegisterSingleton<IUserRepository, UserRepository>();
            Services.RegisterSingleton<MainViewModel>();
            Services.RegisterSingleton<HomeViewModel>();

            Services.Verify();
        }
    }
}
