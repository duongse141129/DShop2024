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
	public class CategoryManageController : Controller
    {
        private readonly DShopContext _context;

        public CategoryManageController(DShopContext context)
        {
            _context = context;
        }

        // GET: Admin/CategoryManage
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.Where(p => p.Status == 1).ToListAsync());
        }

        // GET: Admin/CategoryManage/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoryModel = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (categoryModel == null)
            {
                return NotFound();
            }

            return View(categoryModel);
        }

        // GET: Admin/CategoryManage/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/CategoryManage/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryName,Description,Slug")] CategoryModel categoryModel)
        {
            
            
            if (ModelState.IsValid)
            {
                categoryModel.Slug = categoryModel.CategoryName.ToLower().Replace(" ", "-");
                var slug = await _context.Categories.FirstOrDefaultAsync(s => s.Slug == categoryModel.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "This category already exists");
                    return View(categoryModel);
                }
                categoryModel.Status = 1;

				_context.Add(categoryModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoryModel);
        }

        // GET: Admin/CategoryManage/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoryModel = await _context.Categories.FindAsync(id);
            if (categoryModel == null)
            {
                return NotFound();
            }
            return View(categoryModel);
        }

        // POST: Admin/CategoryManage/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CategoryName,Description")] CategoryModel categoryModel)
        {
            if (id != categoryModel.Id)
            {
                return NotFound();
            }
			var exitedCategory = await _context.Categories.FindAsync(id);
			if (ModelState.IsValid)
            {
                try
                {
					categoryModel.Slug = categoryModel.CategoryName.ToLower().Replace(" ", "-");
					var slug = await _context.Categories.FirstOrDefaultAsync(s => s.Slug == categoryModel.Slug);
					if (slug != null && exitedCategory.CategoryName.ToLower() != categoryModel.CategoryName.ToLower())
					{
						ModelState.AddModelError("", "Can't same slug");
						return View(categoryModel);
					}

					exitedCategory.CategoryName = categoryModel.CategoryName;
					exitedCategory.Description = categoryModel.Description;
					exitedCategory.Slug = categoryModel.Slug;
					exitedCategory.Status = 1;

					_context.Update(exitedCategory);
					await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryModelExists(categoryModel.Id))
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
            return View(categoryModel);
        }

        // GET: Admin/CategoryManage/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoryModel = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (categoryModel == null)
            {
                return NotFound();
            }

            return View(categoryModel);
        }

        // POST: Admin/CategoryManage/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoryModel = await _context.Categories.FindAsync(id);
            if (categoryModel != null)
            {
                _context.Categories.Remove(categoryModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryModelExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
