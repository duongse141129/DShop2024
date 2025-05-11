using System.Configuration;
using System;
using Microsoft.EntityFrameworkCore;
using DShop2024.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using DShop2024.Models;
using DShop2024.Services.Momo;
using DShop2024.Models.Momo;
using DShop2024.Services.Vnpay;
using static Org.BouncyCastle.Math.EC.ECCurve;
using DShop2024.Services;
using DShop2024.Hubs;
using DShop2024.AutoMapper;
using Microsoft.Extensions.FileProviders;
using System.Security.Policy;
using System.Reflection.Metadata;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions();
var mailsetting = builder.Configuration.GetSection("MailSettings");
builder.Services.Configure<MailSettings>(mailsetting);
builder.Services.AddSingleton<IEmailSender, SendMailService>();

builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();

builder.Services.AddScoped<IVnPayService, VnPayService>();

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



//builder.Services.AddIdentity<AppUserModel, IdentityRole>()
//	.AddEntityFrameworkStores<DShopContext>().AddDefaultTokenProviders();

// Dang ky Identity
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
	//options.User.AllowedUserNameCharacters =
	//"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
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

var app = builder.Build();

app.UseStatusCodePagesWithRedirects("/Home/Error?statuscode={0}");

app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

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
    name: "category",
    pattern: "{area:exists}/{controller=ProductManage}/{action=Index}/{id?}");



app.MapControllerRoute(
    name: "Areas",
    pattern: "/category/{CategorySlug?}",
    defaults: new { controller = "Product", action = "Index" });

app.MapControllerRoute(
    name: "Areas",
    pattern: "/brand/{BrandSlug?}",
    defaults: new { controller = "Product", action = "Index"});



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//Seed data
//var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<DShopContext>();
//SeedData.SeedingData(context);
app.MapHub<ChatHub>("/chatHub");

app.Run();
