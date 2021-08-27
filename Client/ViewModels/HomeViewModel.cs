using CommonLib.Messages;
using CommonLib.Models;
using CommonLib.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Client.ViewModels
{
    public class HomeViewModel : ViewModelBase 
    {

        private TcpClient client;
        private BinaryWriter writer;
        public MessagesType MsgType { get; set; }
        public IPAddress ServerIp { get; set; }
        public int ServerPort { get; set; }
        public Socket ClientSocket { get; set; } = new Socket(SocketType.Stream, ProtocolType.IP); 
        public Visibility VisMain { get; set; } = Visibility.Hidden;
        public Visibility VisSetting { get; set; } = Visibility.Visible;
        public User User { get; set; } = new User();
        public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();
        public ObservableCollection<Group> Groups { get; set; } = new ObservableCollection<Group>();
        public ObservableCollection<string> Chat { get; set; } = new ObservableCollection<string>();
        public DispatcherTimer timer { get; set; }
        public string Messages { get; set; } = "";

        private readonly IMessenger messenger;
        private readonly IUserRepository userRepository;
        public HomeViewModel(IMessenger messenger, IUserRepository userRepository)
        {
            messenger.Register<SendUser>(this, message => {  this.User = message.User; }); 
            this.messenger = messenger;
            this.userRepository = userRepository;


            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 3);
            timer.Tick += Timer_Tick; 
        }

        private RelayCommand logOutCommand = null;

        public RelayCommand LogOutCommand => logOutCommand ??= new RelayCommand(
        () =>
        {
            messenger.Send(new NavigationMessage { ViewModelType = typeof(LoginViewModel) });
        });
        public string PathFile { get; set; }
        private RelayCommand addFileCommand = null;

        public RelayCommand AddFileCommand => addFileCommand ??= new RelayCommand(
        () =>
        {
            OpenFileDialog dialog = new OpenFileDialog() { InitialDirectory = Environment.CurrentDirectory };

            if (dialog.ShowDialog().Value)
            {
                PathFile = dialog.FileName;

                byte[] bytes = File.ReadAllBytes(PathFile);
                int len = bytes.Length;
                string fileName = new FileInfo(PathFile).Name;

                writer.Write(fileName);
                writer.Write(len);
                writer.Write(bytes);
                writer.Flush();

                if (ClientSocket != null && ClientSocket.Connected)
                {
                    if (SelectGroup != null)
                    {
                        MsgType = MessagesType.Group;
                        ClientSocket.Send(Encoding.Unicode.GetBytes($"{MsgType} {SelectGroup.Id} {User.Id} ({DateTime.Now.ToShortTimeString()}) Send File"));

                    }
                    else if (SelectUser != null)
                    {
                        MsgType = MessagesType.Inbox;
                        ClientSocket.Send(Encoding.Unicode.GetBytes($"{MsgType} {SelectUser.Id} {User.Id} ({DateTime.Now.ToShortTimeString()}) Send File"));
                    }
                    else
                    {
                        MsgType = MessagesType.All;
                        ClientSocket.Send(Encoding.Unicode.GetBytes($"{MsgType} {User.Id} ({DateTime.Now.ToShortTimeString()}) Send File"));
                    }


                }
            }
        });
 

        private RelayCommand addGroupCommand = null;

        public RelayCommand AddGroupCommand => addGroupCommand ??= new RelayCommand(
        () =>
        {
            App.Services.GetInstance<AddGroupViewModel>().Refresh();
            messenger.Send(new NavigationMessage { ViewModelType = typeof(AddGroupViewModel) }); 
        });
        public int CountGroup { get; set; }
        public int CountUsers { get; set; }
        public void Refresh()
        {
            if (SelectGroup != null)
            {
                App.Current.Dispatcher.Invoke(() => {
                    Chat = new ObservableCollection<string>(userRepository.GetGroupChat(selectGroup.Id));
                });

            }
            else if (SelectUser != null)
            {
                App.Current.Dispatcher.Invoke(() => {
                    Chat = new ObservableCollection<string>(userRepository.GetInbox(User.Id, SelectUser.Id));
                });
            }
            else
            {
                App.Current.Dispatcher.Invoke(() => {
                    Chat = new ObservableCollection<string>(userRepository.GetAllChatByUserDate(User.Date));//????
                });
            }

            var newCountGroup = userRepository.GetGroups().Count;
            var newCountUsers = userRepository.GetUsers().Count;

            if (CountGroup< newCountGroup)
            {
                CountGroup = newCountGroup;
                RefreshGroupList();
            }

            if (CountUsers < newCountUsers)
            {
                CountUsers = newCountUsers;
                RefreshGroupList();
            }

        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            Refresh();
        }

        private RelayCommand allChatCommand = null; 
        public RelayCommand AllChatCommand => allChatCommand ??= new RelayCommand(
        () =>
        { 
             SelectGroup = null;
             SelectUser = null;
             App.Current.Dispatcher.Invoke(() => {
                 Chat = new ObservableCollection<string>(userRepository.GetAllChatByUserDate(User.Date));//????
             }); 
        });


        private User selectUser;

        public User SelectUser
        {
            get { return selectUser; }
            set { selectUser = value;
                if (SelectUser!=null)
                {
                    SelectGroup = null; 
                    App.Current.Dispatcher.Invoke(() => { 
                        Chat = new ObservableCollection<string>(userRepository.GetInbox(User.Id, SelectUser.Id));
                    });  
                } 
            }
        }

        private Group selectGroup;

        public Group SelectGroup
        {
            get { return selectGroup; }
            set { selectGroup = value;
                if (SelectGroup != null) { 
                    SelectUser = null; 
                    App.Current.Dispatcher.Invoke(() => {
                        Chat = new ObservableCollection<string>(userRepository.GetGroupChat(selectGroup.Id));
                    });
                }
            }
        }

        public void RefreshGroupList()
        {
            App.Current.Dispatcher.Invoke(() => {
                Groups.Clear();
                var lsit = userRepository.GetGroupsIdByUser(User.Id); 
                foreach (var groupId in lsit)
                {
                    var groups = userRepository.GetGroupsById(groupId); 
                    foreach (var item in groups)
                    {
                        Groups.Add(item); 
                    } 
                } 
            });
        } 
        private RelayCommand sendCommand = null;

        public RelayCommand SendCommand => sendCommand ??= new RelayCommand(
        () =>
        {
            if (ClientSocket != null && ClientSocket.Connected)
            { 
                if (SelectGroup!=null)
                {
                    MsgType = MessagesType.Group;
                    ClientSocket.Send(Encoding.Unicode.GetBytes($"{MsgType} {SelectGroup.Id} {User.Id} ({DateTime.Now.ToShortTimeString()}) {Messages}"));
                     
                }
                else if (SelectUser!=null)
                { 
                    MsgType = MessagesType.Inbox;
                    ClientSocket.Send(Encoding.Unicode.GetBytes($"{MsgType} {SelectUser.Id} {User.Id} ({DateTime.Now.ToShortTimeString()}) {Messages}"));
                }
                else  
                {
                    MsgType = MessagesType.All;
                    ClientSocket.Send(Encoding.Unicode.GetBytes($"{MsgType} {User.Id} ({DateTime.Now.ToShortTimeString()}) {Messages}"));
                }

                Messages = string.Empty;
            }
        });
         

        private RelayCommand connectCommand;
        public RelayCommand ConnectCommand => connectCommand = new RelayCommand(
        () =>
        {
            try
            {
                Task.Run(() =>
                {
                    client = new TcpClient();
                    client.Client.Connect(IPAddress.Parse("127.0.0.1"), 58002);

                    NetworkStream stream = client.GetStream();
                    writer = new BinaryWriter(stream);
                });

                CountGroup = userRepository.GetGroups().Count;
                CountUsers = userRepository.GetUsers().Count;
                App.Current.Dispatcher.Invoke(() => {
                    var users = new ObservableCollection<User>(userRepository.GetUsers());
                    Users = new ObservableCollection<User>(users.Where(user => user.Id != User.Id));
                });
                RefreshGroupList(); 
                Chat = new ObservableCollection<string>(userRepository.GetAllChatByUserDate(User.Date)); 
                ServerPort = 58001;
                ServerIp = IPAddress.Parse("127.0.0.1");
                ClientSocket.Connect(new IPEndPoint(ServerIp, ServerPort));
                MsgType = MessagesType.Connect;
                ClientSocket.Send(Encoding.Unicode.GetBytes($"{MsgType} {User.Id} ({DateTime.Now.ToShortTimeString()}) Connected"));
                ReceiverAsync(); 
                VisSetting = Visibility.Hidden;
                VisMain = Visibility.Visible;

                timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,$"Error",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        });
         

        private async void ReceiverAsync()
        {
            await Task.Run(() =>
            {
                byte[] buffer = new byte[1000];
                do
                {
                    try
                    {
                        string msg = Encoding.Unicode.GetString(buffer, 0, ClientSocket.Receive(buffer));
                        if (msg.Equals("The server closed the connection"))
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                Chat.Add("you disconnected");
                            });


                            ClientSocket.Close();
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, $"Info  + {ex.TargetSite} + {ex.StackTrace}", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    }
                } while (true);
            });
        }
        
    }
}
