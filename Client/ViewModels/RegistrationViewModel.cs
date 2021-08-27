using CommonLib.Messages;
using CommonLib.Models;
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
    public class RegistrationViewModel : ViewModelBase
    {
        private IMessenger messanger;
        private IUserRepository userRepository;

        private RelayCommand registrationCommand = null;
        private RelayCommand backCommand = null;

        private string password;
        private string forwardPassword;
        private string login;
        private string username;
        public bool IsEnable { get; set; }  
         
        public Visibility VisPassword { get; set; }
        public Visibility VisLogin { get; set; }
        public Visibility VisUsername { get; set; }

        public string Password
        {
            get => password;
            set { password = value; Check(); }
        }
        public string ForwardPassword
        {
            get => forwardPassword;
            set { forwardPassword = value; Check(); }
        }

        public string Login
        {
            get => login;
            set { login = value; Check(); }
        }
        public string Username
        {
            get => username;
            set { username = value; Check(); }
        }

        public RegistrationViewModel(IMessenger messanger, IUserRepository userRepository)
        {
            this.messanger = messanger;
            this.userRepository = userRepository;
            Clear();
        }

        public RelayCommand RegistrationCommand => registrationCommand ??= new RelayCommand(
        () =>
        {
            if (Password != null && ForwardPassword != null && Password.Equals(ForwardPassword))
            {
                try
                { 
                    UnicodeEncoding unicodeEncoding = new UnicodeEncoding();
                    var sha256 = SHA256.Create();
                    byte[] secret = sha256.ComputeHash(unicodeEncoding.GetBytes(Password));
                    string cryptedPassword = Convert.ToBase64String(secret);



                    var user = new User() {  Login = Login,  Password = cryptedPassword,Username = Username }; 

                    userRepository.AddUser(user);
                    messanger.Send(new NavigationMessage() { ViewModelType = typeof(LoginViewModel) });
                    Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else MessageBox.Show("Passwords not equel!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        });

        void Check()
        {
            var correct = true;
            if (string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ForwardPassword) || !Password.Equals(ForwardPassword))
            {
                VisPassword = Visibility.Visible;
                correct = false;
            }
            else VisPassword = Visibility.Collapsed;
             
            if (string.IsNullOrWhiteSpace(Login))
            {
                VisLogin = Visibility.Visible;
                correct = false;
            }
            else VisLogin = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(Username))
            {
                VisUsername = Visibility.Visible;
                correct = false;
            }
            else VisUsername = Visibility.Collapsed;

            IsEnable = correct;
        }
        void Clear()
        { 
            Login = string.Empty; 
            IsEnable = false;
            Password = string.Empty;
            ForwardPassword = string.Empty; 
            Username = string.Empty;
        }



        public RelayCommand BackCommand => backCommand ??= new RelayCommand(
        () =>
        {
            Clear();
            messanger.Send(new NavigationMessage() { ViewModelType = typeof(LoginViewModel) });
        });
    }
}
