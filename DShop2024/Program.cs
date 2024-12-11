using System.Configuration;
using System;

using Microsoft.EntityFrameworkCore;
using DShop2024.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<DShopContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectedDb"));
});

var app = builder.Build();



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
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//Seed data
//var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<DShopContext>();
//SeedData.SeedingData(context);

app.Run();
