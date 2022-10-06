using AllUp.DAL;
using AllUp.Helpers;
using AllUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllUp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        public CategoriesController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            List<Category> categories = await _db.Categories.Include(x => x.Children).Include(x => x.Parent).ToListAsync();
            return View(categories);
        }
        public async Task<IActionResult> Create()
        {
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).Where(x => !x.IsDeactive).ToListAsync();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category,int? mainCatId)
        {
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).Where(x => !x.IsDeactive).ToListAsync();

            if(category.IsMain)
            {
                bool isExist = await _db.Categories.AnyAsync(x => x.Parent.Name == category.Name);
                if (isExist)
                {
                    ModelState.AddModelError("Name", "This category is already exist");
                    return View();
                }
                if (category.Photo == null)
                {
                    ModelState.AddModelError("Photo", "Please choose an image");
                    return View();
                }
                if (!category.Photo.IsImage())
                {
                    ModelState.AddModelError("Photo", "Please choose the image file");
                    return View();
                }
                if (category.Photo.IsOlder1MB())
                {
                    ModelState.AddModelError("Photo", "Please choose Max 1mb image file");
                    return View();
                }
                string folder = Path.Combine(_env.WebRootPath, "assets", "images");
                category.Image = await category.Photo.SaveFileAsync(folder);
            }
            else
            {
                if (mainCatId == null)
                {
                    return NotFound();
                }
                category.ParentId = mainCatId;
            }
            await _db.Categories.AddAsync(category);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");

        }
        public async Task<IActionResult> Update(int? id)
        {
            ViewBag.Categories = await _db.Categories.Where(x => !x.IsDeactive).ToListAsync();
            if (id == null)
            {
                return NotFound();
            }
            Category dbCategory = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (dbCategory == null)
            {
                return BadRequest();
            }
            return View(dbCategory);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, Category category, int mainCatId)
        {
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).Where(x => !x.IsDeactive).ToListAsync();
            if (id == null)
            {
                return NotFound();
            }
            Category dbCategory = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (dbCategory == null)
            {
                return BadRequest();
            }
            if (dbCategory.IsMain)
            {
                bool isExist = await _db.Categories.AnyAsync(x => x.Parent.Name == category.Name);
                if (isExist)
                {
                    ModelState.AddModelError("Name", "This category is already exist");
                    return View();
                }
                if (category.Photo != null)
                {
                    if (!category.Photo.IsImage())
                    {
                        ModelState.AddModelError("Photo", "Please choose the image file");
                        return View();
                    }
                    if (category.Photo.IsOlder1MB())
                    {
                        ModelState.AddModelError("Photo", "Please choose Max 1mb image file");
                        return View();
                    }
                    string folder = Path.Combine(_env.WebRootPath, "assets", "images");
                    dbCategory.Image = await category.Photo.SaveFileAsync(folder);
                    
                }
                
            }
            else
            {
                if (mainCatId == null)
                {
                    return NotFound();
                }
                dbCategory.ParentId = mainCatId;
                
            }
            dbCategory.Name = category.Name;
            
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Activity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Category category = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (category == null)
            {
                return BadRequest();
            }
            if (category.IsDeactive)
            {
                category.IsDeactive = false;
            }
            else
            {
                category.IsDeactive = true;

            }
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");

        }
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Category? category =await _db.Categories.FirstOrDefaultAsync(x=>x.Id==id);
            if (category==null)
            {
                return BadRequest();
            }
            return View(category);
        }
    }

}
