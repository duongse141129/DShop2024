using DShop2024.Models;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Design;

namespace DShop2024.Repository
{
	public class SeedData
	{
		public static void SeedingData(DShopContext _context)
		{
			_context.Database.Migrate();
			if (!_context.Products.Any())
			{
				CategoryModel duffle = new CategoryModel { CategoryName = "Duffle Bag", Slug = "dufflebag", Description = " Duffle Bag is a long bag used for carrying clothes ", Status = 1 };
				CategoryModel hikingBackpacks = new CategoryModel { CategoryName = "Hiking Backpacks", Slug = "hikingbackpacks ", Description = "Hiking Backpacks  is an outdoor recreation where gear is carried in a backpack ", Status = 1 };

				BrandModel brean = new BrandModel { BrandName = "L.L.Bean", Slug = "llbean", Description = "L.L.Bean is large brand in the world", Status = 1 };
				BrandModel canvas = new BrandModel { BrandName = "Canvas", Slug = "canvas", Description = "Canvas is large brand in the world", Status = 1 };

				_context.Products.AddRange(
					new ProductModel { ProductName = "Waxed Canvas Duffle", Slug = "waxedcanvasduffle", Description = "We built our classic zippered duffle with real waxed canvas fabric for a rugged feel that just gets better with age. Meets most airline carry-on requirements.", Image = "1.jpg", Category = duffle, Brand = canvas, Price = 1233, Status=1 },
					new ProductModel { ProductName = "L.L.Bean Adventure Pack", Slug = "llbeanadventurepack", Description = "We built our classic zippered duffle with real waxed canvas fabric for a rugged feel that just gets better with age. Meets most airline carry-on requirements.", Image = "1.jpg", Category = hikingBackpacks, Brand = brean, Price = 1200, Status = 1}

				);

				_context.SaveChanges();
			}
			

		}
	}
}
