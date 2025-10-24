using DShop2024.Models.Blog;
using Microsoft.AspNetCore.Mvc;

namespace DShop2024.Components
{
    [ViewComponent]
    public class SubjectSidebar : ViewComponent
    {
        public class SubjectSidebarData
        {
            public List<SubjectModel> Subjects { get; set; }

            public int level { get; set; }

            public string subjectSlug { get; set; }
        }

        public IViewComponentResult Invoke(SubjectSidebarData data)
        {
            return View(data);
        }
    }
}
