using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using MVCPrototypeChat.Models;

namespace MVCPrototypeChat.Hubs
{
    public class ChatHub : Hub
    {
            static List<User> UsersList = new List<User>();
            static List<MessageInfo> MessageList = new List<MessageInfo>();
           //static List<UserInfo> WaitingUserssList = new List<UserInfo>();

        public void UnAuthConnect(string userName)
        {
            var id = Context.ConnectionId;
            string userGroup = "";
            //Random random = new Random();
            //int randomNumber = random.Next(1000, 9000);
            try
            {
                var strg = (from s in UsersList where (s.tpflag == "1") && (s.freeflag == "1") select s).First();
                UsersList.Add(new User { ConnectionId = id, Email = userName, UserGroup = userGroup, freeflag = "0", tpflag = "0", });
                Groups.Add(Context.ConnectionId, userGroup);
                Clients.Caller.onConnected(id, userName, userGroup);
               // Clients.Caller.onConnected(id, userName,  userGroup);
            }
            catch
            {
                Clients.Caller.NoExistAdmin(userName);
            }
        }

        public void Connect(string userName)
        {
            var id = Context.ConnectionId;
            string userGroup = "";
            //Manage Hub Class
            //if freeflag==0 ==> Busy
            //if freeflag==1 ==> Free

            //if tpflag==0 ==> User
            //if tpflag==1 ==> Admin

            AppContext db = new AppContext();
            var userInfo = (from m in db.Users
                            where m.Email == HttpContext.Current.User.Identity.Name
                            select new { m.UserId, m.Email, m.AdminCode, m.FirstName, m.LastName }).FirstOrDefault();

            try
            {
                //You can check if user or admin did not login before by below line which is an if condition
                //if (UsersList.Count(x => x.ConnectionId == id) == 0)
                //Here you check if there is no userGroup which is same DepID --> this is User otherwise this is Admin
                //userGroup = DepID
                if ((int)userInfo.AdminCode == 0)
                {
                    //now we encounter ordinary user which needs userGroup and at this step, system assigns the first of free Admin among UsersList
                    var strg = (from s in UsersList where (s.tpflag == "1") && (s.freeflag == "1") select s).First();
                    // var flag = strg.tpflag;
                    userGroup = strg.UserGroup;
                    //Admin becomes busy so we assign zero to freeflag which is shown admin is busy
                    strg.freeflag = "0";
                    //now add USER to UsersList
                    UsersList.Add(new User { ConnectionId = id, UserId = userInfo.UserId, Email = userName, UserGroup = userGroup, freeflag = "0", tpflag = "0", });
                    var flag = (from s in UsersList where (s.Email == userName) select s.tpflag);
                    //whether it is Admin or User now both of them has userGroup and I Join this user or admin to specific group 
                    Groups.Add(Context.ConnectionId, userGroup);
                    //Clients.Caller.onConnected(id, userName, userInfo.UserID, userGroup);
                    Clients.Caller.onConnected(id, userName, userGroup, flag);
                    //WaitingUserssList.Remove(WaitingUserssList.Single(s => s.UserName == userName));
                }
                else
                {
                    //If user has admin code so admin code is same userGroup
                    //now add ADMIN to UsersList
                    UsersList.Add(new User { ConnectionId = id, UserId = userInfo.UserId, Email = userName, UserGroup = userInfo.AdminCode.ToString(), freeflag = "1", tpflag = "1" });
                    var flag = (from s in UsersList where (s.Email == userName) select s.tpflag);
                    //whether it is Admin or User now both of them has userGroup and I Join this user or admin to specific group 
                    Groups.Add(Context.ConnectionId, userInfo.AdminCode.ToString());
                    Clients.Caller.onConnected(id, userName, userInfo.AdminCode.ToString(), flag);
                }
            }

            catch
            {
                //string msg = "All Administrators are busy, please be patient and try again";
                //***** Return to Client *****
                Clients.Caller.NoExistAdmin(HttpContext.Current.User.Identity.Name);
                //WaitingUserssList.Add(new UserInfo { ConnectionId = id, UserID = userInfo.UserID, UserName = userName });
                //var waitingUserName = (from s in WaitingUserssList select s.UserName);
               // Clients.All.getMessages(waitingUserName);
            }
        }
        // <<<<<-- ***** Return to Client [  NoExist  ] *****

        //--group ***** Receive Request From Client [  SendMessageToGroup  ] *****
        public void SendMessageToGroup(string userName, string message)
        {
            if (UsersList.Count != 0)
            {
                var strg = (from s in UsersList where (s.Email == userName) select s).First();
                MessageList.Add(new MessageInfo { UserName = userName, Message = message, UserGroup = strg.UserGroup });
                string strgroup = strg.UserGroup;
                // If you want to Broadcast message to all UsersList use below line
                // Clients.All.getMessages(userName, message);
                //If you want to establish peer to peer connection use below line so message will be send just for user and admin who are in same group
                //***** Return to Client *****
                //Clients.Group(strgroup).getMessages(userName, message);
                Clients.Group(strgroup).getMessagesAdmin(userName, message);
                Clients.Group(strgroup).getMessagesUser(userName, message);
            }
        }
        // End SendMessage

        //--group ***** Receive Request From Client ***** { Whenever User close session then OnDisconneced will be occurs }
        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            var item = UsersList.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (item != null)
            {
                UsersList.Remove(item);
                var id = Context.ConnectionId;
                if (item.tpflag == "0")
                {
                    //user logged off == user
                    try
                    {
                        var stradmin = (from s in UsersList where (s.UserGroup == item.UserGroup) && (s.tpflag == "1") select s).First();
                        //become free
                        stradmin.freeflag = "1";
                    }
                    catch
                    {
                        //***** Return to Client *****
                        var userName = HttpContext.Current.User.Identity.Name;
                        Clients.Caller.NoExistAdmin(userName);
                    }

                }
                //save conversation to dat abase
            }
            return base.OnDisconnected(stopCalled);
        }

    }
}