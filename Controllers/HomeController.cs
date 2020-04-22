using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LoginNRegister.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

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
                    // Hashing password
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    newUser.Password = Hasher.HashPassword( newUser, newUser.Password);

                    // creating new user in DB
                    dbContext.Add(newUser);
                    dbContext.SaveChanges();
                    
                    // Creating session 
                    HttpContext.Session.SetInt32("UserId", newUser.UserId);

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
                // Check DB for matching user email
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == userSubmission.Email);
                if (userInDb == null)
                {
                    // if false
                    ModelState.AddModelError( "Email", "Invalid Email/Password");
                    return View("login");
                }
                else{
                    //  if email is valid hash user login password and compare to hashed user in Db
                    var hasher = new PasswordHasher<LoginUser>();
                    var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);
                    if(result == 0)
                    {
                        // If password is invalid
                        ModelState.AddModelError( "Email", "Invalid Email/Password");
                        return View("login");
                    }
                    else
                    {
                        // Create Session
                         HttpContext.Session.SetInt32("UserId", userInDb.UserId);
                        return View("success");
                    }
                }

            }
            return View("login");
        }

        [HttpGet("success")]
        public IActionResult Success()
        {
            // Check for session userId
            int? LoginCheck = HttpContext.Session.GetInt32("UserId");
            if(LoginCheck == null)
            {
                return View("index");
            }
            else{
                return View("success");
            }
        }

        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            // clear session
            HttpContext.Session.Clear();
            return View("index");
        }
    }
}
