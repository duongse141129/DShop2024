using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
    [SidebarMenu(Menu.Admin.CustomerService, SubMenu.CustomerService.FAQ)]
    public class FAQController : Controller
    {
        private readonly DShopContext _context;

        public FAQController(DShopContext context)
        {
            _context = context;
        }

        // GET: Admin/FAQModels
        public async Task<IActionResult> Index()
        {
            
            return View(await _context.FAQs.Where(f => f.Status != 0).ToListAsync());
        }


        [Authorize(Roles = RoleName.Administrator)]
        public IActionResult Create()
        {
            
            return View();
        }

        [Authorize(Roles = RoleName.Administrator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Question,Answer")] FAQModel fAQModel)
        {
            
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

                    fAQModel.Status = 1;
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Question,Answer,Status")] FAQModel fAQModel)
        {
            
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
            try
            {
                fAQModel.Status = 0;
                _context.FAQs.Update(fAQModel);
                await _context.SaveChangesAsync();
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Delete FAQ successful ";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Update FAQ fail " + ex.Message;
                return RedirectToAction(nameof(Index));
            }

        }

        private bool FAQModelExists(int id)
        {
            return _context.FAQs.Any(e => e.Id == id);
        }
    }
}
