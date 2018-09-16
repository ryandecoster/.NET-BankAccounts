using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BankAccounts.Models;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace BankAccounts.Controllers
{
    
    public class UserController : Controller
    {
        private YourContext _context;
        public UserController(YourContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("login")]
        public IActionResult LoginUser()
        {
            return View("Login");
        }

        [HttpPost]
        [Route("register")]
        public IActionResult Register(User userReg)
        {
            // if email already exists in database then throw error
            if(_context.Users.Where(user => user.Email == userReg.Email).FirstOrDefault() != null)
            {
                ModelState.AddModelError("Email", "Email already in use.");
            }
            if (ModelState.IsValid){
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                userReg.Password = Hasher.HashPassword(userReg, userReg.Password);
                _context.Add(userReg);
                _context.SaveChanges();
                ViewBag.User = userReg;
                HttpContext.Session.SetInt32("id", userReg.User_Id);
                int? user_id = userReg.User_Id;
                return Redirect($"/account/{user_id}");
            }
            else {
                return View("Index", userReg);
            }
            
        }

        [HttpPost]
        [Route("login/user")]
        public IActionResult Login(User userLog)
        {
            // user variable to store a user if the email entered equals the same email in the database
            User check = _context.Users.FirstOrDefault(u => u.Email == userLog.Email);
            // if there is a user and password entered
            if(check != null && userLog.Password != null)
            {
                var Hasher = new PasswordHasher<User>();
                // if password entered matches the hashed password in database
                if(0 != Hasher.VerifyHashedPassword(check, check.Password, userLog.Password))
                {
                    ViewBag.User = check.User_Id;
                    HttpContext.Session.SetInt32("id", check.User_Id);
                    int? user_id = check.User_Id;
                    return Redirect($"/account/{user_id}");
                }
                ModelState.AddModelError("password", "Incorrect email or password.");
                return View("Login", userLog);
                
            }
            ModelState.AddModelError("email", "Incorrect email or password.");
            return View("Login", userLog);
        }

        [HttpGet]
        [Route("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["success"] = "You have successfully logged out.";
            return Redirect("/");
        }
    }
}
