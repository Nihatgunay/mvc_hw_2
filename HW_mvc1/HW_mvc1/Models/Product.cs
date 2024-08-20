namespace HW_mvc1.Models
{
    public class Product:BaseEntity
    {
		public string Name { get; set; }
		public string Description { get; set; }
		public decimal Price { get; set; }
		public string SKU { get; set; }

		public int CategoryId { get; set; }
		public Category Category { get; set; }

		public int SizeId { get; set; }
		public Size Size { get; set; }

        public int ColorId { get; set; }
        public Color color { get; set; }

        public int TagId { get; set; }
        public Tag tag { get; set; }

        public ICollection<ProductImage> ProductImages { get; set; }
		

	}
}
