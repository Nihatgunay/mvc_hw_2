using HW_mvc1.DAL;
using HW_mvc1.Models;
using HW_mvc1.Utilities.Enums;
using HW_mvc1.Utilities.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HW_mvc1.Areas.ProniaAdminPanel.Controllers
{
	[Area("ProniaAdminPanel")]
	public class SlideController : Controller
	{
		private readonly AppDbContext _context;
		private readonly IWebHostEnvironment _env;

		public SlideController(AppDbContext context, IWebHostEnvironment env)
        {
			_context = context;
			_env = env;
		}
        public async Task<IActionResult> Index()
		{
			List<Slide> slides = await _context.Slides.Where(x => !x.IsDeleted).ToListAsync();
			return View(slides);
		}

		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(Slide slide)
		{
			//if(!ModelState.IsValid) return View();

			if (!slide.Photo.ValidateType("image/"))
			{
				ModelState.AddModelError("Photo", "File type isnt correct");
				return View();
			}

			if(!slide.Photo.ValidateSize(FileSize.MB, 2))
			{
				ModelState.AddModelError("Photo", "File size must be <= 2mb");
				return View();
			}

			
			slide.ImageUrl = await slide.Photo.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images");


			slide.CreatedTime = DateTime.Now;
			await _context.Slides.AddAsync(slide);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}
	}
}
