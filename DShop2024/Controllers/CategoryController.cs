﻿using DShop2024.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Controllers
{
	public class CategoryController : Controller
	{
		private readonly DShopContext _dataContext;

		public CategoryController( DShopContext context)
		{
			_dataContext = context;
		}
		public async Task<IActionResult> Index(string slug = "")
		{
			CategoryModel category = await _dataContext.Categories
										.Where(c => c.Status == 1)
										.Where(c => c.Slug == slug)
										.FirstOrDefaultAsync();
			if (category == null)
			{
				return RedirectToAction("Index");
			}

			var productByCategory = await _dataContext.Products.Where(p => p.CategoryId == category.Id).ToListAsync();
			productByCategory.OrderByDescending(c => c.Id);
			return View(productByCategory);

		}
	}
}
