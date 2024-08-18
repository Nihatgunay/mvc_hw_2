using HW_mvc1.DAL;
using HW_mvc1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HW_mvc1.Areas.ProniaAdminPanel.Controllers
{
	[Area("ProniaAdminPanel")]
	public class SizeController : Controller
	{
        private readonly AppDbContext _context;

        public SizeController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<Size> sizes = await _context.Sizes.Include(c => c.Products).ToListAsync();

            return View(sizes);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Size size)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            bool result = await _context.Sizes.AnyAsync(x => x.Name.Trim() == size.Name.Trim());

            if (result)
            {
                ModelState.AddModelError("Name", "name exists");
                return View();
            }

            size.CreatedTime = DateTime.Now;
            await _context.Sizes.AddAsync(size);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
