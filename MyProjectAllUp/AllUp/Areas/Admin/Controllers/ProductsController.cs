using AllUp.DAL;
using AllUp.Helpers;
using AllUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllUp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        public ProductsController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            List<Product> products = await _db.Products.Include(x => x.ProductDeatil).Include(x => x.ProductImages).Include(x => x.ProductCategories).ThenInclude(x => x.Category).ToListAsync();
            return View(products);
        }

        #region Create
        public async Task<IActionResult> Create()
        {
            Category? mainCategory = await _db.Categories.Include(x => x.Children).FirstOrDefaultAsync(x => x.IsMain);
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();
            ViewBag.ChildCategories = mainCategory.Children;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, int? mainCatId, int? childCatId)
        {
            if (mainCatId == null)
            {
                return NotFound();
            }
            if (childCatId == null)
            {
                return NotFound();
            }
            Category? mainCategory = await _db.Categories.Include(x => x.Children).FirstOrDefaultAsync(x => x.IsMain);
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();
            ViewBag.ChildCategories = mainCategory.Children;
            if (product.Photos == null)
            {
                ModelState.AddModelError("Photos", "Add a pic");
                return View();
            }
            if (mainCatId == null)
            {
                return BadRequest();
            }
            List<ProductImage> productImages = new List<ProductImage>();
            foreach (IFormFile Photo in product.Photos)
            {
                if (!Photo.IsImage())
                {
                    ModelState.AddModelError("Photo", "Please choose the image file");
                    return View();
                }
                if (Photo.IsOlder1MB())
                {
                    ModelState.AddModelError("Photo", "Please choose Max 1mb image file");
                    return View();
                }
                string folder = Path.Combine(_env.WebRootPath, "assets", "images", "product");
                ProductImage productImage = new ProductImage
                {
                    Image = await Photo.SaveFileAsync(folder)
                };
                productImages.Add(productImage);
            }

            List<ProductCategory> productCategories = new List<ProductCategory>();
            ProductCategory mainProductCategory = new ProductCategory
            {
                CategoryId = (int)mainCatId,

            };
            mainProductCategory.CategoryId = (int)mainCatId;
            productCategories.Add(mainProductCategory);
            if (childCatId != null)
            {
                ProductCategory childProductCategory = new ProductCategory
                {
                    CategoryId = (int)childCatId,

                };
                productCategories.Add(childProductCategory);
            }
            product.ProductCategories = productCategories;
            product.ProductImages = productImages;
            await _db.Products.AddAsync(product);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        #endregion

        #region LoadChild
        public async Task<IActionResult> LoadChild(int mainId)
        {
            List<Category> categories = await _db.Categories.Where(x => x.ParentId == mainId).ToListAsync();
            return PartialView("_LoadChildCategoriesPartial", categories);
        }
        #endregion
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Product? product = await _db.Products.Include(x => x.ProductDeatil).FirstOrDefaultAsync(x => x.Id == id);
            if (product == null)
            {
                return BadRequest();
            }
            return View(product);
        }
        public async Task<IActionResult> DeleteProductImage(int? proImgId)
        {
            if (proImgId == null)
            {
                return NotFound();
            }
            ProductImage productImage = await _db.ProductImages.Include(x => x.Product).ThenInclude(x => x.ProductImages).FirstOrDefaultAsync(x => x.Id == proImgId);
            int productImagesCount = productImage.Product.ProductImages.Count;

            string path = Path.Combine(_env.WebRootPath, "assets", "images", "product", productImage.Image);
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            _db.ProductImages.Remove(productImage);

            await _db.SaveChangesAsync();
            if (productImagesCount == 2)
            {
                return Content("Stop");
            }

            return Content("Ok");
        }
        public async Task<IActionResult> Update(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }
            Product? dbProduct = await _db.Products.
                Include(x => x.ProductCategories).ThenInclude(x => x.Category).ThenInclude(x => x.Children).
                Include(x => x.ProductImages).
                Include(x => x.ProductDeatil).
                FirstOrDefaultAsync(x => x.Id == id);
            if (dbProduct == null)
            {
                return BadRequest();
            }
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).Where(x => !x.IsDeactive).ToListAsync();

            return View(dbProduct);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, Product product, int? mainCatId, int? childCatId)
        {

            if (id == null)
            {
                return NotFound();
            }
            Product? dbProduct = await _db.Products.
                Include(x => x.ProductCategories).ThenInclude(x => x.Category).ThenInclude(x => x.Children).
                Include(x => x.ProductImages).
                Include(x => x.ProductDeatil).
                FirstOrDefaultAsync(x => x.Id == id);
            if (dbProduct == null)
            {
                return BadRequest();
            }
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).Where(x => !x.IsDeactive).ToListAsync();
            if (mainCatId == null)
            {
                return BadRequest();
            }
            List<ProductImage> productImages = new List<ProductImage>();
            foreach (IFormFile Photo in product.Photos)
            {
                if (!Photo.IsImage())
                {
                    ModelState.AddModelError("Photo", "Please choose the image file");
                    return View();
                }
                if (Photo.IsOlder1MB())
                {
                    ModelState.AddModelError("Photo", "Please choose Max 1mb image file");
                    return View();
                }
                string folder = Path.Combine(_env.WebRootPath, "assets", "images", "product");
                ProductImage productImage = new ProductImage
                {
                    Image = await Photo.SaveFileAsync(folder)
                };
                productImages.Add(productImage);
            }
            List<ProductCategory> productCategories = new List<ProductCategory>();
            ProductCategory mainProductCategory = new ProductCategory
            {
                CategoryId = (int)mainCatId,

            };
            mainProductCategory.CategoryId = (int)mainCatId;
            productCategories.Add(mainProductCategory);
            if (childCatId != null)
            {
                ProductCategory childProductCategory = new ProductCategory
                {
                    CategoryId = (int)childCatId,

                };
                productCategories.Add(childProductCategory);
            }
            dbProduct.ProductCategories = productCategories;
            dbProduct.ProductImages.AddRange(productImages);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
