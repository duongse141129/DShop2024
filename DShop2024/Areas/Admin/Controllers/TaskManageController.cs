using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleName.Administrator)]
    [SidebarMenu(Menu.Admin.Assignment, SubMenu.Assignment.TaskManagement)]
    public class TaskManageController : Controller
    {
        private readonly DShopContext _context;

        public TaskManageController(DShopContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            
            var listTask = await _context.Tasks.Where(t => t.Status != 0).ToListAsync();
            return View(listTask);
        }



        public IActionResult Create()
        {
            
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] TaskModel taskModel)
        {
            
            if (ModelState.IsValid)
            {
                try
                {
                    taskModel.Status = 1;
                    _context.Add(taskModel);
                    await _context.SaveChangesAsync();
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Create task successful ";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "Create task fail " + ex.Message;
                    return RedirectToAction(nameof(Index));
                }

            }
            return View(taskModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }

            var taskModel = await _context.Tasks.FirstOrDefaultAsync(s => s.Id == id && s.Status != 0);
            if (taskModel == null)
            {
                return NotFound();
            }
            return View(taskModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] TaskModel taskModel)
        {
            
            if (id != taskModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Edit task successful ";
                    taskModel.Status = 1;
                    _context.Tasks.Update(taskModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskModelExists(taskModel.Id))
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
            return View(taskModel);
        }


        public async Task<IActionResult> Delete(int? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }

            var taskModel = await _context.Tasks
                .FirstOrDefaultAsync(m => m.Id == id && m.Status != 0);
            if (taskModel == null)
            {
                return NotFound();
            }
            try
            {
                taskModel.Status = 0;
                _context.Tasks.Update(taskModel);
                await _context.SaveChangesAsync();
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Delete task successful ";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Delete task fail " + ex.Message;
                return RedirectToAction(nameof(Index));
            }

        }

        private bool TaskModelExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}
