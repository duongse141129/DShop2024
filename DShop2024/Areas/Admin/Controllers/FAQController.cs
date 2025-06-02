using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DShop2024.Models;
using DShop2024.EnumData;
using Microsoft.AspNetCore.Authorization;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
    public class FAQController : Controller
    {
        private readonly DShopContext _context;
        private readonly string sidebar = "faq";

        public FAQController(DShopContext context)
        {
            _context = context;
        }

        // GET: Admin/FAQModels
        public async Task<IActionResult> Index()
        {
            ViewBag.sidebar = sidebar;
            return View(await _context.FAQs.ToListAsync());
        }

        // GET: Admin/FAQModels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            ViewBag.sidebar = sidebar;
            if (id == null)
            {
                return NotFound();
            }

            var fAQModel = await _context.FAQs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fAQModel == null)
            {
                return NotFound();
            }

            return View(fAQModel);
        }

        [Authorize(Roles = RoleName.Administrator)]
        public IActionResult Create()
        {
            ViewBag.sidebar = sidebar;
            return View();
        }

        [Authorize(Roles = RoleName.Administrator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Question,Answer")] FAQModel fAQModel)
        {
            ViewBag.sidebar = sidebar;
            if (ModelState.IsValid)
            {
                _context.Add(fAQModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(fAQModel);
        }

        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> Edit(int? id)
        {
            ViewBag.sidebar = sidebar;
            if (id == null)
            {
                return NotFound();
            }

            var fAQModel = await _context.FAQs.FindAsync(id);
            if (fAQModel == null)
            {
                return NotFound();
            }
            return View(fAQModel);
        }

        [Authorize(Roles = RoleName.Administrator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Question,Answer")] FAQModel fAQModel)
        {
            ViewBag.sidebar = sidebar;
            if (id != fAQModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fAQModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FAQModelExists(fAQModel.Id))
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
            return View(fAQModel);
        }


        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> Delete(int? id)
        {
            ViewBag.sidebar = sidebar;
            if (id == null)
            {
                return NotFound();
            }

            var fAQModel = await _context.FAQs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fAQModel == null)
            {
                return NotFound();
            }
            if (fAQModel != null)
            {
                _context.FAQs.Remove(fAQModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FAQModelExists(int id)
        {
            return _context.FAQs.Any(e => e.Id == id);
        }
    }
}
