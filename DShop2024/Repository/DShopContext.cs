using DShop2024.Models;
using Microsoft.EntityFrameworkCore;


	public class DShopContext : DbContext
	{
		public DShopContext()
		{

		}
		public DShopContext(DbContextOptions<DShopContext> options) : base(options)
		{

		}

		public virtual DbSet<BrandModel> Brands { get; set; }
		public virtual DbSet<ProductModel> Products { get; set; }
		public virtual DbSet<CategoryModel> Categories { get; set; }
	}

