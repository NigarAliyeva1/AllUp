using AllUp.DAL;
using AllUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllUp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _db;
        public CategoriesController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            List<Category> categories = await _db.Categories.Include(x => x.Children).Include(x => x.Parent).ToListAsync();
            return View(categories);
        }
        public IActionResult Create()
        {
            return View();
        }
        //public async Task<IActionResult> Activity(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }
        //    Category category = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
        //    if (category == null)
        //    {
        //        return BadRequest();
        //    }
        //    if (category.IsDeactive)
        //    {
        //        category.IsDeactive = false;
        //    }
        //    else
        //    {
        //        category.IsDeactive = true;

        //    }
        //    await _db.SaveChangesAsync();
        //    return RedirectToAction("Index");

        //}
    }

}
