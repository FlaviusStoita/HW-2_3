using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver;
using M101DotNet.WebApp.Models;
using M101DotNet.WebApp.Models.Account;
using MongoDB.Bson;

namespace M101DotNet.WebApp.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            var model = new LoginModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var blogContext = new BlogContext();


            // XXX WORK HERE
            // fetch a user by the email in model.Email

            var user= new User() {Name = "", Email = ""};

            using (var cursor = await blogContext.Users.Find(new BsonDocument("Email",model.Email) ).ToCursorAsync())
            { while (await cursor.MoveNextAsync())
                { foreach (var usr in cursor.Current) {
                            user.Name = usr.Name;
                            user.Email = usr.Email; } 
                }
            
            }
             //Done !!


            if (user.Name == "")
                {
                    ModelState.AddModelError("Email", "Email address has not been registered.");
                    return View(model);
                }

            var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email)
                },
                "ApplicationCookie");

            var context = Request.GetOwinContext();
            var authManager = context.Authentication;

            authManager.SignIn(identity);

            return Redirect(GetRedirectUrl(model.ReturnUrl));
        }

        [HttpPost]
        public ActionResult Logout()
        {
            var context = Request.GetOwinContext();
            var authManager = context.Authentication;

            authManager.SignOut("ApplicationCookie");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View(new RegisterModel());
        }

        [HttpPost]
        public async Task<ActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var blogContext = new BlogContext();


            // XXX WORK HERE
            // create a new user and insert it into the database
            var col = blogContext.Users;

            await col.UpdateOneAsync
                (
                 Builders<User>.Filter.Eq("Email", model.Email),
                 Builders<User>.Update.Set("Name", model.Name),
                 options: new UpdateOptions{ IsUpsert=true }
                );
             // Done !

            return RedirectToAction("Index", "Home");
        }

        private string GetRedirectUrl(string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
            {
                return Url.Action("index", "home");
            }

            return returnUrl;
        }
    }
}