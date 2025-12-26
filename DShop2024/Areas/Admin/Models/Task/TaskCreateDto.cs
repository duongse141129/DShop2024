namespace DShop2024.Areas.Admin.Models.Task
{
    public class TaskCreateDto
    {
        public string Title { get; set; }
        public string Date { get; set; }   // yyyy-MM-dd
        public string TimeFrom { get; set; }
        public string TimeTo { get; set; }
        public List<string> UserIds { get; set; }
    }
}
