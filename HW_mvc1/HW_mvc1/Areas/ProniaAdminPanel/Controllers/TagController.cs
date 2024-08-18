using HW_mvc1.DAL;
using HW_mvc1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HW_mvc1.Areas.ProniaAdminPanel.Controllers
{
	[Area("ProniaAdminPanel")]
	public class TagController : Controller
	{
        private readonly AppDbContext _context;

        public TagController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<Tag> tags = await _context.Tags.Include(c => c.ProductTags).ToListAsync();

            return View(tags);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Tag tag)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            bool result = await _context.Tags.AnyAsync(x => x.Name.Trim() == tag.Name.Trim());

            if (result)
            {
                ModelState.AddModelError("Name", "name exists");
                return View();
            }

            tag.CreatedTime = DateTime.Now;
            await _context.Tags.AddAsync(tag);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
