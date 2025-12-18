using DShop2024.Models;
using DShop2024.Models.Blog;
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

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        base.OnConfiguring(builder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);



        modelBuilder.Entity<BrandModel>(entity =>
        {
            entity.HasIndex(c => c.Slug).IsUnique();
        });


        modelBuilder.Entity<CategoryModel>(entity =>
        {
            entity.HasIndex(c => c.Slug).IsUnique();
        });

        modelBuilder.Entity<ProductModel>(entity =>
        {
            entity.HasIndex(c => c.Slug).IsUnique();
        });

        modelBuilder.Entity<ShippingModel>(entity =>
        {
            entity.HasIndex(c => c.Province).IsUnique();
        });

        modelBuilder.Entity<PromotionModel>(entity =>
        {
            entity.HasIndex(c => c.CategoryCouponName).IsUnique();
        });

        modelBuilder.Entity<OrderModel>(entity =>
        {
            entity.HasIndex(c => c.OrderCode).IsUnique();
        });

        modelBuilder.Entity<FAQModel>(entity =>
        {
            entity.HasIndex(c => c.Question).IsUnique();
        });

        modelBuilder.Entity<CouponModel>(entity =>
        {
            entity.HasIndex(c => c.CouponCode).IsUnique();
        });

        modelBuilder.Entity<PostModel>(entity =>
        {
            entity.HasIndex(c => c.Slug).IsUnique();
        });

        modelBuilder.Entity<SubjectModel>(entity =>
        {
            entity.HasIndex(c => c.Slug).IsUnique();
        });

        modelBuilder.Entity<PaymentModel>(entity =>
        {
            entity.HasIndex(c => c.PaymentName).IsUnique();
        });

        modelBuilder.Entity<LikeModel>(entity =>
        {
            entity.HasKey(c => new { c.PostId, c.UserId });
        });
        modelBuilder.Entity<RefundModel>(entity =>
        {
            entity.HasIndex(c => c.OrderCode).IsUnique();
        });
        modelBuilder.Entity<RefundModel>(entity =>
        {
            entity.HasIndex(c => c.TransactionId).IsUnique();
        });

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
		public virtual DbSet<MessageModel> Messages { get; set; }
		public virtual DbSet<InformationShopModel> InformationShops { get; set; }
		public virtual DbSet<CouponRedemptionModel> CouponRedemptions { get; set; }
		public virtual DbSet<OrderCouponsModel> OrderCouponss { get; set; }
		public virtual DbSet<PromotionModel> Promotions { get; set; }
		public virtual DbSet<FAQModel> FAQs { get; set; }
		public virtual DbSet<ProductImageModel> ProductImages { get; set; }
		public virtual DbSet<PostModel> Posts { get; set; }
		public virtual DbSet<PostSubjectModel> PostSubjects { get; set; }
		public virtual DbSet<SubjectModel> Subjects { get; set; }
        public virtual DbSet<PolicyModel> Policies { get; set; }
        public virtual DbSet<PaymentModel> Payments { get; set; }
        public virtual DbSet<CommentModel> Comments { get; set; }
        public virtual DbSet<LikeModel> Likes { get; set; }
        public virtual DbSet<TaskModel> Tasks { get; set; }
        public virtual DbSet<AssignmentModel> Assignments { get; set; }
        public virtual DbSet<ReturnModel> Returns { get; set; }
        public virtual DbSet<ReturnDetailModel> ReturnDetails { get; set; }
        public virtual DbSet<RefundModel> Refunds { get; set; }

}

