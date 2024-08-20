using System.ComponentModel.DataAnnotations;

namespace HW_mvc1.Models
{
	public class Tag : BaseEntity
	{
		
        public string Name { get; set; }
        public ICollection<Product>? Products { get; set; }

    }
}
