using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MVCPrototypeChat.Models;
using System.ComponentModel.DataAnnotations;

namespace MVCPrototypeChat.ViewModels
{
    public class UserRegisterViewModel
    {
        [Required(ErrorMessage = "First Name is required !")]
        [StringLength(50)]
        [Display(Name = "First Name: ")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required !")]
        [StringLength(50)]
        [Display(Name = "Last Name: ")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required !")]
        [EmailAddress]
        [StringLength(50)]
        [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", ErrorMessage = "Please enter valid email")]
        [Display(Name = "Email Address: ")]
        public string Email { get; set; }

        [StringLength(50)]
        [Display(Name = "Company Name: ")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Password is required !")]
        [DataType(DataType.Password)]
        [Display(Name = "Password: ")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords don't match !")]
        public string ConfirmPassword { get; set; }
    }

    public class UserLoginViewModel
    {
        [Required(ErrorMessage = "Email is required !")]
        [EmailAddress]
        [StringLength(50)]
        [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$", ErrorMessage = "Please enter valid email")]
        [Display(Name = "Email Address: ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required !")]
        [DataType(DataType.Password)]
        [Display(Name = "Password: ")]
        public string Password { get; set; }
    }
}