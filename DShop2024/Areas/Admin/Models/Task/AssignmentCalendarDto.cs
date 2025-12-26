namespace DShop2024.Areas.Admin.Models.Task
{
    public class AssignmentCalendarDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string Time { get; set; }
        public bool IsAllowUpdateDelete { get; set; }
        public string Status { get; set; }
    }
}
