using CommonLib.Messages;
using CommonLib.Models;
using CommonLib.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace Client.ViewModels
{
    class AddGroupViewModel  : ViewModelBase
    {   
        private readonly IMessenger messenger;
        private readonly IUserRepository userRepository; 
        private string name;
        public bool IsCreate { get; set; }
        public string Name
        {
            get { return name; }
            set { name = value;
                Check();
            }
        }

        public void Check()
        {
            IsCreate = true;
            if (string.IsNullOrWhiteSpace(name))IsCreate = false;
             
            if (AddedUsers.Count<=1)
            {
                IsCreate = false;
            }
        }
        public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();
        public ObservableCollection<User> AddedUsers { get; set; } = new ObservableCollection<User>();
        public User User { get; set; }
        public AddGroupViewModel(IMessenger messenger,IUserRepository userRepository)
        {
            messenger.Register<SendUser>(this, message => { this.User = message.User; });
            this.messenger = messenger;
            this.userRepository = userRepository;

             
        }
         
        private RelayCommand<User> addUsers = null;

        public RelayCommand<User> AddUsers => addUsers ??= new RelayCommand<User>(
        (user) =>
        { 
            if (user != null)
            { 
                if (!AddedUsers.Contains(user))
                {
                    AddedUsers.Add(user);
                    Check();
                }
                else MessageBox.Show("This users exists","Wrong",MessageBoxButton.OK,MessageBoxImage.Information);
                
            }
            else MessageBox.Show("You not select", "Wrong", MessageBoxButton.OK, MessageBoxImage.Information);
            
        });

        private RelayCommand<User> removeUsers = null;

        public RelayCommand<User> RemoveUsers => removeUsers ??= new RelayCommand<User>(
        (user) =>
        { 
            if (user != null)
            {
                AddedUsers.Remove(user);
                Check();
            }
        });

        private RelayCommand cancelCommand = null;

        public RelayCommand CancelCommand => cancelCommand ??= new RelayCommand(
        () =>  
        {
            messenger.Send(new NavigationMessage { ViewModelType = typeof(HomeViewModel) });

            Clear();
        });

        public void Clear()
        {
            AddedUsers.Clear();
            Name = string.Empty;
            IsCreate = false;
            Users.Clear();
        }


        private RelayCommand createCommand = null;

        public RelayCommand CreateCommand => createCommand ??= new RelayCommand(
        () =>
        {
            var group = userRepository.GetGroupByName(Name);
            if (group==null)
            {
                AddedUsers.Add(User);
                userRepository.AddGroup(Name);
                group = userRepository.GetGroupByName(Name);
                userRepository.AddUserInGroup(AddedUsers.ToList(), group.Id);

                StringBuilder stringBuilder = new StringBuilder(100);
                foreach (var item in AddedUsers)
                {
                    stringBuilder.Append(item.Username + "  ");
                }
                userRepository.AddGroupMsg(group.Id, User.Id, $"Group {group.Name} created by {User.Username} in {DateTime.Now}\nWith users : {stringBuilder}");

                 MessageBox.Show("Group created","Information",MessageBoxButton.OK,MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Group not created", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            App.Services.GetInstance<HomeViewModel>().RefreshGroupList();
            messenger.Send(new NavigationMessage { ViewModelType = typeof(HomeViewModel) });


            Clear();

        });
          
        public void Refresh()
        {
            App.Current.Dispatcher.Invoke(() => {
                var users = new ObservableCollection<User>(userRepository.GetUsers());
                Users = new ObservableCollection<User>(users.Where(user => user.Id != User.Id));
            });
        }
        
    }
}
