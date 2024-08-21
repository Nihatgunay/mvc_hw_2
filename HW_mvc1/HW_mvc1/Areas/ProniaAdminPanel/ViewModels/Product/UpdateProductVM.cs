using System.ComponentModel.DataAnnotations;
using HW_mvc1.Models;

namespace HW_mvc1.Areas.ProniaAdminPanel.ViewModels
{
    public class UpdateProductVM
    {
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public int? CategoryId { get; set; }
        public decimal Price { get; set; }
        public string SKU { get; set; }
        public List<Category>? Categories { get; set; }
		public List<Tag>? Tags { get; set; }
		public List<int>? TagIds { get; set; }
	}
}
