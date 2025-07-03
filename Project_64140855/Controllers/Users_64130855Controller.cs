using Project_64140855.Models;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity; // nhớ thêm using này để dùng Include()
using System;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Data.Entity.Validation;


namespace Project_64140855.Controllers
{
    public class Users_64130855Controller : Controller
    {
        private Project_64130855Entities db = new Project_64130855Entities();

        // GET: Users_64130855
        public ActionResult GIOITHIEU_64130855_2()
        {
            return View();
        }
        public ActionResult Support_2()
        {
            return View();
        }

        public ActionResult Index_2(string search, int? categoryId, decimal? minPrice, decimal? maxPrice, int page = 1, int pageSize = 8)
        {
            var games = db.Games.Include(g => g.Category).AsQueryable().ToList();

            if (!string.IsNullOrEmpty(search))
                games = games.Where(g => g.Title.Contains(search)).ToList();

            if (categoryId.HasValue)
                games = games.Where(g => g.CategoryId == categoryId.Value).ToList();

            if (minPrice.HasValue)
                games = games.Where(g => g.Price >= minPrice.Value).ToList();

            if (maxPrice.HasValue)
                games = games.Where(g => g.Price <= maxPrice.Value).ToList();

            var categories = db.Categories.Select(c => new { c.CategoryId, c.CategoryName }).ToList();

            int totalItems = games.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var data = games
            .OrderBy(c => c.CategoryId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;

            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");

            return View(data);
        }
        public ActionResult DetailsGames(int id)
        {
            var game = db.Games.Include("Category").FirstOrDefault(g => g.GameId == id);
            if (game == null)
            {
                return HttpNotFound();
            }
            int userId = 1;
            ViewBag.CartQuantity = db.CartItems.Where(c => c.UserId == userId).Sum(c => (int?)c.Quantity) ?? 0;
            if (game == null) return HttpNotFound();

            // Chỉ rõ tên View là "DetailsGame"
            return View("DetailsGames", game);
        }

        //
        
        public ActionResult BuyNow(int id)
        {
            var game = db.Games.Find(id);
            if (game == null)
            {
                return HttpNotFound();
            }

            return View(game);
        }

        [HttpPost]
        
        public ActionResult ConfirmBuy(int gameId, decimal price, int quantity)
        {
            // Lấy user từ session (giả sử bạn đã lưu UserId trong Session khi đăng nhập)



            int userId;

            if (Session["UserId"] == null || !int.TryParse(Session["UserId"].ToString(), out userId))
            {
                // Nếu không có userId trong session → yêu cầu đăng nhập lại
                return RedirectToAction("Login", "Users_64130855");
            }

            // Kiểm tra user có tồn tại trong bảng Users không
            var user = db.Users.Find(userId);
            if (user == null)
            {
                return Content("Lỗi: Người dùng không tồn tại.");
            }


            // Tạo đơn hàng
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = price * quantity,
                Status = "Đã thanh toán"
            };

            db.Orders.Add(order);
            db.SaveChanges(); // để lấy OrderId

            // Tạo chi tiết đơn hàng
            var orderDetail = new OrderDetail
            {
                OrderId = order.OrderId,
                GameId = gameId,
                Quantity = quantity,
                UnitPrice = price
            };

            db.OrderDetails.Add(orderDetail);
            db.SaveChanges();

            TempData["Success"] = "Bạn đã mua game thành công!";
            return RedirectToAction("PurchaseSuccess", "Users_64130855");
        }
        //
        public ActionResult PurchaseSuccess()
        {
            // Tạo mã kích hoạt game ngẫu nhiên (8 ký tự, thêm tiền tố GAME-)
            string gameCode = "GAME-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

            // Gửi thông báo thành công và mã code qua TempData
            TempData["Success"] = "Bạn đã mua hàng thành công!";
            TempData["GameCode"] = gameCode;

            // Trả về View PurchaseSuccess.cshtml
            return View();
        }



        [ChildActionOnly]
        public ActionResult CartIcon()
        {
            int userId = 1; // giả lập UserId từ session
            int totalQuantity = db.CartItems
                .Where(c => c.UserId == userId)
                .Sum(c => (int?)c.Quantity) ?? 0;

            ViewBag.CartQuantity = totalQuantity;
            return PartialView("_CartIcon");
        }

        [HttpPost]
        public ActionResult AddToCart(int gameId, int quantity)
        {
            int userId = 1; // giả lập userId, sau này bạn có thể lấy từ session hoặc auth

            var existingItem = db.CartItems.FirstOrDefault(c => c.GameId == gameId && c.UserId == userId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                db.CartItems.Add(new CartItem
                {
                    GameId = gameId,
                    Quantity = quantity,
                    UserId = userId
                });
            }

            db.SaveChanges();

            // Sau khi thêm có thể chuyển về trang giỏ hàng hoặc quay lại trang game chi tiết
            return RedirectToAction("DetailsGames", new { id = gameId });
        }




        //
        public ActionResult CartGames()
        {
            // Giả sử UserId lấy từ Session hoặc Auth
            int userId = 1;
            var cartItems = db.CartItems
                              .Where(c => c.UserId == userId)
                              .Include("Game")
                              .ToList();

            return View(cartItems);
        }

        // Nút thanh toán   
        [HttpPost]
        public ActionResult Checkout()
        {
            int userId = 1; // giả sử lấy từ session

            var cartItems = db.CartItems.Where(c => c.UserId == userId).ToList();

            if (!cartItems.Any())
            {
                TempData["Message"] = "Giỏ hàng trống!";
                return RedirectToAction("CartGames");
            }

            // Tạo đơn hàng
            Order order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = cartItems.Sum(i => i.Game.Price * i.Quantity)
            };

            db.Orders.Add(order);
            db.SaveChanges();

            // Tạo chi tiết đơn hàng
            foreach (var item in cartItems)
            {
                OrderDetail detail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    GameId = item.GameId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Game.Price
                };
                db.OrderDetails.Add(detail);
            }

            // Xóa giỏ hàng sau khi đặt hàng
            db.CartItems.RemoveRange(cartItems);
            db.SaveChanges();

            TempData["Message"] = "Đặt hàng thành công!";
            return RedirectToAction("CartGames");
        }



        // Xóa game khỏi giỏ hàng
        [HttpPost]
        public ActionResult RemoveFromCart(int gameId)
        {
            int userId = 1; // sau này lấy từ session

            var item = db.CartItems.FirstOrDefault(c => c.UserId == userId && c.GameId == gameId);
            if (item != null)
            {
                db.CartItems.Remove(item);
                db.SaveChanges();
            }

            return RedirectToAction("CartGames");
        }


        // Cập nhật số lượng trong giỏ hàng (nếu cần)
        [HttpPost]
        public ActionResult UpdateQuantity(int cartItemId, int quantity)
        {
            var cartItem = db.CartItems.Find(cartItemId);
            if (cartItem != null && quantity > 0)
            {
                cartItem.Quantity = quantity;
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
        //PROFILE


        // GET: /Customer/
        public ActionResult ProfileUser()
        {
            var email = Session["Email"]?.ToString();

            if (string.IsNullOrEmpty(email))
            {
                // Chưa đăng nhập hoặc session bị mất
                return RedirectToAction("ProfileUser", "Users_64130855");
            }

            var user = db.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                // Không tìm thấy user phù hợp trong DB
                return HttpNotFound("Không tìm thấy người dùng.");
            }

            return View(user);
        }
        //


        // GET: Users_64130855/EditProfileUser/1
        public ActionResult EditProfileUser(int? id)
        {

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.Users.FirstOrDefault(u => u.UserId == id);

            if (user == null)
                return HttpNotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfileUser(User user, string oldPassword, string newPassword)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = db.Users.Find(user.UserId);
                    if (existingUser == null) return HttpNotFound();

                    existingUser.FullName = user.FullName;

                    // Nếu có yêu cầu đổi mật khẩu
                    if (!string.IsNullOrEmpty(newPassword))
                    {
                        // Kiểm tra mật khẩu cũ có khớp không
                        if (existingUser.Password != oldPassword)
                        {
                            ViewBag.PasswordError = "❌ Mật khẩu hiện tại không đúng.";
                            return View(user);
                        }

                        existingUser.Password = newPassword;
                    }

                    db.SaveChanges();
                    return RedirectToAction("ProfileUser");
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                            System.Diagnostics.Debug.WriteLine($"Lỗi ở: {validationError.PropertyName} - {validationError.ErrorMessage}");
                        }
                    }
                }
            }

            return View(user);
        }




        // Hiển thị lịch sử mua hàng của user
        public ActionResult OrderHistory()
        {
            int userId = 1; // Thay bằng cách lấy UserId thật từ session/auth
            var orders = db.Orders
                           .Where(o => o.UserId == userId)
                           .Include(o => o.OrderDetails.Select(od => od.Game))
                           .OrderByDescending(o => o.OrderDate)
                           .ToList();

            return View(orders);
        }
    }
}

