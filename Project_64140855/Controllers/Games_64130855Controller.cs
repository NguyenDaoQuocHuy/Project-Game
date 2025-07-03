using Project_64140855.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace Project_64140855.Controllers
{
    public class Games_64130855Controller : Controller
    {
        private Project_64130855Entities db = new Project_64130855Entities();
        //
        public ActionResult GIOITHIEU_64130855_3()
        {
            return View();
        }
        //Dashboard
        public ActionResult Dashboard()
        {
            var totalOrders = db.Orders.Count();
            var totalRevenue = db.Orders.Sum(o => (decimal?)o.TotalAmount) ?? 0;
            var totalGames = db.Games.Count();
            var totalUsers = db.Users.Count();

            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-i))
                .OrderBy(d => d)
                .ToList();

            // Bước 1: Lấy dữ liệu ra trước, tránh các thuộc tính không hỗ trợ trong LINQ to Entities
            var last7DaysList = last7Days.ToList();

            // Bước 2: Xử lý định dạng hoặc các thuộc tính không hỗ trợ trong LINQ to Objects
            var ordersByDate = last7DaysList.Select(date => new OrderByDate
            {
                Date = date.ToString("dd/MM"),
                // Các xử lý khác
            }).ToList();


            var orderStatusGroups = db.Orders
                .GroupBy(o => o.Status)
                .Select(g => new OrderStatusGroup
                {
                    Status = g.Key,
                    Count = g.Count()
                }).ToList();

            var model = new Dashboard_64130855
            {
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                TotalGames = totalGames,
                TotalUsers = totalUsers,
                OrdersByDate = ordersByDate,
                OrderStatusGroups = orderStatusGroups
            };

            return View(model);
        }

        //QUẢN LÝ USER
        public ActionResult QlyUser(string search = "", string role = "", int page = 1, int pageSize = 5)
        {
            var users = db.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(u => u.UserName.Contains(search) || u.Email.Contains(search));
            }

            if (!string.IsNullOrEmpty(role))
            {
                users = users.Where(u => u.Role == role);
            }

            int totalItems = users.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var data = users
            .OrderBy(u => u.UserId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;

            return View(data);
        }

        // GET: Users/Create
        public ActionResult CreateUser()
        {
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUser(User user, string password)
        {
            if (ModelState.IsValid)
            {
                // Mã hóa mật khẩu trước khi lưu
                user.Password = HashPassword(password);
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("QlyUser");
            }

            return View(user);
        }

        // GET: Users/Edit/5
        public ActionResult EditUser(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(User user, string newPassword)
        {
            if (ModelState.IsValid)
            {
                var existingUser = db.Users.Find(user.UserId);
                if (existingUser == null) return HttpNotFound();

                existingUser.UserName = user.UserName;
                existingUser.FullName = user.FullName;
                existingUser.Email = user.Email;
                existingUser.Role = user.Role;

                if (!string.IsNullOrEmpty(newPassword))
                {
                    existingUser.Password = newPassword;

                }

                db.SaveChanges();
                return RedirectToAction("QlyUser");
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public ActionResult DeleteUser(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("DeleteConfirmedUser")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmedUser(int id)
        {
            var user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("QlyUser");
        }

        private string HashPassword(string password)
        {
            
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }
        // QUẢN LÝ DANH MỤC
        public ActionResult QlyCategories(int page = 1, int pageSize = 5)
        {
            var categories = db.Categories;
            int totalItems = categories.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var data = categories
                .OrderBy(c => c.CategoryId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(data); // Trả về danh sách danh mục
        }


        public ActionResult CreateCategories()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateCategories(Category category)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction("QlyCategories");
            }
            return View(category);
        }

        public ActionResult EditCategories(int id)
        {
            var category = db.Categories.Find(id);
            return View(category);
        }

        [HttpPost]
        public ActionResult EditCategories(Category category)
        {
            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("QlyCategories");
            }
            return View(category);
        }

        public ActionResult DeleteCategories(int id)
        {
            var category = db.Categories.Find(id);
            return View(category);
        }

        [HttpPost, ActionName("DeleteConfirmedCategories")]
        public ActionResult ConfirmedCategories(int id)
        {
            var category = db.Categories.Find(id);
            db.Categories.Remove(category);
            db.SaveChanges();
            return RedirectToAction("QlyCategories");
        }
        //QUẢN LÝ GAME
        public ActionResult QlyGames(string search, int? categoryId, int page = 1, int pageSize = 5)
        {

            var games = db.Games.Include(g => g.Category);

            if (!string.IsNullOrEmpty(search))
            {
                games = games.Where(g => g.Title.Contains(search));
            }

            if (categoryId.HasValue)
            {
                games = games.Where(g => g.CategoryId == categoryId.Value);
            }


            ViewBag.Categories = new SelectList(db.Categories.ToList(), "CategoryId", "CategoryName");

            int totalItems = games.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var data = games
            .OrderBy(g => g.GameId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;

            return View(data);
        }

        // GET: AdminGames/Create
        public ActionResult CreateGames()
        {
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CategoryName");
            var game = new Game(); // tạo object mới để tránh null
            return View(game);
        }

        // POST: AdminGames/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateGames(Game game, HttpPostedFileBase uploadImage)
        {
            if (ModelState.IsValid)
            {
                if (uploadImage != null && uploadImage.ContentLength > 0)
                {
                    string fileName = System.IO.Path.GetFileName(uploadImage.FileName);
                    string path = Server.MapPath("~/Images/" + fileName);
                    uploadImage.SaveAs(path);
                    game.ImageUrl = fileName; // lưu tên file vào DB
                    System.Diagnostics.Debug.WriteLine(">>> Tên ảnh đã lưu: " + game.ImageUrl);
                }
                
                db.Games.Add(game);
                db.SaveChanges();
                return RedirectToAction("QlyGames");
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CategoryName", game.CategoryId);
            return View(game);
        }

       
        public ActionResult EditGames(int id)
        {
            
            var game = db.Games.Find(id);
            if (game == null) return HttpNotFound();
            ViewBag.CategoryId = new SelectList(db.Categories.ToList(), "CategoryId", "CategoryName", game.CategoryId);
            return View(game);
        }

        
        [HttpPost]
        public ActionResult EditGames(Game game, HttpPostedFileBase uploadImage)
        {
            if (ModelState.IsValid)
            {
                var existing = db.Games.Find(game.GameId);
                if (existing == null)
                {
                    return HttpNotFound();
                }

                // Cập nhật dữ liệu
                existing.Title = game.Title;
                existing.Description = game.Description;
                existing.Price = game.Price;
                existing.CategoryId = game.CategoryId;

                if (uploadImage != null && uploadImage.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(uploadImage.FileName);
                    string path = Server.MapPath("~/Images/" + fileName);
                    uploadImage.SaveAs(path);
                    existing.ImageUrl = fileName;
                }

                db.SaveChanges(); 
                return RedirectToAction("QlyGames");
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "CategoryName", game.CategoryId);
            return View(game);
        }


        // GET: AdminGames/Delete/5
        public ActionResult DeleteGames(int id)
        {
            var game = db.Games.Find(id);
            if (game == null) return HttpNotFound();

            return View(game);
        }

        // POST: AdminGames/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmedGames(int id)
        {
            var game = db.Games.Find(id);
            if (game == null)
                return HttpNotFound();

            db.Games.Remove(game);
            db.SaveChanges();

            return RedirectToAction("QlyGames");
        }
        public ActionResult Order(string searchUser, string status, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 5)
        {
            IQueryable<Order> orders = db.Orders.Include("User");


            if (!string.IsNullOrEmpty(searchUser))
            {
                orders = orders.Where(o => o.User.FullName.Contains(searchUser));
            }

            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.Status == status);
            }

            if (fromDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate <= toDate.Value);
            }

            int totalItems = orders.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var data = orders
            .OrderBy(o => o.OrderId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = searchUser;
            ViewBag.Search = status;

            return View(data);
        }
        public ActionResult DetailsOrder(int id)
        {
            var order = db.Orders
                          .Include("User")
                          .Include("OrderDetails.Game")
                          .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

    }
}