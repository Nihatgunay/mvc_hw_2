using HW_mvc1.DAL;
using HW_mvc1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HW_mvc1.Areas.ProniaAdminPanel.Controllers
{
	[Area("ProniaAdminPanel")]
	public class ProductTagController : Controller
	{
		private readonly AppDbContext _context;

		public ProductTagController(AppDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index()
		{
			List<ProductTag> prdtag = await _context.ProductTags
				.Include(c => c.Product)
				.Include(c => c.Tag)
				.ToListAsync();

			return View(prdtag);
		}

		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(ProductTag prdtag2)
		{
			if (!ModelState.IsValid)
			{
				return View();
			}

			bool exists = await _context.ProductTags
				.AnyAsync(x => x.ProductId == prdtag2.ProductId && x.TagId == prdtag2.TagId);

			if (exists)
			{
				ModelState.AddModelError("ProductId", "This product is already with the selected tag.");
				return View();
			}

			prdtag2.CreatedTime = DateTime.Now;
			await _context.ProductTags.AddAsync(prdtag2);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}
	}
}
