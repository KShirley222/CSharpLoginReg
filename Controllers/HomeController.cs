using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LoginNRegister.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace LoginNRegister.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;

        public HomeController(MyContext context)
        {
            dbContext = context;
        }

        [HttpGet("/")] 
        public IActionResult Index()
        {
            // List<User> AllUsers = dbContext.Users.ToList();
            return View();
        }
        
        [HttpPost("register")]
        public IActionResult Register(User newUser)
        {
            if(ModelState.IsValid)
            {
                if(dbContext.Users.Any(u => u.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "Email already in use.");
                    return View("index", newUser);
                }
                else
                {
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    newUser.Password = Hasher.HashPassword( newUser, newUser.Password);
                    
                    dbContext.Add(newUser);
                    dbContext.SaveChanges();
                    return Redirect("success"); 
                }
            }
            return View("index", newUser);
        }
        
        [HttpGet("login")]
        public IActionResult ViewLogin()
        {
            return View("login");
        }

        [HttpPost("login")]
        public IActionResult Login(LoginUser userSubmission)
        {
            if(ModelState.IsValid)
            {
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == userSubmission.Email);
                if (userInDb == null)
                {
                    ModelState.AddModelError( "Email", "Invalid Email/Password");
                    return View("login");
                }
                var hasher = new PasswordHasher<LoginUser>();
                var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);
                if(result == 0)
                {
                    ModelState.AddModelError( "Email", "Invalid Email/Password");
                    return View("login");
                }
                else
                {
                    return View("success");
                }

            }
            return View("login");
        }

        [HttpGet("success")]
        public IActionResult Success()
        {
            return View("success");
        }
    }
}
