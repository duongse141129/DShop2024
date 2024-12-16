using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class BrandManageController : Controller
    {
        private readonly DShopContext _context;

        public BrandManageController(DShopContext context)
        {
            _context = context;
        }

        // GET: Admin/BrandManage
        public async Task<IActionResult> Index()
        {
            return View(await _context.Brands.Where(p => p.Status == 1).ToListAsync());
        }

        // GET: Admin/BrandManage/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brandModel = await _context.Brands
                .FirstOrDefaultAsync(m => m.Id == id);
            if (brandModel == null)
            {
                return NotFound();
            }

            return View(brandModel);
        }

        // GET: Admin/BrandManage/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/BrandManage/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BrandName,Description")] BrandModel brandModel)
        {
            if (ModelState.IsValid)
            {
				brandModel.Slug = brandModel.BrandName.ToLower().Replace(" ", "-");
				var slug = await _context.Brands.FirstOrDefaultAsync(s => s.Slug == brandModel.Slug);
				if (slug != null)
				{
					ModelState.AddModelError("", "This brand already exists.");
					return View(brandModel);
				}
                brandModel.Status =1;

				_context.Add(brandModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(brandModel);
        }

        // GET: Admin/BrandManage/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brandModel = await _context.Brands.FindAsync(id);
            if (brandModel == null)
            {
                return NotFound();
            }
            return View(brandModel);
        }

        // POST: Admin/BrandManage/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BrandName,Description")] BrandModel brandModel)
        {
            if (id != brandModel.Id)
            {
                return NotFound();
            }
			var exitedBrand = await _context.Brands.FindAsync(id);
			if (ModelState.IsValid)
            {
                try
                {
					brandModel.Slug = brandModel.BrandName.ToLower().Replace(" ", "-");
					var slug = await _context.Brands.FirstOrDefaultAsync(s => s.Slug == brandModel.Slug);
					if (slug != null && exitedBrand.BrandName.ToLower() != brandModel.BrandName.ToLower())
					{
						ModelState.AddModelError("", "Can't same slug");
						return View(brandModel);
					}

					exitedBrand.BrandName = brandModel.BrandName;
					exitedBrand.Description = brandModel.Description;
					exitedBrand.Slug = brandModel.Slug;
					exitedBrand.Status = 1;

					_context.Update(exitedBrand);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BrandModelExists(brandModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(brandModel);
        }

        // GET: Admin/BrandManage/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brandModel = await _context.Brands
                .FirstOrDefaultAsync(m => m.Id == id);
            if (brandModel == null)
            {
                return NotFound();
            }

            return View(brandModel);
        }

        // POST: Admin/BrandManage/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var brandModel = await _context.Brands.FindAsync(id);
            if (brandModel != null)
            {
                _context.Brands.Remove(brandModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BrandModelExists(int id)
        {
            return _context.Brands.Any(e => e.Id == id);
        }
    }
}
