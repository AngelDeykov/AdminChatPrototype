using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using MVCPrototypeChat.Models;
using Newtonsoft.Json;

namespace MVCPrototypeChat.Hubs
{
    public class ChatHub : Hub
    {
            static List<User> UsersList = new List<User>();
            static List<MessageInfo> MessageList = new List<MessageInfo>();
            static List<WaitingUser> WaitingUseresList = new List<WaitingUser>();

        //call from client with 1 property
        public void UnAuthConnect(string userName)
        {
            var id = Context.ConnectionId;
            string userGroup = "";
            Random random = new Random();
            int randomNumber = random.Next(1000, 9000);
            try
            {
                var strg = (from s in UsersList where (s.tpflag == "1") && (s.freeflag == "1") select s).First();
                userGroup = strg.UserGroup;
                strg.freeflag = "0";
                UsersList.Add(new User { ConnectionId = id, UserId = randomNumber, Email = userName, UserGroup = userGroup, freeflag = "0", tpflag = "0", });
                Groups.Add(Context.ConnectionId, userGroup);
                Clients.Caller.onConnected(id, userName, randomNumber, userGroup);
                bool check = WaitingUseresList.Any(u => u.waitingUsrName == userName);
                if (check == true)
                {
                    var removeUsrName = WaitingUseresList.Single(rUsr => rUsr.waitingUsrName == userName);
                    WaitingUseresList.Remove(removeUsrName);
                    Clients.All.RemoveWaitingUser(userName);
                }
            }
            catch
            {
                if (!WaitingUseresList.Any(p => p.waitingUsrName == userName ))
                {
                    WaitingUseresList.Add(new WaitingUser { waitingUsrName = userName });
                    Clients.All.UpdateWaitingUsrList(userName);
                }               
                Clients.Caller.NoExistAdmin();
            }
        }

        //call from client with 1 property
        public void Connect(string userName)
        {
            var id = Context.ConnectionId;
            string userGroup = "";
            //if freeflag==0 ==> Busy
            //if freeflag==1 ==> Free

            //if tpflag==0 ==> User
            //if tpflag==1 ==> Admin

            //find the user in the database
            AppContext db = new AppContext();
            var userInfo = (from m in db.Users
                            where m.Email == HttpContext.Current.User.Identity.Name
                            select new { m.UserId, m.Email, m.AdminCode, m.FirstName, m.LastName }).FirstOrDefault();

            try
            {
                //if the admin code is 0, ordinary user
                if ((int)userInfo.AdminCode == 0)
                {
                    //we check our user list for free administrator
                    //if there is not we go to catch expresion
                    var strg = (from s in UsersList where (s.tpflag == "1") && (s.freeflag == "1") select s).First();
                    userGroup = strg.UserGroup;
                    //Admin becomes busy now and we set 0 to his freeflag
                    strg.freeflag = "0";
                    //add the user to UsersList
                    UsersList.Add(new User { ConnectionId = id, UserId = userInfo.UserId, Email = userName, UserGroup = userGroup, freeflag = "0", tpflag = "0", });
                    //the user joins the user group of admin 
                    Groups.Add(Context.ConnectionId, userGroup);
                    //return to client and call onConnected();
                    Clients.Caller.onConnected(id, userName, userInfo.UserId, userGroup);
                    //now we check if our user exists in the WaitingUseresList
                    //if yes we remove it from the WaitingUseresList and 
                    //call the client RemoveWaitingUser(), which will remove the user from the
                    //admin list "Users in queue"
                    bool check = WaitingUseresList.Any(u => u.waitingUsrName == userName);
                    if (check == true)
                    {
                        var removeUsrName = WaitingUseresList.Single(rUsr => rUsr.waitingUsrName == userName);
                        WaitingUseresList.Remove(removeUsrName);
                        Clients.All.RemoveWaitingUser(userName);
                    }
                }
                else
                {
                    //if admin code is NOT 0 it is admin
                    //we add the admin to the UsersList, and use his unique admin code for user group
                    UsersList.Add(new User { ConnectionId = id, UserId = userInfo.UserId, Email = userName, UserGroup = userInfo.AdminCode.ToString(), freeflag = "1", tpflag = "1" });
                    //return to client and call onConnected(); 
                    Groups.Add(Context.ConnectionId, userInfo.AdminCode.ToString());
                    Clients.Caller.onConnected(id, userName, userInfo.UserId, userInfo.AdminCode.ToString());
                }
            }

            catch
            {
                //if there is no free Admin, chech if the username has alraedy exists in the
                //WaitingUseresList and if not add the username to that list and send it to admin client
                if (!WaitingUseresList.Any(p => p.waitingUsrName == userName))
                {
                    WaitingUseresList.Add(new WaitingUser { waitingUsrName = userName });
                    Clients.All.UpdateWaitingUsrList(userName);
                }
                //return to client and call NoExistAdmin();
                Clients.Caller.NoExistAdmin();
            }
        }

        public void onClosedWindow(string userName)
        {
            bool check = WaitingUseresList.Any(u => u.waitingUsrName == userName);
            if (check == true)
            {
                var removeUsrName = WaitingUseresList.Single(rUsr => rUsr.waitingUsrName == userName);
                WaitingUseresList.Remove(removeUsrName);
                Clients.All.RemoveWaitingUser(userName);
            }
        }

        //request from client with 2 string parameters
        public void SendMessageToGroup(string userName, string message)
        {
            if (UsersList.Count != 0)
            {
                var strg = (from s in UsersList where (s.Email == userName) select s).First();
                MessageList.Add(new MessageInfo { UserName = userName, Message = message, UserGroup = strg.UserGroup });
                string strgroup = strg.UserGroup;
                //return to client in the group and call getMessages();
                Clients.Group(strgroup).getMessages(userName, message);
            }
        }

        //request from client
        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            //pick up the user with this unique connection id from the UsersList
            var item = UsersList.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (item != null)
            {
                //if he exists remove it from the list
                UsersList.Remove(item);
                var id = Context.ConnectionId;
                //if the disconnected user is ordinary user
                if (item.tpflag == "0")
                {
                    try
                    {
                        //find the admin with the same group
                        var stradmin = (from s in UsersList where (s.UserGroup == item.UserGroup) && (s.tpflag == "1") select s).First();
                        //and mark him as free
                        stradmin.freeflag = "1";
                    }
                    catch
                    {
                        Clients.Caller.NoExistAdmin();
                    }
                }
            }
            return base.OnDisconnected(stopCalled);
        }
    }
}