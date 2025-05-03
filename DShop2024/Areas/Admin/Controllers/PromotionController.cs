using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DShop2024.Models;
using DShop2024.EnumData;
using Microsoft.AspNetCore.Authorization;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize(Roles = RoleName.Administrator)]
	public class PromotionController : Controller
    {
        private readonly DShopContext _context;

        public PromotionController(DShopContext context)
        {
            _context = context;
        }

        // GET: Admin/PromotionModels
        public async Task<IActionResult> Index()
        {
            return View(await _context.Promotions.Where(p => p.Status != 0).ToListAsync());
        }

        // GET: Admin/PromotionModels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var promotionModel = await _context.Promotions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (promotionModel == null)
            {
                return NotFound();
            }

            return View(promotionModel);
        }

        // GET: Admin/PromotionModels/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/PromotionModels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CategoryCouponName,Status")] PromotionModel promotionModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(promotionModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(promotionModel);
        }

        // GET: Admin/PromotionModels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var promotionModel = await _context.Promotions.FindAsync(id);
            if (promotionModel == null)
            {
                return NotFound();
            }
            return View(promotionModel);
        }

        // POST: Admin/PromotionModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CategoryCouponName,Status")] PromotionModel promotionModel)
        {
            if (id != promotionModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(promotionModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PromotionModelExists(promotionModel.Id))
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
            return View(promotionModel);
        }

        // GET: Admin/PromotionModels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var promotionModel = await _context.Promotions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (promotionModel == null)
            {
                return NotFound();
            }

            return View(promotionModel);
        }

        // POST: Admin/PromotionModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var promotionModel = await _context.Promotions.FindAsync(id);
            if (promotionModel != null)
            {
                promotionModel.Status = 0;
                _context.Promotions.Update(promotionModel);
                await _context.SaveChangesAsync();
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PromotionModelExists(int id)
        {
            return _context.Promotions.Any(e => e.Id == id);
        }
    }
}
