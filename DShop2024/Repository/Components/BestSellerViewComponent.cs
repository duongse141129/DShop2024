using DShop2024.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Repository.Components
{
	public class BestSellerViewComponent : ViewComponent
	{
		private readonly DShopContext _context;

		public BestSellerViewComponent(DShopContext context)
		{
			_context = context;
		}
		public async Task<IViewComponentResult> InvokeAsync()
		{
			var bestSaleProducts = await _context.Products
			 .Where(p => p.Status != 0)
			 .Join(_context.OrderDetails,
			 p => p.Id,
			 od => od.ProductId,
			 (p, od) => new { p, od })
			 .Join(_context.Orders.Where(o => o.Status != 0),
			 pod => pod.od.OrderId,
			 o => o.Id,
			 (pod, o) => new { pod.p, pod.od, o })
			 .GroupBy(x => new
			 {
				 x.p.Id,
				 x.p.ProductName,
				 x.p.MainImage,
				 x.p.Price,
				 x.p.Stock,
				 x.p.BrandId,
				 x.p.CategoryId
			 })
			 .Select(g => new ProductViewModel
			 {
				 Id = g.Key.Id,
				 ProductName = g.Key.ProductName,
				 MainImage = g.Key.MainImage,
				 Stock = g.Key.Stock,	
				 Price = g.Key.Price,
				 BrandName = _context.Brands.FirstOrDefault( b => b.Id == g.Key.BrandId).BrandName,
				 CategoryName = _context.Categories.FirstOrDefault( b => b.Id == g.Key.CategoryId).CategoryName,
				 QuantitySold = g.Sum(x => x.od.Quantity)
			 })
			 .OrderByDescending(x => x.QuantitySold)
			 .Take(8)
			 .ToListAsync();
			return View(bestSaleProducts);

		}
	}
}
