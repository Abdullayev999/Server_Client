CREATE DATABASE  Client

GO
USE Client


CREATE TABLE Users
(
	Id INT PRIMARY KEY IDENTITY,  
	[Login] NVARCHAR(30) NOT NULL, 
    [Password] NVARCHAR(150) NOT NULL, 
    Username NVARCHAR(30) NOT NULL,
	[Date] DATETIME NOT NULL DEFAULT(GETDATE()),
    
	CONSTRAINT UQ_Users_Login UNIQUE([Login])  
) 


CREATE TABLE Inboxs
(
	[Date] DATETIME NOT NULL DEFAULT(GETDATE()),
	UserSendId INT NOT NULL,   
	UserReceivedId INT NOT NULL,   
    [Text] NVARCHAR(150) NOT NULL,  
	  
	CONSTRAINT FK_Inboxs_UserSendId  FOREIGN KEY  (UserSendId) REFERENCES Users(Id),
	CONSTRAINT FK_Inboxs_UserReceivedId  FOREIGN KEY  (UserReceivedId) REFERENCES Users(Id)
) 


CREATE TABLE AllChats
(
	Id INT PRIMARY KEY IDENTITY, 
	[Date] DATETIME NOT NULL DEFAULT(GETDATE()),
	UserId INT NOT NULL,  
    [Text] NVARCHAR(150) NOT NULL 

	CONSTRAINT FK_AllChats_UserId  FOREIGN KEY  (UserId) REFERENCES Users (Id),
) 



CREATE TABLE Groups
(
    Id INT PRIMARY KEY IDENTITY,  
	[Name] NVARCHAR(30) NOT NULL,
    [Date] DATETIME NOT NULL DEFAULT(GETDATE()),

	CONSTRAINT UQ_Groups_Name UNIQUE([Name])  
)


CREATE TABLE GroupsUsers
(
    UserId INT NOT NULL, 
    GroupId INT NOT NULL,   
	
	CONSTRAINT FK_GroupsUsers_UserId  FOREIGN KEY  (UserId) REFERENCES Users (Id), 
	CONSTRAINT FK_GroupsUsers_GroupId FOREIGN KEY  (GroupId) REFERENCES Groups (Id)
)


CREATE TABLE GroupsChat
( 
    GroupId INT NOT NULL,  
	[Date] DATE NOT NULL DEFAULT(GETDATE()),
	UserSendId INT NOT NULL,  
	[Text] NVARCHAR(150) NOT NULL, 
	 
    CONSTRAINT FK_GroupsChat_UserSendId  FOREIGN KEY  (UserSendId) REFERENCES Users(Id),
	CONSTRAINT UQ_GroupsChat_GroupId FOREIGN KEY  (GroupId) REFERENCES Groups (Id)
) 

SELECT * FROM Groups
JOIN GroupsUsers ON Groups.Id = GroupsUsers.GroupId
JOIN GroupsChat ON GroupsChat.GroupId = Groups.Id

SELECT * FROM GroupsChat
JOIN Groups ON Groups.Id = GroupsChat.GroupId
JOIN GroupsUsers ON Groups.Id = GroupsUsers.GroupId 



insert into GroupsChat(GroupId,UserSendId,[Text]) values (1,6,'salam eziz tamawacilar')
insert into GroupsChat(GroupId,UserSendId,[Text]) values (1,4,'lol')
insert into GroupsChat(GroupId,UserSendId,[Text]) values (1,11,'siktir de')

 

 select CONCAT(Users.Username,' : ' + Text) from GroupsChat
 join Groups on GroupId = Groups.Id
 join Users on Users.Id = UserSendId
 where Groups.Id = 1


insert into AllChats(UserId,[Text]) values (4,'Baku')
insert into AllChats(UserId,[Text]) values (5,'salam necesen')
insert into AllChats(UserId,[Text]) values (6,'sagolwukur sen necesen')




insert into Inboxs(UserSendId,UserReceivedId,[Text]) values (4,6,'sBABAT')
insert into Inboxs(UserSendId,UserReceivedId,[Text]) values (6,4,'bratuxa')
insert into Inboxs(UserSendId,UserReceivedId,[Text]) values (4,6,'ua aleykjma')
 
insert into Inboxs(UserSendId,UserReceivedId,[Text]) values (11,13,'abdullayev')
insert into Inboxs(UserSendId,UserReceivedId,[Text]) values (13,11,'ferid')
insert into Inboxs(UserSendId,UserReceivedId,[Text]) values (11,13,'Ewref')


SELECT CONCAT(Users.Username,' : ' + Text) FROM Inboxs 
Join Users on Users.Id = UserSendId
WHERE (UserSendId = 6 and  UserReceivedId = 4) or (UserSendId = 4 and  UserReceivedId = 6)
ORDER BY Date



SELECT CONCAT(Users.Username,' : ' + Text) FROM AllChats
JOIN Users ON AllChats.UserId =Users.Id



SELECT UserFirstId,UserSecondId, Text FROM Inboxs  
Join Users on Users.Id = UserFirstId
WHERE (UserFirstId = 11 and UserSecondId = 13) OR (UserFirstId = 13 and UserSecondId = 11)


 select CONCAT(Users.Username,' : ' + Text) from GroupsChat
                          join Groups on GroupId = Groups.Id
                          join Users on Users.Id = UserSendId
                          where Groups.Id = 1
                          ORDER BY Groups.Date

						  DELETE Users
SELECT * FROM Users
SELECT * FROM Inboxs
SELECT * FROM Groups
SELECT * FROM AllChats
SELECT * FROM GroupsUsers
Where GroupsUsers.UserId = 1
SELECT * FROM GroupsChat


SELECT CONCAT(Users.Username,' : ' + Text) FROM AllChats
                          JOIN Users ON AllChats.UserId =Users.Id
                          WHERE Users.Date>= '2021-08-20'
                          ORDER BY AllChats.Date

SELECT * FROM GroupsChat
JOIN GroupsUsers ON GroupsUsers.GroupId =GroupsChat.GroupId
JOIN Users on GroupsUsers.UserId = Users.Id


SELECT GroupId FROM GroupsUsers Where GroupsUsers.UserId = 1
SELECT * FROM Groups where Id = 1 ORDER BY Date



SELECT CONCAT(Users.Username,' : ' + Text) FROM AllChats
                          JOIN Users ON AllChats.UserId =Users.Id
						   WHERE Users.Date <= GETDATE()
                          ORDER BY AllChats.Date
                         