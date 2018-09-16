using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using BankAccounts.Models;
using System.Linq;

namespace BankAccounts.Controllers {
    public class HomeController : Controller {
        private YourContext _context;
        public HomeController(YourContext context) {
            _context = context;
        }

        [HttpGet]
        [Route("account/{user_id}")]
        public IActionResult ShowAccount(int user_id) {
            // if user is not when trying to visit route then redirect back to home
            if(HttpContext.Session.GetInt32("id") == null) {
                return RedirectToAction("Index", "User");
            }
            // if user tries to visit another account then redirect back to home
            if(HttpContext.Session.GetInt32("id") != user_id) {
                return RedirectToAction("Index", "User");
            }
            // include the transactions list for each user if it matches the user id
            User currentuser = _context.Users
                                .Include(user => user.Transactions)
                                .Where(user => user.User_Id == user_id).SingleOrDefault();
            if(currentuser.Transactions != null) {
                // order transactions by date and only show the 5 most recent
                currentuser.Transactions = currentuser.Transactions.OrderByDescending(t => t.Date).Take(5).ToList();
            }
            ViewBag.User = currentuser;
            return View();
        }

        [HttpPost]
        [Route("transaction")]
        public IActionResult Transaction(double amount) {
            int? user_id = HttpContext.Session.GetInt32("id");
            User CurrentUser = _context.Users.SingleOrDefault(user => user.User_Id == HttpContext.Session.GetInt32("id"));
            // if user tries to withdraw less than what they have then throw error
            if(CurrentUser.Balance + amount < 0) {
                TempData["error"] = "Insufficient funds.";
                return Redirect($"/account/{user_id}");
            }
            else {
                Transaction newTrans = new Transaction 
                {
                    Amount = (double) amount,
                    Date = DateTime.UtcNow,
                    UserId = (int) user_id,
                };
                _context.Add(newTrans);
                // add the current balance and the amount submitted from the form
                CurrentUser.Balance += amount;
                _context.SaveChanges();
                return Redirect($"/account/{user_id}");
            }
        }
    }
}