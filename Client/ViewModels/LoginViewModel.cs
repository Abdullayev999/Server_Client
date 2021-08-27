using CommonLib.Messages;
using CommonLib.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Client.ViewModels
{
    public class LoginViewModel:ViewModelBase
    {
        private IMessenger messanger;
        private IUserRepository userRepository;

        private string login;
        private string password;

        public string Login
        {
            get { return login; }
            set { login = value; Check(); }
        }


        public string Password
        {
            get { return password; }
            set { password = value; Check(); }
        }

        public Visibility Visibility { get; set; }
        public bool IsEnable { get; set; }
        public LoginViewModel(IMessenger messanger, IUserRepository userRepository)
        {
            this.messanger = messanger;
            this.userRepository = userRepository;
        }

        private RelayCommand loginCommand = null;

        public RelayCommand LoginCommand => loginCommand ??= new RelayCommand(
        () =>
        {
            UnicodeEncoding unicodeEncoding = new UnicodeEncoding();
            var sha256 = SHA256.Create();
            byte[] secret = sha256.ComputeHash(unicodeEncoding.GetBytes(Password));
            Password = Convert.ToBase64String(secret);

            var user = userRepository.GetUserByLoginAndPassword(Login, Password);

            if (user != null)
            {

                messanger.Send(new SendUser()  { User = user  });
                messanger.Send(new NavigationMessage() { ViewModelType = typeof(HomeViewModel) });
                Clear();
            }
            else
            {
                MessageBox.Show("Incorrect Login or Password!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        });

        private RelayCommand registrationCommand = null;

        public RelayCommand RegistrationCommand => registrationCommand ??= new RelayCommand(
        () =>
        {
            Clear();
            messanger.Send(new NavigationMessage { ViewModelType = typeof(RegistrationViewModel) });
        });

        private RelayCommand exitCommand = null;

        public RelayCommand ExitCommand => exitCommand ??= new RelayCommand(
        () =>
        {
            Environment.Exit(0);
        });
         

        void Clear()
        {
            Password = string.Empty;
            Login = string.Empty;
        }

        void Check()
        {
            bool isCorrect = true;

            if (string.IsNullOrWhiteSpace(Password))
            {
                isCorrect = false;
                Visibility = Visibility.Visible;
            }

            if (string.IsNullOrWhiteSpace(Login))
            {
                isCorrect = false;
                Visibility = Visibility.Visible;
            }

            if (isCorrect)
                Visibility = Visibility.Collapsed;

            IsEnable = isCorrect;
        }

    }
}
