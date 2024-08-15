using HW_mvc1.DAL;
using HW_mvc1.Models;
using HW_mvc1.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HW_mvc1.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            HomeVM homeVM = new HomeVM
            {
                Slides = _context.Slides.OrderBy(x => x.Order).Take(2).ToList(),
                Products = _context.Products.OrderByDescending(p=>p.CreatedTime).Take(8)
                .Include(x => x.ProductImages.Where(pi => pi.IsPrimary != null))
                .ToList(),
            };

            return View(homeVM);
        }  
    }
}
