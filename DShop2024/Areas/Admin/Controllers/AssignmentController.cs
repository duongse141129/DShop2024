using DShop2024.EnumData;
using DShop2024.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
    public class AssignmentController : Controller
    {
        private readonly DShopContext _context;
        private readonly UserManager<AppUserModel> _userManager;

        public AssignmentController(DShopContext context, UserManager<AppUserModel> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            ViewBag.sidebar = Menu.Admin.Assignment;
            var listAssignMent = await _context.Assignments.Where(a => a.Status != 0)
                                                            .Include(a => a.Task)
                                                            .Include(a => a.AssignedBy)
                                                            .Include(a => a.Employee)
                                                            .ToListAsync();
            return View(listAssignMent);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.sidebar = Menu.Admin.Assignment;
            ViewBag.Tasks = new SelectList(_context.Tasks.Where(b => b.Status != 0), "Id", "Name");
            List<AppUserModel> employees = await(from u in _context.Users
                                             join ur in _context.UserRoles on u.Id equals ur.UserId
                                             join r in _context.Roles on ur.RoleId equals r.Id
                                             where r.Name == RoleName.Employee
                                             where u.Status != 0
                                             select u).ToListAsync();

            ViewBag.Employees = new SelectList(employees.Where(b => b.Status != 0), "Id", "UserName");
            return PartialView("_CreateAssignmentPopupPartial");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssignmentModel assignmentModel)
        {
            ViewBag.sidebar = Menu.Admin.Assignment;
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(this.User);
                    assignmentModel.AssignedByUserID = user.Id;
                    assignmentModel.Status = 1;
                    _context.Assignments.Add(assignmentModel);
                    await _context.SaveChangesAsync();
                    return Ok(new { success = true, message = "Create assignment successful" });
                }
                catch (Exception ex)
                {
                    return Ok(new { success = false, message = "Create assignment fail "+ex.Message });
                }

            }
            return Ok(new { success = false, message = "Input value is not complete" });
        }

        public async Task<IActionResult> Edit(int? Id)
        {
            ViewBag.sidebar = Menu.Admin.Assignment;
            if (Id == null)
            {
                return NotFound();
            }

            var assignmentModel = await _context.Assignments.FirstOrDefaultAsync( s=> s.Id == Id && s.Status != 0);
            if (assignmentModel == null)
            {
                return NotFound();
            }
            ViewBag.Tasks = new SelectList(_context.Tasks.Where(b => b.Status != 0), "Id", "Name", assignmentModel.TaskID);
            List<AppUserModel> employees = await (from u in _context.Users
                                                  join ur in _context.UserRoles on u.Id equals ur.UserId
                                                  join r in _context.Roles on ur.RoleId equals r.Id
                                                  where r.Name == RoleName.Employee
                                                  where u.Status != 0
                                                  select u).ToListAsync();

            ViewBag.Employees = new SelectList(employees.Where(b => b.Status != 0), "Id", "UserName", assignmentModel.EmployeeUserID);
            return PartialView("_EditAssignmentPopupPartial", assignmentModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, AssignmentModel assignmentModel)
        {
            ViewBag.sidebar = Menu.Admin.Assignment;
            if (Id != assignmentModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(this.User);
                    assignmentModel.AssignedByUserID = user.Id;
                    assignmentModel.Status = 1;
                    _context.Assignments.Update(assignmentModel);
                    await _context.SaveChangesAsync();
                    return Ok(new { success = true, message = "Edit assignment successful "});
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    return Ok(new { success = false, message = "Edit assignment fail " + ex.Message });
                }
            }
            return Ok(new { success = false, message = "Input value is not complete" });
        }

        public async Task<IActionResult> Delete(int? Id)
        {
            ViewBag.sidebar = Menu.Admin.Assignment;
            if (Id == null)
            {
                return NotFound();
            }

            var assignmentModel = await _context.Assignments.FirstOrDefaultAsync(s => s.Id == Id && s.Status != 0);
            if (assignmentModel == null)
            {
                return NotFound();
            }
            return PartialView("_DeleteAssignmentPopupPartial", assignmentModel);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteConfirm(int? Id)
        {
            ViewBag.sidebar = Menu.Admin.Assignment;
            if (Id == null)
            {
                return NotFound();
            }
            var assignmentModel = await _context.Assignments
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);
            if (assignmentModel == null)
            {
                return NotFound();
            }
            try
            {
                assignmentModel.Status = 0;
                _context.Assignments.Update(assignmentModel);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Delete assignment successful" });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Delete assignment fail " + ex.Message });
            }

        }
    }
}
