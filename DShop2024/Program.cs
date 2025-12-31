using DShop2024.AutoMapper;
using DShop2024.Hubs;
using DShop2024.Models;
using DShop2024.Models.Momo;
using DShop2024.Services;
using DShop2024.Services.ChatboxAI;
using DShop2024.Services.Momo;
using DShop2024.Services.Recommend;
using DShop2024.Services.Vnpay;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions();
var mailsetting = builder.Configuration.GetSection("MailSettings");
builder.Services.Configure<MailSettings>(mailsetting);
builder.Services.AddSingleton<IEmailSender, SendMailService>();

builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();

builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<ChatBotService>();



builder.Services.AddDbContext<DShopContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectedDb"));
});

//builder.Services.AddDefaultIdentity<AppUserModel>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<DShopContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
});




//  Identity
builder.Services.AddIdentity<AppUserModel, IdentityRole>()
        .AddEntityFrameworkStores<DShopContext>()
        .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
	// Password settings.
	options.Password.RequireDigit = true;
	options.Password.RequireLowercase = true;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireUppercase = false;
	options.Password.RequiredLength = 4;

    // Lockout settings.
    //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    //options.Lockout.MaxFailedAccessAttempts = 5;
    //options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
	options.SignIn.RequireConfirmedEmail = true;
	options.SignIn.RequireConfirmedAccount = true;
});



builder.Services.AddAuthentication()
        .AddGoogle(options =>
        {
            var gconfig = builder.Configuration.GetSection("Authentication:Google");
            options.ClientId = gconfig["ClientId"];
            options.ClientSecret = gconfig["ClientSecret"];
			// localhost:7213/signin-google
			options.CallbackPath = "/login-google";
        });

builder.Services.AddSingleton<IdentityErrorDescriber, AppIdentityErrorDescriber>();

builder.Services.AddSignalR();

builder.Services.AddAutoMapper(typeof(ProductMapper));
builder.Services.AddAutoMapper(typeof(CouponMapper));
builder.Services.AddAutoMapper(typeof(BannerMapper));
builder.Services.AddAutoMapper(typeof(PostMapper));
builder.Services.AddAutoMapper(typeof(OrderMapper));
builder.Services.AddAutoMapper(typeof(ReturnMapper));
builder.Services.AddAutoMapper(typeof(RefundMapper));
builder.Services.AddAutoMapper(typeof(AssignmentMapper));


var app = builder.Build();

app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithRedirects("/Home/Error?statuscode={0}");

app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseStaticFiles();

// /contents/1.jpg => Uploads/1.jpg
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Uploads")
    ),
    RequestPath = "/contents"
});

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "Areas",
    pattern: "{area:exists}/{controller=ProductManage}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "Areas",
    pattern: "/product/{Slug?}",
    defaults: new { controller = "ShopProducts", action = "GetDetailProductBySlug" });

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}");

//Seed data
//var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<DShopContext>();
//SeedData.SeedingData(context);
app.MapHub<ChatHub>("/chatHub");

app.Run();
