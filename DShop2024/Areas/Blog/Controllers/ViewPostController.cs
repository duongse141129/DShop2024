using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.Models.Blog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppMvc.Areas.Blog.Controllers
{
    [Area("Blog")]
    public class ViewPostController : Controller
    {
        private readonly ILogger<ViewPostController> _logger;

        private readonly DShopContext _context;

        public ViewPostController(ILogger<ViewPostController> logger, DShopContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Route("/post/{subjectSlug?}")]
        public async Task<IActionResult> Index(string search,string subjectSlug,[FromQuery(Name ="p")] int currentPage, int pagesSize)
        {
            ViewBag.sidebar = Menu.Home.Blog;
            var Subjects = GetSubjects();
            ViewBag.Subjects = Subjects;
            ViewBag.subjectSlug = subjectSlug;

            SubjectModel subject = null;
            if (!string.IsNullOrEmpty(subjectSlug))
            {
                subject = _context.Subjects
                                    .Where(c => c.Slug == subjectSlug)
                                    .Include(c => c.SubjectChildren)
                                    .FirstOrDefault();
                if (subject == null)
                {
                    return NotFound("Cannot find subject");
                }
            }


            IQueryable<PostModel> posts = _context.Posts.Where(p => p.Status != 0)
                                .Include(P => P.CreateBy)
                                .Include(P => P.PostSubjects)
                                .ThenInclude(p => p.Subject)
                                .OrderByDescending(p => p.DateUpdated)
                                .AsQueryable();

            var count = await posts.CountAsync();
            if (count > 0)
            {
                if (!String.IsNullOrEmpty(search))
                {
                    posts = posts.Where(c => c.Title.Contains(search) || c.ShortDescription.Contains(search) || c.PostContent.Contains(search));
                }
            }

            if (subject != null)
            {
                var ids = new List<int>();
                subject.ChildSubjectIDs(null, ids);
                ids.Add(subject.Id);

                posts = posts.Where(p => p.PostSubjects.Where(pc => ids.Contains(pc.SubjectId)).Any());
            }

            int totalPosts = posts.Count();
            if (pagesSize <= 0)
                pagesSize = 9;
            int countPages = (int)Math.Ceiling((double)totalPosts / 9);

            if (currentPage > countPages)
                currentPage = countPages;
            if (currentPage < 1)
                currentPage = 1;

            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Index", new
                {
                    p = pageNumber,
                    pagesSize = pagesSize
                })
            };

            var postsInPage = posts.Skip((currentPage - 1) * pagesSize)
                        .Take(pagesSize);

            ViewBag.pagingModel = pagingModel;
            ViewBag.totalPosts = totalPosts;

            ViewBag.Subject = subject;
            ViewBag.search = search;
            return View(postsInPage.ToList());
        }




        [Route("/post/{postslug}.html")]
        public IActionResult Detail(string postslug)
        {
            ViewBag.sidebar = Menu.Home.Blog;
            var Subjects = GetSubjects();
            ViewBag.Subjects = Subjects;

            var post = _context.Posts.Where(p => p.Slug == postslug)
                                    .Where(p => p.Status != 0)
                                   .Include(p => p.CreateBy)
                                   .Include(p => p.PostSubjects)
                                   .ThenInclude(pc => pc.Subject)
                                   .FirstOrDefault();

            if (post == null)
            {
                return NotFound("Cannot find post");
            }

            SubjectModel subject = post.PostSubjects.FirstOrDefault()?.Subject;
            ViewBag.Subject = subject;

            var otherPosts = _context.Posts.Where(p => p.PostSubjects.Any(c => c.Subject.Id == subject.Id))
                                            .Where(p => p.Id != post.Id)
                                            .OrderByDescending(p => p.DateUpdated)
                                            .Take(5);
            ViewBag.otherPosts = otherPosts;

            return View(post);
        }

        private List<SubjectModel> GetSubjects()
        {
            var subjects = _context.Subjects
                            .Include(c => c.SubjectChildren)
                            .AsEnumerable()
                            .Where(c => c.ParentSubject == null)
                            .Where(s => s.Status != 0)
                            .ToList();
            return subjects;
        }

 
    }
}
