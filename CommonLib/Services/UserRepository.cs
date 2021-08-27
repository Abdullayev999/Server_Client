using CommonLib.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib.Services
{
    public class UserRepository : IUserRepository
    {
        private readonly string ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Client;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";


        public void AddUser(User user)
        {
            using var db = new SqlConnection(ConnectionString);
            var query = @"INSERT INTO Users(Login,[Password],Username) 
                          VALUES (@Login,@Password,@Username)";
            db.Execute(query, user);
        }

        public User GetUserByLoginAndPassword(string login, string password)
        {
            using var db = new SqlConnection(ConnectionString);
            var query = @" SELECT * FROM Users
                           WHERE Login LIKE @login AND [Password] LIKE @password";
            return db.Query<User>(query, new { login, password }).FirstOrDefault();
        }

        public List<User> GetUsers()
        {
            using var db = new SqlConnection(ConnectionString);
            var query = @"SELECT * FROM Users";
            return db.Query<User>(query).ToList();
        }

        public void DeleteUserById(int id)
        {
            using var db = new SqlConnection(ConnectionString);
            var query = @"DELETE FROM Users WHERE Id = @id";
            db.Execute(query, new { id });
        }

        public List<Group> GetGroups()
        {
            using var db = new SqlConnection(ConnectionString);
            var query = @"SELECT * FROM Groups ORDER BY Date";
            return db.Query<Group>(query).ToList();
        }

        public List<string> GetAllChat()
        {
            using var db = new SqlConnection(ConnectionString);
            var query = @"SELECT CONCAT(Users.Username,' : ' + Text) FROM AllChats
                          JOIN Users ON AllChats.UserId =Users.Id
                          ORDER BY AllChats.Date";

            return db.Query<string>(query).ToList();
        }

        public List<string> GetInbox(int firstUserid, int secondUserid)
        {
            using var db = new SqlConnection(ConnectionString);
            var query = @"SELECT CONCAT(Users.Username,' : ' + Text) FROM Inboxs 
                          Join Users on Users.Id = UserSendId
                          WHERE (UserSendId = @secondUserid and  UserReceivedId = @firstUserid) or (UserSendId = @firstUserid and  UserReceivedId = @secondUserid)
                          ORDER BY Inboxs.Date";
            return db.Query<string>(query, new { firstUserid, secondUserid }).ToList();
        }

        public List<string> GetGroupChat(int groupId)
        {
            using var db = new SqlConnection(ConnectionString);
            var query = @"select CONCAT(Users.Username,' : ' + Text) from GroupsChat
                          join Groups on GroupId = Groups.Id
                          join Users on Users.Id = UserSendId
                          where Groups.Id = @groupId
                          ORDER BY GroupsChat.Date";
            return db.Query<string>(query, new { groupId }).ToList();


        }

        public void AddAllChat(int idUser, string msg)
        {
            using var db = new SqlConnection(ConnectionString);
            var query = @"insert into AllChats(UserId,[Text]) values (@idUser,@msg)";
            db.Execute(query, new { idUser, msg });
        }

        public void AddInbox(int idUserSend, int idUserReceive, string msg)
        { 
            using var db = new SqlConnection(ConnectionString);
            var query = @"insert into Inboxs(UserSendId,UserReceivedId,[Text]) values (@idUserSend,@idUserReceive,@msg)";
            db.Execute(query, new { idUserSend, idUserReceive, msg });
        }

        public void AddGroupMsg(int groupId, int userSendId, string msg)
        {
            using var db = new SqlConnection(ConnectionString);
            var query = @"insert into GroupsChat(GroupId, UserSendId, [Text]) values(@groupId,@userSendId,@msg)";
            db.Execute(query, new { groupId, userSendId, msg });
        }

        public void AddGroup(string name)
        {
            using var db = new SqlConnection(ConnectionString);
            var query = @"INSERT INTO Groups(Name) 
                          VALUES (@name)";
            db.Execute(query, new { name });
        }

        public Group GetGroupByName(string name)
        {
            using var db = new SqlConnection(ConnectionString);
            var query = @"SELECT * FROM Groups where Groups.Name = @name";
            return db.Query<Group>(query, new { name }).FirstOrDefault();
        }

        public void AddUserInGroup(List<User> users, int groupId)
        {
            using var db = new SqlConnection(ConnectionString);
            var query = @"INSERT INTO GroupsUsers(UserId,GroupId) 
                          VALUES (@Id,@groupId)";

            foreach (var user in users)
            {
                db.Execute(query, new { user.Id, groupId });
            }
        }

        public List<int> GetGroupsIdByUser(int Id)
        { 

            using var db = new SqlConnection(ConnectionString);
            var query = @"SELECT GroupId FROM GroupsUsers Where GroupsUsers.UserId = @Id";
            return db.Query<int>(query, new { Id }).ToList();


        }

        public List<Group> GetGroupsById(int id)
        {
            using var db = new SqlConnection(ConnectionString);
            var query = @"SELECT * FROM Groups where Id = @id ORDER BY Date";
            return db.Query<Group>(query,new { id }).ToList();
        } 

        public List<string> GetAllChatByUserDate(DateTime date)
        {
            using var db = new SqlConnection(ConnectionString);
            var query = @"SELECT CONCAT(Users.Username,' : ' + Text) FROM AllChats
                          JOIN Users ON AllChats.UserId =Users.Id
                          WHERE AllChats.Date>= @date
                          ORDER BY AllChats.Date";

            return db.Query<string>(query, new { date } ).ToList(); ;
        }
    }
}
