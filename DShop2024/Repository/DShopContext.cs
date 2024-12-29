﻿using DShop2024.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


	public class DShopContext : IdentityDbContext<AppUserModel>
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
		public virtual DbSet<OrderModel> Orders { get; set; }
		public virtual DbSet<OrderDetailModel> OrderDetails { get; set; }
		public virtual DbSet<RatingModel> Ratings { get; set; }
		public virtual DbSet<BannerModel> Banners { get; set; }
		public virtual DbSet<ContactModel> Contacts { get; set; }
		public virtual DbSet<WishListModel> WishLists { get; set; }
		public virtual DbSet<CompareModel> Compares { get; set; }
		public virtual DbSet<ReceivingStockModel> ReceivingStocks { get; set; }
		public virtual DbSet<ShippingModel> Shippings { get; set; }
		public virtual DbSet<CouponModel> Coupons { get; set; }
		public virtual DbSet<StatisticalModel> Statisticals { get; set; }
	}

