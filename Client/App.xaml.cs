using Client.ViewModels;
using CommonLib.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Client
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
              Start<LoginViewModel>(); ;
            //Start<HomeViewModel>(); 
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
            Services.RegisterSingleton<LoginViewModel>();
            Services.RegisterSingleton<RegistrationViewModel>();
            Services.RegisterSingleton<AddGroupViewModel>();

            Services.Verify();
        }
    }
}
