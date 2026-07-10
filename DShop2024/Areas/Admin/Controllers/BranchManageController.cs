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
    [SidebarMenu(Menu.Admin.Information, SubMenu.Information.Branch)]
    public class BranchManageController : Controller
    {
        private readonly DShopContext _context;

        public BranchManageController(DShopContext context)
        {
            _context = context;
        }

        // GET: Admin/BranchManage
        public async Task<IActionResult> Index()
        {
            return View(await _context.Branches.Where(b => b.Status != 0).ToListAsync());
        }


        // GET: Admin/BranchManage/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var branchModel = await _context.Branches.Where(b => b.Status != 0)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (branchModel == null)
            {
                return NotFound();
            }

            return View(branchModel);
        }

        // GET: Admin/BranchManage/Create
        [Authorize(Roles = RoleName.Administrator)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/BranchManage/Create
        [Authorize(Roles = RoleName.Administrator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Latitude,Longitude,Address,MapEmbed")] BranchModel branchModel)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    branchModel.Status = 1;
                    _context.Add(branchModel);
                    await _context.SaveChangesAsync();
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Create Branch successful ";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "Create Branch fail " + ex.Message;
                    return RedirectToAction(nameof(Create));
                }
            }
            return View(branchModel);
        }

        // GET: Admin/BranchManage/Edit/5
        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var branchModel = await _context.Branches.Where(b => b.Status != 0).FirstOrDefaultAsync(b => b.Id == id);
            if (branchModel == null)
            {
                return NotFound();
            }
            return View(branchModel);
        }

        // POST: Admin/BranchManage/Edit/5
        [Authorize(Roles = RoleName.Administrator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Latitude,Longitude,Address,MapEmbed,Status")] BranchModel branchModel)
        {
            if (id != branchModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(branchModel);
                    await _context.SaveChangesAsync();
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Update Branch successful";
                }
                catch (DbUpdateConcurrencyException db)
                {
                    if (!BranchModelExists(branchModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        TempData[DShopConst.TEMPDATA_ERROR] = "Update Branch fail " + db.Message;
                        return View(branchModel);
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(branchModel);
        }

        // GET: Admin/BranchManage/Delete/5
        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var branchModel = await _context.Branches.Where(b => b.Status != 0)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (branchModel == null)
            {
                return NotFound();
            }
            try
            {
                branchModel.Status = 0;
                _context.Branches.Update(branchModel);
                await _context.SaveChangesAsync();
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Delete Branch successful ";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Update Branch fail " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private bool BranchModelExists(int id)
        {
            return _context.Branches.Any(e => e.Id == id);
        }
    }
}
