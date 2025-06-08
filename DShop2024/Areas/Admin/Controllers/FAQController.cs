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
                try
                {
                    var checkExit = await _context.FAQs
                                       .Where(p => p.Question == fAQModel.Question)
                                      .AnyAsync();
                    if (checkExit)
                    {
                        TempData[DShopConst.TEMPDATA_ERROR] = "This question already exists.";
                        return View(fAQModel);
                    }


                    _context.Add(fAQModel);
                    await _context.SaveChangesAsync();
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Create FAQ successful ";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "Create FAQ fail "+ ex.Message;
                    return RedirectToAction(nameof(Create));
                }

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
            var exitedFAQ = await _context.FAQs.FindAsync(id);

            if (ModelState.IsValid)
            {
                try
                {
                    var checkExit = await _context.FAQs
                       .Where(p => p.Question == fAQModel.Question)
                      .AnyAsync();
                    if (checkExit && fAQModel.Question.ToLower() != exitedFAQ.Question.ToLower())
                    {
                        TempData[DShopConst.TEMPDATA_ERROR] = "This question already exists.";
                        return View(exitedFAQ);
                    }
                    exitedFAQ.Question = fAQModel.Question;
                    exitedFAQ.Answer = fAQModel.Answer;
                    _context.Update(exitedFAQ);
                    await _context.SaveChangesAsync();
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Update FAQ successful ";
                }
                catch (DbUpdateConcurrencyException db)
                {
                    if (!FAQModelExists(fAQModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        TempData[DShopConst.TEMPDATA_ERROR] = "Update FAQ fail "+db.Message;
                        return View(fAQModel);
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
