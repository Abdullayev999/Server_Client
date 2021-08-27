using CommonLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib.Services
{
    public interface IUserRepository
    {
        void AddUser(User user);
        User GetUserByLoginAndPassword(string login, string password);
        List<string> GetGroupChat(int groupId);
        void AddAllChat(int idUser, string msg);
        void AddInbox(int idUserSend, int idUserReceive, string msg);
        void AddGroupMsg(int groupId, int userSendId, string msg);
        void AddGroup(string name);
        List<User> GetUsers();
        List<Group> GetGroups();
        List<Group> GetGroupsById(int id);
        List<int> GetGroupsIdByUser(int Id);
        void DeleteUserById(int id);
        List<string> GetAllChat();
        List<string> GetAllChatByUserDate(DateTime date);
        Group GetGroupByName(string name);
        void AddUserInGroup(List<User> users, int groupId);
        List<string> GetInbox(int firstUserid, int secondUserid);
    }
}
