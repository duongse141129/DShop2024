using System.Configuration;
using System;

using Microsoft.EntityFrameworkCore;
using DShop2024.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DShopContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectedDb"));
});


// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
});


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

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "category",
    pattern: "{area:exists}/{controller=ProductManage}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "Areas",
    pattern: "/category/{Slug?}",
    defaults: new { controller = "Category", action = "Index" });

app.MapControllerRoute(
    name: "Areas",
    pattern: "/brand/{Slug?}",
    defaults: new { controller = "Brand", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");




//Seed data
//var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<DShopContext>();
//SeedData.SeedingData(context);

app.Run();
