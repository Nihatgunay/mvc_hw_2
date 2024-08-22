using System.Collections.Immutable;
using HW_mvc1.Areas.ProniaAdminPanel.ViewModels;
using HW_mvc1.DAL;
using HW_mvc1.Models;
using HW_mvc1.Utilities.Enums;
using HW_mvc1.Utilities.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;

namespace HW_mvc1.Areas.ProniaAdminPanel.Controllers
{
    [Area("ProniaAdminPanel")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
		private readonly IWebHostEnvironment _env;

		public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
			_env = env;
		}

        public async Task<IActionResult> Index()
        {
            List<GetAdminProductVM> products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Color)
                .Include(p => p.Size)
				.Include(p => p.ProductImages.Where(pi=>pi.IsPrimary == true))
                .Select(p=> new GetAdminProductVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryName = p.Category.Name,
                    ColorName = p.Color.Name,
                    SizeName = p.Size.Name,
                    Image = p.ProductImages.FirstOrDefault().ImageUrl
                })
                .ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            CreateProductVM productVM = new CreateProductVM
            {
				Categories = await _context.Categories.Where(x => x.IsDeleted == false).ToListAsync(),
				Colors = await _context.Colors.Where(x => x.IsDeleted == false).ToListAsync(),
				Sizes = await _context.Sizes.Where(x => x.IsDeleted == false).ToListAsync(),
				Tags = await _context.Tags.Where(x => x.IsDeleted == false).ToListAsync()
			};
            return View(productVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM productVM)
        {
            productVM.Categories = await _context.Categories.Where(x => x.IsDeleted == false).ToListAsync();
            productVM.Colors = await _context.Colors.Where(x => x.IsDeleted == false).ToListAsync();
            productVM.Sizes = await _context.Sizes.Where(x => x.IsDeleted == false).ToListAsync();
			productVM.Tags = await _context.Tags.Where(x => x.IsDeleted == false).ToListAsync();

			if (!ModelState.IsValid)
            {

                return View(productVM);
            }

            if (!productVM.MainPhoto.ValidateType("image/"))
            {
				ModelState.AddModelError("MainPhoto", "image type incorrect");

				return View(productVM);
            }
			if (!productVM.MainPhoto.ValidateSize(FileSize.MB, 2))
			{
				ModelState.AddModelError("MainPhoto", "image size incorrect( <= 2mb)");

				return View(productVM);
			}

			if (!productVM.HoverPhoto.ValidateType("image/"))
			{
				ModelState.AddModelError("HoverPhoto", "image type incorrect");

				return View(productVM);
			}
			if (!productVM.HoverPhoto.ValidateSize(FileSize.MB, 2))
			{
				ModelState.AddModelError("HoverPhoto", "image size incorrect( <= 2mb)");

				return View(productVM);
			}

			bool result = await _context.Categories.AnyAsync(x => x.Id == productVM.CategoryId && x.IsDeleted == false);
            if (!result)
            {
                ModelState.AddModelError("CategoryId", "Category doesnt exist");

                return View(productVM);
            }

			bool resultcolor = await _context.Colors.AnyAsync(x => x.Id == productVM.ColorId && x.IsDeleted == false);
			if (!resultcolor)
			{
				ModelState.AddModelError("ColorId", "color doesnt exist");

				return View(productVM);
			}

			bool resultsize = await _context.Sizes.AnyAsync(x => x.Id == productVM.SizeId && x.IsDeleted == false);
			if (!resultsize)
			{
				ModelState.AddModelError("SizeId", "size doesnt exist");

				return View(productVM);
			}

			if (productVM.TagIds is not null)
            {
				bool tagresult = productVM.TagIds.Any(tid => !productVM.Tags.Exists(t => t.Id == tid));
				if (tagresult)
				{
					ModelState.AddModelError("TagIds", "tag doesnt exist");
					return View(productVM);
				}
			}

            ProductImage mainimg = new ProductImage
            {
                CreatedTime = DateTime.Now,
                IsPrimary = true,
                ImageUrl = await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images")
            };

			ProductImage hoverimg = new ProductImage
			{
				CreatedTime = DateTime.Now,
				IsPrimary = true,
				ImageUrl = await productVM.MainPhoto.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images")
			};

			Product product = new Product
            {
                CategoryId = productVM.CategoryId.Value,
                ColorId = productVM.ColorId.Value,
                SizeId = productVM.SizeId.Value,
				SKU = productVM.SKU,
                Description = productVM.Description,
                Name = productVM.Name,
                Price = productVM.Price,
                CreatedTime = DateTime.Now,
                ProductImages = new List<ProductImage> { mainimg, hoverimg }
            };

            if(productVM.Photos is not null)
            {
				string text = string.Empty;
				foreach (IFormFile file in productVM.Photos)
				{
					if (!file.ValidateType("image/"))
					{
						text += $"{file.Name} file type incorrect";
						continue;
					}
					if (!file.ValidateSize(FileSize.MB, 2))
					{
						text += $"{file.Name} file size incorrect";
						continue;
					}
					ProductImage img = new ProductImage
					{
						CreatedTime = DateTime.Now,
						IsPrimary = null,
						ImageUrl = await file.CreateFileAsync(_env.WebRootPath, "assets", "images", "website-images")
					};
					product.ProductImages.Add(img);
				}
				TempData["ErrorMessage"] = text;
			}

            if(productVM.TagIds is not null)
            {
                product.ProductTags = productVM.TagIds.Select(tid => new ProductTag
                {
                    TagId = tid

                }).ToList();

			}

            //instead of select you can write:
            //foreach (var tagid in productVM.TagIds)
            //{
            //    ProductTag prodtag = new ProductTag
            //    {
            //        TagId = tagid,
            //        Product = product
            //    };
            //    _context.ProductTags.Add(prodtag);
            //}

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id <= 0) return BadRequest();

            Product product = await _context.Products.Include(x => x.ProductTags).FirstOrDefaultAsync(x => x.Id ==  id && x.IsDeleted == false);
            if (product == null) return NotFound();

            UpdateProductVM productVM = new UpdateProductVM
            {
                Name = product.Name,
                CategoryId = product.CategoryId,
                ColorId = product.ColorId,
                SizeId = product.SizeId,
				SKU = product.SKU,
                Description = product.Description,
                Price = product.Price,
                Categories = await _context.Categories.Where(x => !x.IsDeleted).ToListAsync(),
                Colors = await _context.Colors.Where(x => !x.IsDeleted).ToListAsync(),
                Sizes = await _context.Sizes.Where(x => !x.IsDeleted).ToListAsync(),
				Tags = await _context.Tags.Where(x => !x.IsDeleted).ToListAsync(),
                TagIds = product.ProductTags.Select(pt => pt.TagId).ToList()
			};

            return View(productVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateProductVM productVM)
        {
            if (id == null || id <= 0) return BadRequest();

            Product existed = await _context.Products.Include(x => x.ProductTags).FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == false);
            if (existed == null) return NotFound();

            productVM.Categories = await _context.Categories.Where(x => !x.IsDeleted).ToListAsync();
            productVM.Tags = await _context.Tags.Where(x => !x.IsDeleted).ToListAsync();
            productVM.Colors = await _context.Colors.Where(x => !x.IsDeleted).ToListAsync();
            productVM.Sizes = await _context.Sizes.Where(x => !x.IsDeleted).ToListAsync();
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
			if (existed.ColorId != productVM.ColorId)
			{
				bool result2 = await _context.Colors.AnyAsync(x => x.Id == productVM.ColorId && x.IsDeleted == false);
				if (!result2)
				{
					ModelState.AddModelError("ColorId", "color does not exist");
					return View(productVM);
				}
			}
			if (existed.SizeId != productVM.SizeId)
			{
				bool result3 = await _context.Sizes.AnyAsync(x => x.Id == productVM.SizeId && x.IsDeleted == false);
				if (!result3)
				{
					ModelState.AddModelError("SizeId", "size does not exist");
					return View(productVM);
				}
			}

			_context.ProductTags.RemoveRange(existed.ProductTags.Where(pt => !productVM.TagIds.Exists(tid => tid == pt.Id)).ToList());
            existed.ProductTags.AddRange(productVM.TagIds.Where(tid => existed.ProductTags.Any(pt => pt.TagId == tid)).Select(tId => new ProductTag { TagId=tId}));

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
