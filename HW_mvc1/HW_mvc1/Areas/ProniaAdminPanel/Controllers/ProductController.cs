using System.Collections.Immutable;
using HW_mvc1.Areas.ProniaAdminPanel.ViewModels;
using HW_mvc1.DAL;
using HW_mvc1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HW_mvc1.Areas.ProniaAdminPanel.Controllers
{
    [Area("ProniaAdminPanel")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            List<GetAdminProductVM> products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages.Where(pi=>pi.IsPrimary == true))
                .Select(p=> new GetAdminProductVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryName = p.Category.Name,
                    Image = p.ProductImages.FirstOrDefault().ImageUrl
                })
                .ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            CreateProductVM productVM = new CreateProductVM
            {
				Categories = await _context.Categories.Where(x => x.IsDeleted == false).ToListAsync()
		    };
            return View(productVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            productVM.Categories = await _context.Categories.Where(x => x.IsDeleted == false).ToListAsync();

            if (!ModelState.IsValid)
            {

                return View(productVM);
            }

            bool result = await _context.Categories.AnyAsync(x => x.Id == productVM.CategoryId && x.IsDeleted == false);
            if (!result)
            {
                ModelState.AddModelError("CategoryId", "Category doesnt exist");

                return View(productVM);
            }
            Product product = new Product
            {
                CategoryId = productVM.CategoryId.Value,
                SKU = productVM.SKU,
                Description = productVM.Description,
                Name = productVM.Name,
                Price = productVM.Price,
                CreatedTime = DateTime.Now,

            };
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id <= 0) return BadRequest();

            Product product = await _context.Products.FirstOrDefaultAsync(x => x.Id ==  id && x.IsDeleted == false);
            if (product == null) return NotFound();

            UpdateProductVM productVM = new UpdateProductVM
            {
                Name = product.Name,
                CategoryId = product.CategoryId,
                SKU = product.SKU,
                Description = product.Description,
                Price = product.Price,
                Categories = await _context.Categories.Where(x => !x.IsDeleted).ToListAsync()
            };

            return View(productVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateProductVM productVM)
        {
            if (id == null || id <= 0) return BadRequest();

            Product existed = await _context.Products.FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == false);
            if (existed == null) return NotFound();

            productVM.Categories = await _context.Categories.Where(x => !x.IsDeleted).ToListAsync();
            if (!ModelState.IsValid)
            {
                return View(productVM);
            }
            if(existed.CategoryId != productVM.CategoryId)
            {
                bool result = await _context.Categories.AnyAsync(x => x.Id == productVM.CategoryId && x.IsDeleted == false);
                if (!result)
                {
                    ModelState.AddModelError("CategoryId", "category does not exist");
                    return View(productVM);
                }
            }

            return View(productVM);
        }
    }
}
