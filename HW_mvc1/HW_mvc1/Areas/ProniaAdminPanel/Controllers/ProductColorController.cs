using Microsoft.AspNetCore.Mvc;

namespace HW_mvc1.Areas.ProniaAdminPanel.Controllers
{
	[Area("ProniaAdminPanel")]
	public class ProductColorController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
