﻿namespace HW_mvc1.Models
{
	public class ProductTag : BaseEntity
	{
		public Product Product { get; set; }
		public Tag Tag { get; set; }
		public int TagId { get; set; }
		public int ProductId { get; set; }

	}
}
