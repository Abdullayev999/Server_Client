using CommonLib.Models;
using CommonLib.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Server.ViewModels
{ 
    public class HomeViewModel : ViewModelBase 
    {
        TcpListener tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 58002);
        public Visibility VisSetting { get; set; } = Visibility.Visible;
        public Visibility VisChatCompyuter { get; set; } = Visibility.Hidden; 

        public void Mainn()
        {
            try
            {
                tcpListener.Start();
                Console.WriteLine("Server lstener start...");

                while (true)
                {
                    TcpClient client = tcpListener.AcceptTcpClient();
                    Console.WriteLine($"{client.Client.RemoteEndPoint} connected!");

                    Task.Run(() =>
                    {
                        NetworkStream stream = client.GetStream();
                        BinaryReader reader = new BinaryReader(stream);

                        while (true)
                        {
                            try
                            {
                                string filename = reader.ReadString();
                                int len = reader.ReadInt32();
                                byte[] bytes = reader.ReadBytes(len);

                                File.WriteAllBytes(filename, bytes);
                                Chat=$"File {filename} of {len} bytes received from {client.Client.RemoteEndPoint}\n";
                            }
                            catch (EndOfStreamException ex)
                            {
                                Chat="Message: " + ex.Message + "\n" + client.Client.RemoteEndPoint + " disconnected\n";
                                break;
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Chat = ex.Message +"\n";
            }
            finally
            {
                if (tcpListener != null)
                {
                    tcpListener.Stop();
                    Chat = "Server listenner is stop\n";
                }
            }
        }
        public string Chat { get; set; } 
          
        public IPAddress ServerIp { get; set; }
        public int ServerPort { get; set; }
        public IPEndPoint ServerEndPoint { get; set; }
        public Socket ServerSocket { get; set; }
        public Dictionary<string, Socket> Users { get; set; } = new Dictionary<string, Socket>();
        public HomeViewModel(IUserRepository userRepository)
        {
            ServerIp = IPAddress.Parse("127.0.0.1");
            ServerPort = 58001;
            ServerEndPoint = new IPEndPoint(serverIp, serverPort);
            ServerSocket = new Socket(SocketType.Stream, ProtocolType.IP);
            UsersByCountReceiver = new List<Socket>(); 
            Chat = "";
            this.userRepository = userRepository;
        }
        private RelayCommand connectCommand = null;
        public int Count { get; set; }
        static IPAddress serverIp = IPAddress.Parse("127.0.0.1");
        static int serverPort = 58001;
        private readonly IUserRepository userRepository;

        public List<Socket> UsersByCountReceiver { get; set; }
        public RelayCommand ConnectCommand => connectCommand = new RelayCommand(
        () =>
        {
            VisSetting = Visibility.Hidden;
            VisChatCompyuter = Visibility.Visible;


            ServerSocket.Bind(ServerEndPoint);
            ServerSocket.Listen(10);
            Chat += "Server was started!\n\n";
            Task.Run(() =>
            {
                Mainn();
            });
            Task.Run(() => {
                do
                {
                    Socket clientSocket = ServerSocket.Accept(); 
                    clientSocket.Send(Encoding.Unicode.GetBytes("Welcome to the general chat!"));
                    Chat += $"{clientSocket.RemoteEndPoint} connected\n"; 
                    UsersByCountReceiver.Add(clientSocket);


                    Task.Run(() =>
                    {
                        byte[] buffer = new byte[1000];

                        while (true)
                        {
                            try
                            {

                                int len = clientSocket.Receive(buffer);
                                string msg = Encoding.Unicode.GetString(buffer, 0, len); 
                                  
                                if (msg.StartsWith(MessagesType.Disconnect.ToString()))
                                {
                                    clientSocket.Close();
                                    break;
                                }

                                if (msg.StartsWith(MessagesType.All.ToString()))
                                {
                                    msg = msg.Remove(0, MessagesType.All.ToString().Length+1); 
                                    var msgArr = msg.Split(' ');
                                    int index = Convert.ToInt32(msgArr[0]);
                                    var messages = msg.Remove(0, msgArr[0].Length);
                                    userRepository.AddAllChat(index, messages); 
                                }
                                else if (msg.StartsWith(MessagesType.Inbox.ToString()))
                                {
                                    msg = msg.Remove(0, MessagesType.Inbox.ToString().Length + 1);
                                    var msgArr = msg.Split(' ');
                                    int indexSend = Convert.ToInt32(msgArr[1]);
                                    int indexReceive = Convert.ToInt32(msgArr[0]);
                                    var messages = msg.Remove(0, msgArr[0].Length+ msgArr[1].Length);
                                    userRepository.AddInbox(indexSend, indexReceive, messages);


                                } 
                                else if (msg.StartsWith(MessagesType.Group.ToString()))
                                {
                                    msg = msg.Remove(0, MessagesType.Group.ToString().Length + 1);
                                    var msgArr = msg.Split(' ');
                                    int indexGroup = Convert.ToInt32(msgArr[0]);
                                    int indexUser = Convert.ToInt32(msgArr[1]);
                                    var messages = msg.Remove(0, msgArr[0].Length + msgArr[1].Length);
                                    userRepository.AddGroupMsg(indexGroup, indexUser, messages);  
                                }
                                 
                                clientSocket.Send(Encoding.Unicode.GetBytes("refresh"));
                                
                            }
                            catch (Exception ex)
                            { 
                             Chat += ex.Message + "____" + clientSocket.RemoteEndPoint + "  disconnected\n";
                                break;
                            }
                        }
                        clientSocket.Shutdown(SocketShutdown.Both);
                        clientSocket.Close();
                    });
                } while (true);
            }); 
        });
    }
}
