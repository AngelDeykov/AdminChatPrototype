using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MVCPrototypeChat.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string CompanyName { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public int AdminCode { get; set; }

        [NotMapped]
        public string tpflag { get; set; }
        [NotMapped]
        public string freeflag { get; set; }
        [NotMapped]
        public string ConnectionId { get; set; }
        [NotMapped]
        public string UserGroup { get; set; }
    }
}