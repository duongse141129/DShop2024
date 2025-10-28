using DShop2024.EnumData;
using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
    public class PolicyManageController : Controller
    {
        private readonly DShopContext _context;

        public PolicyManageController(DShopContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            ViewBag.sidebar = Menu.Admin.Information;
            return View(await _context.Policies.Where(p => p.Status != 0).ToListAsync());
        }

        [Authorize(Roles = RoleName.Administrator)]
        public IActionResult Create()
        {
            ViewBag.sidebar = Menu.Admin.Information;
            return View();
        }

        [Authorize(Roles = RoleName.Administrator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description")] PolicyModel policyModel)
        {
            ViewBag.sidebar = Menu.Admin.Information;
            if (ModelState.IsValid)
            {
                try
                {
                    policyModel.Status = 1;
                    _context.Add(policyModel);
                    await _context.SaveChangesAsync();
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Create policy successful ";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {

                    TempData[DShopConst.TEMPDATA_ERROR] = "Create brand fail " + ex.Message;
                    return View(policyModel);
                }

            }
            return View(policyModel);
        }

        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> Edit(int? id)
        {
            ViewBag.sidebar = Menu.Admin.Information;
            if (id == null)
            {
                return NotFound();
            }

            var policyModel = await _context.Policies.Where(p => p.Status != 0).FirstOrDefaultAsync(m => m.Id == id);
            if (policyModel == null)
            {
                return NotFound();
            }
            return View(policyModel);
        }

        [Authorize(Roles = RoleName.Administrator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description")] PolicyModel policyModel)
        {
            ViewBag.sidebar = Menu.Admin.Information;
            if (id != policyModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    policyModel.Status = 1;
                    _context.Update(policyModel);
                    await _context.SaveChangesAsync();
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Edit policy successful ";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PolicyModelExists(policyModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        TempData[DShopConst.TEMPDATA_ERROR] = "Edit policy fail ";
                        return View(policyModel);
                    }
                }
            }
            return View(policyModel);
        }

        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> Delete(int? id)
        {
            ViewBag.sidebar = Menu.Admin.Information;
            if (id == null)
            {
                return NotFound();
            }

            var policyModel = await _context.Policies.Where(p => p.Status != 0)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (policyModel == null)
            {
                return NotFound();
            }

            try
            {
                policyModel.Status = 0;
                _context.Policies.Update(policyModel);
                await _context.SaveChangesAsync();
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Create policy successful ";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {

                TempData[DShopConst.TEMPDATA_ERROR] = "Delete policy fail " + ex.Message;
                return RedirectToAction(nameof(Index));
            }

        }

        private bool PolicyModelExists(int id)
        {
            return _context.Policies.Any(e => e.Id == id);
        }
    }
}
