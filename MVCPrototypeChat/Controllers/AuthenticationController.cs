using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCPrototypeChat.ViewModels;
using System.Web.Security;
using System.Data.Entity.Validation;
using MVCPrototypeChat.Models;
using System.Diagnostics;

namespace MVCPrototypeChat.Controllers
{
    public class AuthenticationController : Controller
    {
        // GET: Authentication
        [HttpGet]
        public ActionResult LogIn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LogIn(UserLoginViewModel user)
        {
            if (ModelState.IsValid)
            {
                if (IsValid(user.Email, user.Password))
              {
                  FormsAuthentication.SetAuthCookie(user.Email, false);
                  return RedirectToAction("Index", "Home");               
              }
              else
              {
                ModelState.AddModelError("", "Incorrect Email or Password. Please check your details!");
              }
            }
            return View(user);
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (AppContext db = new AppContext())
                    {
                        User email = db.Users.FirstOrDefault(u => u.Email.ToLower() == user.Email.ToLower());
                        // Check if email already exists
                        if (email == null)
                        {
                            var crypto = new SimpleCrypto.PBKDF2();
                            var encrypPass = crypto.Compute(user.Password);


                            var newUser = db.Users.Create();
                            newUser.FirstName = user.FirstName;
                            newUser.LastName = user.LastName;
                            newUser.Email = user.Email;
                            newUser.CompanyName = user.CompanyName;
                            newUser.Password = encrypPass;
                            newUser.PasswordSalt = crypto.Salt;
                            newUser.AdminCode = 0;
                            user.Password = encrypPass;
                            user.PasswordSalt = crypto.Salt;
                            db.Users.Add(newUser);
                            db.SaveChanges();

                            FormsAuthentication.SetAuthCookie(user.Email, false);
                            return RedirectToAction("Index", "Home");
                        }
                       else {
                            ModelState.AddModelError("Email", "Email address already exists.");
                        }
                    }

            }

            catch (DbEntityValidationException e)
            {
                foreach (var validationErrors in e.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation(
                              "Class: {0}, Property: {1}, Error: {2}",
                              validationErrors.Entry.Entity.GetType().FullName,
                              validationError.PropertyName,
                              validationError.ErrorMessage);
                    }
                }
            }

        }

            return View();
        }

        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        private bool IsValid(string email, string password)
        {
            var crypto = new SimpleCrypto.PBKDF2();
            bool IsValid = false;

            using (AppContext db = new AppContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Email == email);
                if (user != null )
                {
                    if (user.Password == crypto.Compute(password, user.PasswordSalt))
                    {
                        IsValid = true;
                    }
                }
            }
            return IsValid;
        }
    }
}