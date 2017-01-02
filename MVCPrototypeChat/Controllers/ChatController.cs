using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCPrototypeChat.Models;

namespace MVCPrototypeChat.Controllers
{
    public class ChatController : Controller
    {
        // GET: Chat
        public ActionResult Chat()
        {
            if (User.Identity.IsAuthenticated)
            {
                AppContext db = new AppContext();
                User user = db.Users.Single(usr => usr.Email == User.Identity.Name);
                int isAdmin = user.AdminCode;
                if (isAdmin == 0)
                {
                    return View("UserChat");
                }
                else
                {
                    return View("AdminChat");
                }
            }
            else
            {
                return View("UnauthUserChat");
            }

        }
    }
}
