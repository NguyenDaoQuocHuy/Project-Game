using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Project_64140855.Models;

namespace Project_64140855.Controllers
{
    
    public class Account_64130855Controller : Controller
    {
        private Project_64130855Entities db = new Project_64130855Entities();

        public ActionResult Login()
        {
            return View();
        }


        [HttpPost]
        
        public ActionResult Login(User model)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);
                if (user != null)
                {

                    Session["UserId"] = user.UserId;
                    Session["UserName"] = user.UserName;
                    Session["Email"] = user.Email;
                    Session["FullName"] = user.FullName;
                    Session["Role"] = user.Role;

                    if (user.Role == "Admin")
                        return RedirectToAction("GIOITHIEU_64130855_3", "Games_64130855");
                    else
                        return RedirectToAction("GIOITHIEU_64130855_2", "Users_64130855");
                }

                ModelState.AddModelError("", "Sai email hoặc mật khẩu.");
            }

            return View(model);
        }



        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User model, string ConfirmPassword)
        {
            // Kiểm tra xác nhận mật khẩu
            if (model.Password != ConfirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                var exist = db.Users.Any(u => u.UserName == model.UserName);
                if (exist)
                {
                    ModelState.AddModelError("", "Tên đăng nhập đã tồn tại.");
                    return View(model);
                }

                User user = new User
                {
                    UserName = model.UserName,
                    Password = model.Password,
                    FullName = model.FullName,
                    Email = model.Email,
                    Role = "User"
                };

                db.Users.Add(user);
                db.SaveChanges();

                Session["UserId"] = user.UserId;
                Session["UserName"] = user.UserName;
                Session["FullName"] = user.FullName;
                Session["Role"] = user.Role;

                return RedirectToAction("Login", "Account_64130855");
            }

            return View(model);
        }

        public ActionResult RegisterAdmin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterAdmin(User model)
        {
            if (ModelState.IsValid)
            {
                var exist = db.Users.Any(u => u.UserName == model.UserName);
                if (exist)
                {
                    ModelState.AddModelError("", "Tên đăng nhập đã tồn tại.");
                    return View(model);
                }

                User user = new User
                {
                    UserName = model.UserName,
                    Password = model.Password,
                    FullName = model.FullName,
                    Email = model.Email,
                    Role = "Admin"
                };

                db.Users.Add(user);
                db.SaveChanges();

                Session["UserId"] = user.UserId;
                Session["UserName"] = user.UserName;
                Session["Role"] = user.Role;

                return RedirectToAction("Login", "Account_64130855");
            }

            return View(model);
        }

        public ActionResult DangXuat()
        {
            Session.Clear();
            return RedirectToAction("GIOITHIEU_64130855", "Games_64130855");
        }
    }

   
}