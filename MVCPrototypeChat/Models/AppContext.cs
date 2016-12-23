using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MVCPrototypeChat.Models
{
    public class AppContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        //public System.Data.Entity.DbSet<MVCPrototypeChat.ViewModels.UserViewModel> UserViewModels { get; set; }
    }
}