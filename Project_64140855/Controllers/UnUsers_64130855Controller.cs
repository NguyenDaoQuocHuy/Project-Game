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
    public class UnUsers_64130855Controller : Controller
    {
        private Project_64130855Entities db = new Project_64130855Entities();
        // GET: UnUsers_64130855
        public ActionResult GIOITHIEU_64130855()
        {
            return View();
        }
        //
        public ActionResult Support()
        {
            return View();
        }
        public ActionResult Index(string search, int? categoryId, decimal? minPrice, decimal? maxPrice, int page = 1, int pageSize = 8)
        {
            var games = db.Games.Include(g => g.Category).AsQueryable();

            if (!string.IsNullOrEmpty(search))
                games = games.Where(g => g.Title.Contains(search));

            if (categoryId.HasValue)
                games = games.Where(g => g.CategoryId == categoryId.Value);

            if (minPrice.HasValue)
                games = games.Where(g => g.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                games = games.Where(g => g.Price <= maxPrice.Value);

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
    }
}