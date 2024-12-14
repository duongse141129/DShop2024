﻿using DShop2024.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DShop2024.Models
{
	[Table("Product")]
	public class ProductModel
	{
		[Key]
		public int Id { get; set; }
		[Required, MinLength(4, ErrorMessage = "Name product's can not null")]
		public string ProductName { get; set; }
		public string Slug { get; set; }
		public string Image { get; set; }
		public string? Description { get; set; }
		[Required(ErrorMessage = "Price product's can not null ")]
		[Range(0, int.MaxValue, ErrorMessage = "Price > {1}")]
		[Column(TypeName ="decimal(8,2)")]
		public decimal Price { get; set; }

		[Required, Range(1, int.MaxValue, ErrorMessage ="Seclect a brand")]
		public int BrandId { get; set; }
        [Required, Range(1, int.MaxValue, ErrorMessage = "Seclect a brand")]
        public int CategoryId { get; set; }
		public BrandModel Brand { get; set; }
		public CategoryModel Category { get; set; }
		public int Status { get; set; }

		[NotMapped]
		[FileExtension]
		public IFormFile? ImageUpload { get; set; }
	}
}