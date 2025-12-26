using DShop2024.Areas.Admin.Models.Task;
using DShop2024.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace DShop2024.Areas.Admin.Controllers
{
    [Route("tasks")]
    public class TasksController : Controller
    {
        private readonly DShopContext _context;

        public TasksController(DShopContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] TaskCreateDto dto)
        {
            var task = new AssignmentModel
            {
                AssignmentDetails = dto.Title,
                AssignedDate = DateTime.Parse(dto.Date),
                Deadline = DateTime.Parse(dto.TimeTo)
            };

            _context.Assignments.Add(task);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("calendar")]
        public async Task<IActionResult> GetCalendarTasks()
        {
            var data = await _context.Assignments.Where(s => s.Status != 0).Include(t => t.Task)
                .Select(t => new AssignmentCalendarDto
                {
                    Id = t.Id,
                    Title = t.Task.Name,
                    Day = t.AssignedDate.Day,
                    Month = t.AssignedDate.Month,
                    Year = t.AssignedDate.Year,
                    Time = $"{t.AssignedDate:hh\\:mm} - {t.Deadline:hh\\:mm}"
                })
                .ToListAsync();

            return Json(data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.Assignments.FindAsync(id);
            if (task == null) return NotFound();

            _context.Assignments.Remove(task);
            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
