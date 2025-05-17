using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DShop2024.Models;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
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

        // GET: Admin/FAQModels/Create
        public IActionResult Create()
        {
            ViewBag.sidebar = sidebar;
            return View();
        }

        // POST: Admin/FAQModels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: Admin/FAQModels/Edit/5
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

        // POST: Admin/FAQModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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



        // POST: Admin/FAQModels/Delete/5

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
