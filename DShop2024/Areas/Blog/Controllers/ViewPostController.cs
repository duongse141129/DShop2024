using AutoMapper;
using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.Models.Blog;
using DShop2024.Repository;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace AppMvc.Areas.Blog.Controllers
{
    [Area("Blog")]
    [SidebarMenu(Menu.Home.Blog)]
    public class ViewPostController : Controller
    {
        private readonly ILogger<ViewPostController> _logger;

        private readonly DShopContext _context;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly IMapper _mapper;

        public ViewPostController(ILogger<ViewPostController> logger, DShopContext context, UserManager<AppUserModel> userManager, IMapper mapper)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        [Route("/post/{subjectSlug?}")]
        public async Task<IActionResult> Index(string search,string subjectSlug,[FromQuery(Name ="p")] int currentPage, int pagesSize)
        {
            
            var Subjects = GetSubjects();
            ViewBag.Subjects = Subjects;
            ViewBag.subjectSlug = subjectSlug;

            SubjectModel subject = null;
            if (!string.IsNullOrEmpty(subjectSlug))
            {
                subject = await _context.Subjects
                    .Where(c => c.Slug == subjectSlug)
                    .Include(c => c.SubjectChildren)
                    .FirstOrDefaultAsync();
                if (subject == null)
                {
                    return NotFound();
                }
            }



            IQueryable<PostModel> posts = _context.Posts.Where(p => p.Status != 0)
                                .OrderByDescending(p => p.DateUpdated);

            if (!String.IsNullOrEmpty(search))
            {
                posts = posts.Where(c => c.Title.Contains(search) || c.ShortDescription.Contains(search) || c.PostContent.Contains(search));
            }

            if (subject != null)
            {
                var ids = new List<int>();
                subject.ChildSubjectIDs(null, ids);
                ids.Add(subject.Id);

                posts = posts.Where(p => p.PostSubjects.Where(pc => ids.Contains(pc.SubjectId)).Any());
            }

            int totalPosts = await posts.CountAsync();
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
            var postIds = await posts.Skip((currentPage - 1) * pagesSize)
                         .Take(pagesSize)
                         .Select(p => p.Id)
                         .ToListAsync();

            var postsInPage = await _context.Posts
                            .Include(P => P.Likes)
                            .Include(P => P.Comments.Where(c => c.Status != 0))
                            .Include(P => P.CreateBy)
                            .Include(P => P.PostSubjects)
                                .ThenInclude(p => p.Subject)
                            .Where(p => postIds.Contains(p.Id))
                            .OrderByDescending(p => p.DateUpdated) 
                            .ToListAsync();

            var postsVM = _mapper.Map<List<PostViewModel>>(postsInPage);
            ViewBag.pagingModel = pagingModel;
            ViewBag.totalPosts = totalPosts;

            ViewBag.Subject = subject;
            ViewBag.search = search;
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_PostListPartial", postsVM);
            }
            return View(postsVM);
        }

        [Route("/post/{postslug}.html")]
        public async Task<IActionResult> Detail(string postslug, [FromQuery(Name = "p")] int currentPage, int pagesSize)
        {
            if (string.IsNullOrEmpty(postslug))
            {
                return NotFound();
            }
            
            var Subjects = GetSubjects();
            ViewBag.Subjects = Subjects;

            var post = await _context.Posts.Where(p => p.Slug == postslug)
                                    .Where(p => p.Status != 0)
                                    .Include(p => p.CreateBy)
                                    .Include(p => p.PostSubjects)
                                    .ThenInclude(pc => pc.Subject)
                                    .Include(p => p.Likes)
                                    .Include(p => p.Comments)
                                    .FirstOrDefaultAsync();

            if (post == null)
            {
                return NotFound();
            }
            SubjectModel subject = post.PostSubjects.FirstOrDefault()?.Subject;
            ViewBag.Subject = subject;

            bool checkLike = false;
            var user = await _userManager.GetUserAsync(this.User);
            if (user != null)
            {
                var liked = await _context.Likes.AnyAsync(l => l.UserId == user.Id && l.PostId == post.Id);
                checkLike = liked;
            }
            ViewBag.Like = checkLike;

            var comments = _context.Comments
                        .Where(c => c.PostId == post.Id
                                 && c.Status != 0
                                 && c.ParentCommentId == null)
                        .Include(c => c.User)
                        .Include(c => c.CommentChildren.Where(c => c.Status != 0))
                            .ThenInclude(ch => ch.User)
                        .OrderByDescending(c => c.Timestamp)
                        .AsQueryable();

            int totalComments =  comments.Count();
            if (pagesSize <= 0)
                pagesSize = 10;
            int countPages = (int)Math.Ceiling((double)totalComments / 10);

            if (currentPage > countPages)
                currentPage = countPages;
            if (currentPage < 1)
                currentPage = 1;

            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Detail", new
                {
                    p = pageNumber,
                    pagesSize = pagesSize
                })
            };

            var commentsInPage = await comments.Skip((currentPage - 1) * pagesSize)
            .Take(pagesSize).ToListAsync();
            ViewBag.pagingModel = pagingModel;

            var rootComments = commentsInPage.Select(MapToVM).ToList();
            PostDetailViewModel postDetailVM = new PostDetailViewModel
            {
                PostDetail = _mapper.Map<PostViewModel>(post),
                ListComments = rootComments,
                Liked = checkLike
            };
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ViewCommentListPartial", postDetailVM);
            }

            return View(postDetailVM);
        }

        private CommentViewModel MapToVM(CommentModel c)
        {
            var roleName = _context.UserRoles
                                .Where(ur => c.UserId.Contains(ur.UserId))
                                .Join(_context.Roles, ur => ur.RoleId, r => r.Id,
                                    (ur, r) => new { ur.UserId, r.Name }).Select(r => r.Name).FirstOrDefault();

            return new CommentViewModel
            {
                Id = c.Id,
                CommentContent = c.CommentContent,
                Timestamp = c.Timestamp,
                ParentCommentId = c.ParentCommentId,
                User = new UserViewModel
                {
                    Id = c.User.Id,
                    UserName = c.User.UserName,
                    Avatar = c.User.Avatar,
                    Role = roleName,
                },
                CommentChildren = c.CommentChildren?.Where(c => c.Status != 0)
                    .OrderByDescending(x => x.Timestamp)
                    .Select(MapToVM)
                    .ToList()
            };
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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> LikeOrUnlikePost(int postId)
        {
            
            var user = await _userManager.GetUserAsync(this.User);
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Status != 0 && p.Id == postId);
            if (post == null)
            {
                return NotFound();
            }
            var liked = await _context.Likes.FirstOrDefaultAsync(l => l.UserId == user.Id && l.PostId == postId);
            try
            {
                var countLike = 0;
                if (liked != null)
                {
                    _context.Likes.Remove(liked);
                    await _context.SaveChangesAsync();
                    countLike = await _context.Likes.Where(p => p.PostId == post.Id).CountAsync();
                    return Ok(new { success = true, message = "unlike", count = countLike });
                }
                liked = new LikeModel() { PostId= post.Id, UserId= user.Id };
                await _context.Likes.AddAsync(liked);
                await _context.SaveChangesAsync();
                countLike = await _context.Likes.Where(p => p.PostId == post.Id).CountAsync();  
                return Ok(new { success = true, message = "liked", count = countLike });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Like post fail "+ex.Message });

            }

        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CommentPost(int postId, string content)
        {
            var user = await _userManager.GetUserAsync(this.User);
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Status != 0 && p.Id == postId);

            if (post == null) return NotFound();

            if (string.IsNullOrEmpty(content))
            {
                return Ok(new { success = false, message = "Comment cannot be empty." });
            }

            try
            {
                CommentModel comment = new CommentModel()
                {
                    PostId = post.Id,
                    UserId = user.Id,
                    CommentContent = content,
                    Timestamp = DateTime.Now,
                    Status = 1
                };
                await _context.Comments.AddAsync(comment);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Add comment successfully." });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Add comment fail: " + ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetPopUpReplyComment(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            var commentModel = await _context.Comments
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);
            if (commentModel == null)
            {
                return NotFound();
            }
            return PartialView("_PopupReplyCommenttPartial", commentModel);
        }

        [HttpPost]
        public async Task<IActionResult> ReplyComment(int? Id, int postId, string ReplyMessage)
        {
            if (Id == null)
            {
                return NotFound();
            }
            var commentModel = await _context.Comments
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);
            var postModel = await _context.Posts
                .FirstOrDefaultAsync(m => m.Id == postId && m.Status != 0);
            if (commentModel == null || postModel == null)
            {
                return NotFound();
            }
            if (String.IsNullOrEmpty(ReplyMessage))
            {
                return Ok(new { success = false, Message = "Input reply message " });
            }
            try
            {
                var user = await _userManager.GetUserAsync(this.User);

                CommentModel replyComment = new CommentModel
                {
                    CommentContent = ReplyMessage,
                    UserId = user.Id,
                    PostId = postId,
                    ParentCommentId = commentModel.Id,
                    Timestamp = DateTime.Now,
                    Status = 1
                };
                await _context.Comments.AddAsync(replyComment);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Reply comment successful" });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Reply comment fail " + ex.Message });
            }
        }



    }
}
