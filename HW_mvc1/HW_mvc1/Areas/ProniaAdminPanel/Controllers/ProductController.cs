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

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid)
            {
                return View(product);
            }

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == product.Category.Name);
            if (category == null)
            {
                ModelState.AddModelError("Category.Name", "Category not found");
                return View(product);
            }

            var size = await _context.Sizes.FirstOrDefaultAsync(s => s.Name == product.Size.Name);
            if (size == null)
            {
                ModelState.AddModelError("Size.Name", "Size not found");
                return View(product);
            }

            bool productExists = await _context.Products.AnyAsync(x => x.Name.Trim() == product.Name.Trim());
            if (productExists)
            {
                ModelState.AddModelError("Name", "Product with this name already exists");
                return View(product);
            }

            product.CategoryId = category.Id;
            product.SizeId = size.Id;
            product.CreatedTime = DateTime.Now;

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.Include(p => p.Category).Include(p => p.Size).ToListAsync();
            return View(products);
        }
    }
}
