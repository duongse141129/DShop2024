using AutoMapper;
using DShop2024.Areas.Admin.Models.Task;
using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
    [Route("Admin/Assignment")]
    [SidebarMenu(Menu.Admin.Assignment)]
    public class AssignmentController : Controller
    {
        private readonly DShopContext _context;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly IMapper _mapper;

        public AssignmentController(DShopContext context, UserManager<AppUserModel> userManager, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }
        public  IActionResult Index()
        {
            
            return View();
        }

        [HttpGet("calendar")]
        public async Task<IActionResult> GetCalendarTasks()
        {
            var user = await _userManager.GetUserAsync(this.User);
            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            var dataAssignment = _context.Assignments.Where(s => s.Status != 0).Include(t => t.Task).OrderBy(a => a.AssignedBy).AsQueryable();

            if (role == RoleName.Employee)
            {
                dataAssignment = dataAssignment.Where(a => a.EmployeeUserID == user.Id);
            }

            var data = await dataAssignment.Select(t => new AssignmentCalendarDto
            {
                Id = t.Id,
                Title = t.Task.Name,
                Day = t.AssignedDate.Day,
                Month = t.AssignedDate.Month,
                Year = t.AssignedDate.Year,
                Time = $"{t.AssignedDate:hh\\:mm tt} - {t.Deadline:hh\\:mm tt}",
                IsAllowUpdateDelete = role == RoleName.Administrator && t.Status == 1 && t.AssignedDate >= DateTime.Now ? true : false,
                Status = ((AssignmentEnumData.StatusTask)t.Status).ToString(),
            })
            .ToListAsync();

            return Json(data);
        }

        [HttpGet("Details/{id?}")]
        public async Task<IActionResult> Details(int? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }

            var assignmentModel = await _context.Assignments
                                        .Include( t => t.Task)
                                        .Include( t => t.Employee)
                                        .Include( t => t.AssignedBy)
                                        .FirstOrDefaultAsync(s => s.Id == id && s.Status != 0);
            if (assignmentModel == null)
            {
                return NotFound();
            }

            return PartialView("_DetailsAssignmentPopupPartial", assignmentModel);
        }

        [HttpPost("UpdateStatus/{id?}")]
        public async Task<IActionResult> UpdateStatus(int? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }

            var assignmentModel = await _context.Assignments
                                        .FirstOrDefaultAsync(s => s.Id == id && s.Status != 0);
            if (assignmentModel == null)
            {
                return NotFound();
            }
            try
            {
                var user = await _userManager.GetUserAsync(this.User);
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains(RoleName.Employee) && assignmentModel.EmployeeUserID != user.Id)
                {
                    return Ok(new { success = false, message = "You are not authorized to access other people's work." });
                }

                if (assignmentModel.Status == 1 || assignmentModel.Status == 2)
                {
                    assignmentModel.Status += 1;
                    _context.Assignments.Update(assignmentModel);
                    await _context.SaveChangesAsync();
                }
                return Ok(new { success = true, message = "Update status assignment successful", taskstatus = ((AssignmentEnumData.StatusTask)assignmentModel.Status).ToString() });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Update status assignment fail " + ex.Message });
            }
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create(DateTime startDate, DateTime endDate)
        {
            
            if (startDate.Date < DateTime.Today.Date)
            {
                return PartialView("_ErrorSelectDatePopupPartial", "You can not select a date in the past.");
            }
            ViewBag.Tasks = new SelectList(_context.Tasks.Where(b => b.Status != 0), "Id", "Name");
            List<AppUserModel> employees = await (from u in _context.Users
                                                  join ur in _context.UserRoles on u.Id equals ur.UserId
                                                  join r in _context.Roles on ur.RoleId equals r.Id
                                                  where r.Name == RoleName.Employee
                                                  where u.Status != 0
                                                  select u).ToListAsync();
            ViewBag.PickEmployees = employees;
            ViewBag.DatePick = startDate;
            ViewBag.DatePickEnd = endDate;
            var selectedDates = $" {startDate.Day} -> {endDate.Day} {endDate.ToString("MMMM yyyy")} ";
            if(startDate.Date == endDate.Date)
            {
                selectedDates = $" {startDate.Day} {endDate.ToString("MMMM yyyy")} ";
            }
            ViewBag.SelectedDates = selectedDates;
            return PartialView("_CreateAssignmentPopupPartial");

        }


        [HttpPost("Create")]
        public async Task<IActionResult> Create(CreateAssignmentRequest createAssignmentModel)
        {
            
            if (ModelState.IsValid)
            {
                if(createAssignmentModel.TimeFrom > createAssignmentModel.TimeTo)
                {
                    return Ok(new { success = false, message = "Time To must be greater than the Time From." });
                }

                try
                {
                    var user = await _userManager.GetUserAsync(this.User);

                    List<AssignmentModel> assignments = new List<AssignmentModel>();
                    for (DateTime date = createAssignmentModel.StartDate.Date ; date <= createAssignmentModel.EndDate.Date; date = date.AddDays(1))
                    {
                        var timeFrom = date.Date.Add(createAssignmentModel.TimeFrom);
                        var timeTo = date.Date.Add(createAssignmentModel.TimeTo);
                        if (timeFrom < DateTime.Now)
                        {
                            return Ok(new { success = false, message = "Time From must be greater than the current time." });
                        }

                        AssignmentModel assignmentModel = new AssignmentModel
                        {
                            AssignedByUserID = user.Id,
                            EmployeeUserID = createAssignmentModel.EmployeeUserID,
                            TaskID = createAssignmentModel.TaskId,
                            AssignmentDetails = createAssignmentModel.AssignmentDetails,
                            AssignedDate = timeFrom,
                            Deadline = timeTo,
                            Status = 1
                        };
                        assignments.Add(assignmentModel);
                    }
                    if (assignments.Any())
                    {
                        await _context.Assignments.AddRangeAsync(assignments);
                        await _context.SaveChangesAsync();
                    }
                    return Ok(new { success = true, message = "Create assignment successful" });
                }
                catch (Exception ex)
                {
                    return Ok(new { success = false, message = "Create assignment fail "+ex.Message });
                }

            }
            return Ok(new { success = false, message = "Input value is not complete" });
        }


        [HttpGet("DeleteAll")]
        public async Task<IActionResult> DeleteAll(DateTime startDate, DateTime endDate)
        {
            
            if (startDate.Date < DateTime.Today.Date)
            {
                return PartialView("_ErrorSelectDatePopupPartial", "You can not select a date in the past.");
            }
            var checkAssignment = await _context.Assignments.Where(s => s.Status != 0).Where(a => a.AssignedDate.Date >= startDate && a.AssignedDate.Date <= endDate).AnyAsync();
            if (!checkAssignment)
            {
                return PartialView("_ErrorSelectDatePopupPartial", "There is no assignment to delete.");
            }
            ViewBag.DatePick = startDate;
            ViewBag.DatePickEnd = endDate;
            var selectedDates = $" {startDate.Day} -> {endDate.Day} {endDate.ToString("MMMM yyyy")} ";
            if (startDate.Date == endDate.Date)
            {
                selectedDates = $" {startDate.Day} {endDate.ToString("MMMM yyyy")} ";
            }
            ViewBag.SelectedDates = selectedDates;
            return PartialView("_DeleteAllAssignmentPopupPartial");
        }

        [HttpPost("DeleteAll")]
        public async Task<IActionResult> DeleteAllConfirm(DateTime startDate, DateTime endDate)
        {
            
            if (startDate.Date < DateTime.Today.Date)
            {
                return PartialView("_ErrorSelectDatePopupPartial", "You can not select a date in the past.");
            }
            try
            {
                await _context.Assignments
                        .Where(s => s.Status != 0)
                        .Where(a => a.AssignedDate.Date >= startDate && a.AssignedDate.Date <= endDate)
                        .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, 0));

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Delete tasks all successful" });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Delete all fail" + ex.Message });
            }
        }


        [HttpGet("Edit/{id?}")]
        public async Task<IActionResult> Edit(int? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }

            var assignmentModel = await _context.Assignments.FirstOrDefaultAsync( s=> s.Id == id && s.Status != 0);
            if (assignmentModel == null)
            {
                return NotFound();
            }
            if(assignmentModel.AssignedDate < DateTime.Now)
            {
                return PartialView("_ErrorSelectDatePopupPartial", "Dates in the past shall not be modified.");
            }

            ViewBag.Tasks = new SelectList(_context.Tasks.Where(b => b.Status != 0), "Id", "Name", assignmentModel.TaskID);
            List<AppUserModel> employees = await (from u in _context.Users
                                                  join ur in _context.UserRoles on u.Id equals ur.UserId
                                                  join r in _context.Roles on ur.RoleId equals r.Id
                                                  where r.Name == RoleName.Employee
                                                  where u.Status != 0
                                                  select u).ToListAsync();

            ViewBag.PickEmployees = employees;
            EditAssignmentRequest editAssignment = _mapper.Map<EditAssignmentRequest>(assignmentModel);
            return PartialView("_EditAssignmentPopupPartial", editAssignment);
        }


        [HttpPost("Edit/{id?}")]
        public async Task<IActionResult> Edit(EditAssignmentRequest editAssignment)
        {
            

            if (ModelState.IsValid)
            {
                if (editAssignment.TimeFrom > editAssignment.TimeTo)
                {
                    return Ok(new { success = false, message = "Time To must be greater than the Time From." });
                }
                var date = DateTime.Parse(editAssignment.Date);
                var timeFrom = date.Date.Add(editAssignment.TimeFrom);
                if (timeFrom < DateTime.Now)
                {
                    return Ok(new { success = false, message = "Time From must be greater than the current time." });
                }

                try
                {
                    var user = await _userManager.GetUserAsync(this.User);
                    AssignmentModel assignmentModel = _mapper.Map<AssignmentModel>(editAssignment);
                    assignmentModel.AssignedByUserID = user.Id;
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


        [HttpGet("Delete/{id?}")]
        public async Task<IActionResult> Delete(int? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }

            var assignmentModel = await _context.Assignments.Include(t => t.Task).FirstOrDefaultAsync(s => s.Id == id && s.Status != 0);
            if (assignmentModel == null)
            {
                return NotFound();
            }
            if (assignmentModel.AssignedDate < DateTime.Now)
            {
                return PartialView("_ErrorSelectDatePopupPartial", "Dates in the past shall not be modified.");
            }
            return PartialView("_DeleteAssignmentPopupPartial", assignmentModel);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteConfirm(int? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }
            var assignmentModel = await _context.Assignments
                .FirstOrDefaultAsync(m => m.Id == id && m.Status != 0);
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
