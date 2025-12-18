using App.Utilities;
using AppMvc.Areas.Blog.Models;
using AutoMapper;
using Bogus;
using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.Models.Blog;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;


namespace AppMvc.Areas.Blog.Controllers
{
    [Area("Blog")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
    public class PostController : Controller
    {
        private readonly DShopContext _context;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;

        public PostController(DShopContext context, IWebHostEnvironment webHostEnvironment, UserManager<AppUserModel> userManager, IMapper mapper, IEmailSender emailSender)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _mapper = mapper;
            _emailSender = emailSender;
        }

        public async Task<IActionResult> Index(string search,string subject_by, [FromQuery(Name = "p")]int currentPage, int pagesSize)
        {
            ViewBag.sidebar = Menu.Admin.Blog;
            IQueryable<PostModel> posts = _context.Posts.Where(p => p.Status!= 0)
                                        .OrderByDescending(p => p.IsPin)
                                        .ThenByDescending(p => p.DateUpdated);

            var count = await posts.CountAsync();
            if (count > 0)
            {
                if (!String.IsNullOrEmpty(search))
                {
                    posts = posts.Where(c => c.Title.Contains(search) || c.ShortDescription.Contains(search) || c.PostContent.Contains(search));
                }
                if (!String.IsNullOrEmpty(subject_by))
                {
                    posts = posts.Where(p => p.PostSubjects.Any(ps => ps.SubjectId == Convert.ToInt32(subject_by)));
                }
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

            ViewBag.pagingModel = pagingModel;
            ViewBag.totalPosts = totalPosts;
            ViewBag.postIndex = (currentPage -1) * pagesSize;
            ViewBag.search = search;

            var Subjects = await _context.Subjects.Where(s => s.Status !=0).ToArrayAsync();
            ViewBag.listSubject = Subjects;
            ViewBag.subjectBy = Convert.ToInt32(subject_by);

            var postsInPage =await posts.Skip((currentPage - 1) * pagesSize)
                        .Take(pagesSize)
                        .Include(p => p.CreateBy)
                        .Include(p => p.Likes)
                        .Include(p => p.Comments)
                        .Include(p => p.PostSubjects)
                        .ThenInclude(pc =>pc.Subject)
                        .OrderByDescending(p => p.IsPin)
                        .ThenByDescending(p => p.DateUpdated)
                        .ToListAsync();
            var postsVM = _mapper.Map<List<PostViewModel>>(postsInPage);
            return View(postsVM);
        }


        public async Task<IActionResult> Details(int? id, [FromQuery(Name = "p")] int currentPage, int pagesSize)
        {
            ViewBag.sidebar = Menu.Admin.Blog;
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.Where(p => p.Status != 0)
                .Include(p => p.CreateBy)
                .Include(p => p.PostSubjects)
                .ThenInclude(pc => pc.Subject)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

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


            int totalComments = comments.Count();
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
                generateUrl = (pageNumber) => Url.Action("Details", new
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
            return View(postDetailVM);
        }

        private CommentViewModel MapToVM(CommentModel c)
        {
            var roleName = _context.UserRoles
                                .Where(ur => c.UserId.Contains(ur.UserId))
                                .Join(_context.Roles, ur => ur.RoleId, r => r.Id,
                                    (ur, r) => new { ur.UserId, r.Name }).Select( r => r.Name).FirstOrDefault();

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


        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> CreateAsync()
        {
            ViewBag.sidebar = Menu.Admin.Blog;
            var Subjects = await _context.Subjects.Where(s=> s.Status != 0).ToArrayAsync();

            ViewData["Subjects"] = new MultiSelectList(Subjects, "Id", "Title");
            
            return View();
        }

        [Authorize(Roles = RoleName.Administrator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,ShortDescription,PostContent,SubjectIDs,ImageUpload")] CreatePostModel post)
        {
            ViewBag.sidebar = Menu.Admin.Blog;
            var Subjects = await _context.Subjects.Where(s => s.Status != 0).ToListAsync();
            ViewData["Subjects"] = new MultiSelectList(Subjects,"Id","Title");

            post.Slug = AppUtilities.GenerateSlug(post.Title);

            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug)){

                ModelState.AddModelError("Slug", "This url post already exists.");
                return View(post);
            }
              
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(this.User);

                    if (post.ImageUpload != null)
                    {
                        string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/post");
                        string imageName = Guid.NewGuid().ToString() + "_" + post.ImageUpload.FileName;
                        string filePath = Path.Combine(uploadsDir, imageName);

                        FileStream fs = new FileStream(filePath, FileMode.Create);
                        await post.ImageUpload.CopyToAsync(fs);
                        fs.Close();
                        post.Image = imageName;

                    }

                    post.DateCreated = post.DateUpdated = DateTime.Now;
                    post.UserIdCreate = user.Id;
                    post.IsPin = false;
                    post.Status = 1;
                    _context.Add(post);

                    if (post.SubjectIDs != null)
                    {
                        foreach (var cateID in post.SubjectIDs)
                        {
                            _context.Add(new PostSubjectModel()
                            {
                                SubjectId = cateID,
                                Post = post
                            });
                        }
                    }
                    await _context.SaveChangesAsync();
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Create post successful ";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "Create post fail" + ex.Message;
                    return View(post);
                }

            }
            return View(post);
        }

        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> Edit(int? id)
        {
            ViewBag.sidebar = Menu.Admin.Blog;
            if (id == null)
            {
                return NotFound();
            }
            var post = await _context.Posts.Where(p => p.Status != 0)
                                .Include(p => p.PostSubjects)
                                .FirstOrDefaultAsync(p => p.Id == id );
            if (post == null)
            {
                return NotFound();
            }
            var user = await _userManager.GetUserAsync(this.User);
            if (user.Id != post.UserIdCreate)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Cannot edit other people's posts";
                return RedirectToAction("Index");
            }

            var postEdit = new CreatePostModel()
            {
                Id = post.Id,
                Title = post.Title,
                PostContent = post.PostContent,
                ShortDescription = post.ShortDescription,
                Slug = post.Slug,
                Image = post.Image,
                SubjectIDs = post.PostSubjects.Select(pc => pc.SubjectId).ToArray()
            };

            var Subjects = await _context.Subjects.Where(s => s.Status != 0).ToListAsync();
            ViewData["Subjects"] = new MultiSelectList(Subjects, "Id", "Title");
            return View(postEdit);
        }

        [Authorize(Roles = RoleName.Administrator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ShortDescription,PostContent,SubjectIDs,ImageUpload")] CreatePostModel post)
        {
            ViewBag.sidebar = Menu.Admin.Blog;
            if (id != post.Id)
            {
                return NotFound();
            }
            var subjects = await _context.Subjects.Where(s => s.Status != 0).ToListAsync();
            ViewData["Subjects"] = new MultiSelectList(subjects, "Id", "Title");

            post.Slug = AppUtilities.GenerateSlug(post.Title);

            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug && p.Id != id))
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "This url post already exists.";
                return View(post);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var postUpdate = await _context.Posts.Where(p => p.Status!=0)
                                    .Include(p => p.PostSubjects).FirstOrDefaultAsync(p => p.Id == id);
                    if (postUpdate == null)
                    {
                        return NotFound();
                    }
                    if (post.ImageUpload != null)
                    {

                        string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/post");
                        string imageName = Guid.NewGuid().ToString() + "_" + post.ImageUpload.FileName;
                        string filePath = Path.Combine(uploadsDir, imageName);

                        if (postUpdate.Image != null)
                        {
                            string oldFilePath = Path.Combine(uploadsDir, postUpdate.Image);
                            try
                            {
                                if (System.IO.File.Exists(oldFilePath) && post.Image != PostEnumData.IMAGE_DEFAULT)
                                {
                                    System.IO.File.Delete(oldFilePath);
                                }

                            }
                            catch (IOException ex)
                            {
                                TempData[DShopConst.TEMPDATA_ERROR] = "An error occurred while deleting the post image " + ex.Message;
                                return View(post);
                            }
                        }

                        FileStream fs = new FileStream(filePath, FileMode.Create);
                        await post.ImageUpload.CopyToAsync(fs);
                        fs.Close();
                        postUpdate.Image = imageName;

                    }


                    postUpdate.Title = post.Title;
                    postUpdate.ShortDescription = post.ShortDescription;
                    postUpdate.PostContent = post.PostContent;
                    postUpdate.Slug = post.Slug;
                    postUpdate.DateUpdated = DateTime.Now;

                    // Update PostSubject
                    if (post.SubjectIDs == null) post.SubjectIDs = new int[] { };

                    var oldCateIds = postUpdate.PostSubjects.Select(c => c.SubjectId).ToArray();
                    var newCateIds = post.SubjectIDs;

                    var removeCatePosts = from postCate in postUpdate.PostSubjects
                                          where (!newCateIds.Contains(postCate.SubjectId))
                                          select postCate;
                    _context.PostSubjects.RemoveRange(removeCatePosts);

                    var addCateIds = from CateId in newCateIds
                                     where !oldCateIds.Contains(CateId)
                                     select CateId;

                    foreach (var CateId in addCateIds)
                    {
                        _context.PostSubjects.Add(new PostSubjectModel()
                        {
                            PostId = id,
                            SubjectId = CateId
                        });
                    }

                    _context.Posts.Update(postUpdate);
                    await _context.SaveChangesAsync();
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Update post successful ";
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "Update post fail" + ex.Message;
                    return View(post);
                }

                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> Delete(int? id)
        {
            ViewBag.sidebar = Menu.Admin.Blog;
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.Where(p => p.Status != 0)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }
            try
            {
                if (post.Image != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/post");
                    string oldFilePath = Path.Combine(uploadsDir, post.Image);
                    try
                    {
                        if (System.IO.File.Exists(oldFilePath) && post.Image != PostEnumData.IMAGE_DEFAULT)
                        {
                            System.IO.File.Delete(oldFilePath);
                        }

                    }
                    catch (IOException ex)
                    {
                        TempData[DShopConst.TEMPDATA_ERROR] = "An error occurred while deleting the post image " + ex.Message;
                        return RedirectToAction("Index");
                    }
                }

                post.Status = 0;
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Update post successful " + post.Title;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Update post fail: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }


        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> PinPost(int? id)
        {
            ViewBag.sidebar = Menu.Admin.Blog;
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.Where(p => p.Status != 0)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            try
            {
                if (post.IsPin == false)
                {
                    var count = await _context.Posts.Where(p => p.Status != 0 && p.IsPin == true).CountAsync();
                    if (count == 5)
                    {
                        TempData[DShopConst.TEMPDATA_ERROR] = "Pin post fail. Maximum of 5 posts can be pinned ";
                        return RedirectToAction("Index");
                    }
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Pin post successful " + post.Title;
                    post.IsPin = true;
                }
                else
                {
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "UnPin post successful " + post.Title;
                    post.IsPin = false;
                }
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Pin post fail: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> PinPostDetail(int? id)
        {
            ViewBag.sidebar = Menu.Admin.Blog;
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.Where(p => p.Status != 0)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            try
            {
                if (post.IsPin == false)
                {
                    var count = await _context.Posts.Where(p => p.Status != 0 && p.IsPin == true).CountAsync();
                    if (count == 5)
                    {
                        TempData[DShopConst.TEMPDATA_ERROR] = "Pin post fail. Maximum of 5 posts can be pinned ";
                        return RedirectToAction("Details", new { id = post.Id });
                    }
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Pin post successful " + post.Title;
                    post.IsPin = true;
                }
                else
                {
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "UnPin post successful " + post.Title;
                    post.IsPin = false;
                }
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { id= post.Id });
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Pin post fail: " + ex.Message;
                return RedirectToAction("Details", new { id = post.Id });
            }
        }


        [HttpPost]
        public async Task<IActionResult> LikeOrUnlikePost(int postId)
        {
            ViewBag.sidebar = Menu.Home.Blog;
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
                liked = new LikeModel() { PostId = post.Id, UserId = user.Id };
                await _context.Likes.AddAsync(liked);
                await _context.SaveChangesAsync();
                countLike = await _context.Likes.Where(p => p.PostId == post.Id).CountAsync();
                return Ok(new { success = true, message = "liked", count = countLike });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Like post fail " + ex.Message });

            }

        }

        [HttpPost]
        public async Task<IActionResult> CommentPost(int postId, string content)
        {
            ViewBag.sidebar = Menu.Home.Blog;
            var user = await _userManager.GetUserAsync(this.User);
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Status != 0 && p.Id == postId);
            if (post == null)
            {
                return NotFound();
            }
            if (String.IsNullOrEmpty(content))
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Comment is empty";
                return RedirectToAction("Details", new { id = post.Id });
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
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Comment post successful ";
                return RedirectToAction("Details", new { id = post.Id });
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Comment post fail " + ex.Message;
                return RedirectToAction("Details", new { id = post.Id });
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetPopUpReplyComment(int? Id)
        {
            ViewBag.sidebar = Menu.Home.Blog;
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
            return PartialView("_ModalReplyCommenttPartial", commentModel);
        }

        [HttpPost]
        public async Task<IActionResult> ReplyComment(int? Id, int postId, string ReplyMessage)
        {
            ViewBag.sidebar = Menu.Home.Blog;
            if (Id == null)
            {
                return NotFound();
            }
            var commentModel = await _context.Comments
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0);
            var postModel = await _context.Posts
                .FirstOrDefaultAsync(m => m.Id == postId && m.Status != 0);
            if (commentModel == null || postModel== null)
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


        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> DeleteComment(int? Id)
        {
            ViewBag.sidebar = Menu.Home.Blog;
            if (Id == null)
            {
                return NotFound();
            }
            var commentModel = await _context.Comments.Include(c => c.CommentChildren)
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status != 0) ;
            if (commentModel == null )
            {
                return NotFound();
            }
            try
            {
                if(commentModel?.CommentChildren.Count > 0)
                {
                    foreach (var item in commentModel.CommentChildren)
                    {
                        await DeleteComment(item.Id);
                    }
                }
                commentModel.Status = 0;
                _context.Comments.Update(commentModel);
                await _context.SaveChangesAsync();
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Delete comment successful ";
                return RedirectToAction("Details", new {  Id = commentModel.PostId });
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Delete comment fail "+ ex.Message;
                return RedirectToAction("Details", new { Id = commentModel.PostId });
            }
        }











        public async Task<IActionResult> SendPromotionToNewCustomer()
        {
            var userAdmin = await _userManager.GetUserAsync(this.User);
            if (userAdmin.UserName != DShopConst.ADMIN_DSHOP)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Access denied ";
                return RedirectToAction("Index");
            }

            var promotion = await _context.Promotions.FirstOrDefaultAsync(p => p.CategoryCouponName == DShopConst.NEW_CUSTOMER);
            if (promotion == null)
            {
                promotion = new PromotionModel { CategoryCouponName = DShopConst.NEW_CUSTOMER };
                await _context.Promotions.AddAsync(promotion);
                await _context.SaveChangesAsync();
            }
            //string email = "dacclone878787@gmail.com";
            string email = "dacclone888999@gmail.com";
            var user= await _userManager.FindByEmailAsync(email);

            CouponModel couponModel = new CouponModel
            {
                CouponName = "Promotion for new customer",
                CouponCode = "NEWCUSTOMER_" + user.UserName.ToUpper(),
                Value = 50000,
                DateStart = DateTime.Today,
                DateExpired = DateTime.Today.AddDays(7),
                Quantity = 1,
                Status = 1,
                Description = "Free shipping for new customers' first order",
                PromotionId = promotion.Id
            };

            try
            {

                var infoShop = await _context.InformationShops.FirstOrDefaultAsync();
                await _emailSender.SendEmailCouponForNewCustomer(user, couponModel, infoShop);
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Send coupon email successful " ;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Send coupon email fail " + ex.Message;
                return RedirectToAction("Index");
            }

        }







        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> SeedPostSubject()
        {
            var userAdmin = await _userManager.GetUserAsync(this.User);
            if (userAdmin.UserName != DShopConst.ADMIN_DSHOP)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Access denied ";
                return RedirectToAction("Index");
            }
            // remove old data
            _context.Subjects.RemoveRange(_context.Subjects.Where(c => c.SubjectContent.Contains("[fakeData]")));
            _context.Posts.RemoveRange(_context.Posts.Where(p => p.PostContent.Contains("[fakeData]")));


            await _context.SaveChangesAsync();
            //Subject
            var fakerSubject = new Faker<SubjectModel>();
            int cm = 1;
            fakerSubject.RuleFor(c => c.Title, fk => $"CM{cm++} " + fk.Lorem.Sentence(1, 2).Trim('.'));
            fakerSubject.RuleFor(c => c.SubjectContent, fk => fk.Lorem.Sentences(5) + "[fakeData]");
            fakerSubject.RuleFor(c => c.Slug, fk => fk.Lorem.Slug());

            var cate1 = fakerSubject.Generate();
            var cate11 = fakerSubject.Generate();
            var cate12 = fakerSubject.Generate();

            var cate2 = fakerSubject.Generate();
            var cate21 = fakerSubject.Generate();
            var cate211 = fakerSubject.Generate();

            cate11.ParentSubject = cate1;
            cate12.ParentSubject = cate1;
            cate21.ParentSubject = cate2;
            cate211.ParentSubject = cate21;

            var Subjects = new SubjectModel[] { cate1, cate2, cate12, cate11, cate21, cate211 };
            await _context.Subjects.AddRangeAsync(Subjects);

            // POST
            var author = new[] { "Samuel Clemens", "Eric Blair", "Charles Lutwidge Dodgson", "Charles Dickens", "Danielle Steel", "Mickey Spillane", "Ken Follett", "Stephenie Meyer" };
            var rCateIndex = new Random();
            int postExample = 1;

            var user = _userManager.GetUserAsync(User).Result;

            var fakerPost = new Faker<PostModel>();
            //fakerPost.RuleFor(p => p.Author, f => f.PickRandom(author));
            fakerPost.RuleFor(p => p.PostContent, f => f.Lorem.Paragraphs(7) + "[fakeData]");
            fakerPost.RuleFor(p => p.DateCreated, f => f.Date.Between(new DateTime(2025, 1, 1), new DateTime(2025, 7, 1)));
            fakerPost.RuleFor(p => p.ShortDescription, f => f.Lorem.Sentences(3));
            fakerPost.RuleFor(p => p.Slug, f => f.Lorem.Slug());
            fakerPost.RuleFor(p => p.Title, f => $"Post {postExample++} " + f.Lorem.Sentence(3, 4).Trim('.'));

            List<PostModel> posts = new List<PostModel>();
            List<PostSubjectModel> post_Subjects = new List<PostSubjectModel>();


            for (int i = 0; i < 40; i++)
            {
                var post = fakerPost.Generate();
                post.Status = 1;
                post.IsPin = false;
                post.DateUpdated = post.DateCreated;
                post.UserIdCreate = user.Id;
                posts.Add(post);
                post_Subjects.Add(new PostSubjectModel()
                {
                    Post = post,
                    Subject = Subjects[rCateIndex.Next(5)]
                });
            }

            await _context.AddRangeAsync(posts);
            await _context.AddRangeAsync(post_Subjects);
            // END POST


            await _context.SaveChangesAsync();
            TempData[DShopConst.TEMPDATA_SUCCESS] = "Seed data successful " ;
            return RedirectToAction("Index");
        }

        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> SeedOrder()
        {
            var userAdmin = await _userManager.GetUserAsync(this.User);
            if (userAdmin.UserName != DShopConst.ADMIN_DSHOP)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Access denied ";
                return RedirectToAction("Index");
            }
            try
            {
                List<AppUserModel> user = await (from u in _context.Users
                                join ur in _context.UserRoles on u.Id equals ur.UserId
                                join r in _context.Roles on ur.RoleId equals r.Id
                                where r.Name == RoleName.Customer
                                where u.Status != 0
                                select u).ToListAsync();

                List<ProductModel> products = await _context.Products.Where(p => p.Status != 0).OrderBy(p => p.Id).ToListAsync();
                Random rnd = new Random();
                DateTime dateTime = new DateTime(2025, 11, 1);
                DateTime startDate = new DateTime(2025, 11, 1); 
                DateTime endDate = new DateTime(2025, 11, 5); 
                int range = (endDate - startDate).Days;
                for (int i = 0; i < 4; i++)
                {
                    DateTime randomDate = dateTime.AddDays(rnd.Next(range+1));
                    var u = user[rnd.Next(user.Count)];
                    OrderModel orderModel = new OrderModel
                    {
                        OrderCode = Guid.NewGuid().ToString(),
                        UserId = u.Id,
                        CreatedDate = randomDate,
                        PaymentId = 1,
                        Status = 4,
                        AddressDelivery = "78 lang man_Xã Trác Văn_Thị xã Duy Tiên_Tỉnh Hà Nam",
                        PhoneDelivery = "0987654321",
                        Consignee = u.UserName,
                        ShippingCost = 50000,
                        ValueCoupon = 0,
                        DateUpdate = randomDate.AddDays(7),
                        UserIdUpdate = "57a1ccc4-504c-43f1-bdc2-a9b7f5c8dbbc"                    
                    };
                    await _context.Orders.AddAsync(orderModel);
                    await _context.SaveChangesAsync();
                    List<OrderDetailModel> orderDetails = new List<OrderDetailModel>();
                    for (int j = 0; j < 4; j++)
                    {
                        var p = products[rnd.Next(products.Count)];
                        OrderDetailModel od = new OrderDetailModel
                        {
                            Quantity = rnd.Next(1, 6),
                            Price = p .Price,
                            ProductId = p .Id,
                            OriginalPrice = p.OriginalPrice,
                            OrderId = orderModel.Id
                        };
                        orderDetails.Add(od);
                    }
                    await _context.OrderDetails.AddRangeAsync(orderDetails);
                    await _context.SaveChangesAsync();
                    var grandTotal = orderDetails.Sum(s => s.Quantity * s.Price) + orderModel.ShippingCost;
                    orderModel.GrandTotal = grandTotal;
                    _context.Orders.Update(orderModel);
                    await _context.SaveChangesAsync();
                }


                await _context.SaveChangesAsync();
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Seed data successful ";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Seed data fail " + ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Seed data fail " + ex.Message;
                return RedirectToAction("Index");
            }

        }
        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> DeleteOrderNoDetail()
        {
            var userAdmin = await _userManager.GetUserAsync(this.User);
            if (userAdmin.UserName != DShopConst.ADMIN_DSHOP)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Access denied ";
                return RedirectToAction("Index");
            }
            try
            {
                var listOrder = await _context.Orders.Include(od => od.OrderDetails).ToListAsync();

                foreach (var order in listOrder)
                {
                    var count = order.OrderDetails.Count();
                    if(count == 0)
                    {
                        _context.Orders.Remove(order);
                        await _context.SaveChangesAsync();
                    }
                }


                await _context.SaveChangesAsync();
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Seed data successful ";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Seed data fail " + ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Seed data fail " + ex.Message;
                return RedirectToAction("Index");
            }

        }

        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> SeedLike()
        {
            var userAdmin = await _userManager.GetUserAsync(this.User);
            if (userAdmin.UserName != DShopConst.ADMIN_DSHOP)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Access denied ";
                return RedirectToAction("Index");
            }
            try
            {
                List<AppUserModel> user = await (from u in _context.Users
                                                 join ur in _context.UserRoles on u.Id equals ur.UserId
                                                 join r in _context.Roles on ur.RoleId equals r.Id
                                                 where r.Name == RoleName.Customer
                                                 where u.Status != 0
                                                 select u).ToListAsync();

                List<PostModel> posts = await _context.Posts.Where(p => p.Status != 0).OrderBy(p => p.Id).ToListAsync();
                Random rnd = new Random();
                foreach (var item in posts)
                {
                    for (int i = 0; i < rnd.Next(3,8); i++)
                    {
                        var userId = user[rnd.Next(user.Count)].Id;
                        var checkLike = await _context.Likes.AnyAsync(p => p.UserId == userId && p.PostId == item.Id);
                        if (!checkLike)
                        {
                            LikeModel like = new LikeModel
                            {
                                UserId = userId,
                                PostId = item.Id
                            };
                            await _context.Likes.AddAsync(like);
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Seed data successful ";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Seed data fail " + ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Seed data fail " + ex.Message;
                return RedirectToAction("Index");
            }

        }

        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> SeedComment()
        {
            var userAdmin = await _userManager.GetUserAsync(this.User);
            if (userAdmin.UserName != DShopConst.ADMIN_DSHOP)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Access denied ";
                return RedirectToAction("Index");
            }
            try
            {
                List<AppUserModel> users = await (from u in _context.Users
                                                 join ur in _context.UserRoles on u.Id equals ur.UserId
                                                 join r in _context.Roles on ur.RoleId equals r.Id
                                                 where r.Name == RoleName.Customer
                                                 where u.Status != 0
                                                 select u).ToListAsync();

                List<PostModel> posts = await _context.Posts.Where(p => p.Status != 0).OrderBy(p => p.Id).ToListAsync();
                Random rnd = new Random();
                //DateTime dateTime = new DateTime(2025, 11, 1);
                //DateTime startDate = new DateTime(2025, 11, 1);
                //DateTime endDate = new DateTime(2025, 11, 5);
                //int range = (endDate - startDate).Days;
                //DateTime randomDate = dateTime.AddDays(rnd.Next(range + 1));

                var fakerComment = new Faker<CommentModel>();
                fakerComment.RuleFor(p => p.Timestamp, f => f.Date.Between(new DateTime(2025, 11, 1), new DateTime(2025,11, 27)));
                fakerComment.RuleFor(p => p.CommentContent, f => f.Lorem.Sentences(1));
                //fakerComment.RuleFor(p => p.ParentCommentId, f => f.PickRandom(listIdComment));
                List<CommentModel> comments = new List<CommentModel>();


                for (int i = 0; i < 60; i++)
                {
                    var comment = fakerComment.Generate();
                    comment.Status = 1;
                    var postId = posts[rnd.Next(posts.Count())].Id;
                    comment.PostId = postId;
                    comment.UserId = users[rnd.Next(users.Count())].Id;
                    var listIdComment = await _context.Comments.Where(c => c.Status != 0 && c.PostId == postId).Select(c => c.Id).ToListAsync();
                    comment.ParentCommentId = listIdComment[rnd.Next(listIdComment.Count())];
                    comments.Add(comment);
                }
                await _context.Comments.AddRangeAsync(comments);

                await _context.SaveChangesAsync();
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Seed data successful ";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Seed data fail 2" + ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Seed data fail3 " + ex.Message;
                return RedirectToAction("Index");
            }

        }


        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> SeedDescritp()
        {
            var userAdmin = await _userManager.GetUserAsync(this.User);
            if (userAdmin.UserName != DShopConst.ADMIN_DSHOP)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Access denied ";
                return RedirectToAction("Index");
            }
            try
            {
                string[] brands = new string[]{ "adidas", "bange" , "KF", "markryden", "msi", "osprey", "TNF" };

                string des = "description";

                var products = await _context.Products.Where(p => p.Status != 0).Where(p => p.Description.Contains("img"))
                                                        .ToListAsync();

                foreach (var item in products)
                {
                    if (item.Description.Contains("localhost:7213/contents/Product"))
                    {
                        foreach (var dBrand in brands)
                        {
                            if (item.Description.Contains(dBrand))
                            {
                                string newDes = item.Description.Replace(dBrand, des);
                                item.Description = newDes;
                                _context.Products.Update(item);
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Seed data successful ";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Seed data fail 2" + ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Seed data fail3 " + ex.Message;
                return RedirectToAction("Index");
            }

        }




        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> SeedMonth12()
        {
            var userAdmin = await _userManager.GetUserAsync(this.User);
            if (userAdmin.UserName != DShopConst.ADMIN_DSHOP)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Access denied ";
                return RedirectToAction("Index");
            }
            try
            {

                var ordersInMonth12 = await _context.Orders.Where(p => p.CreatedDate.Month == 12 && p.CreatedDate.Year == 2025)
                                                        .ToListAsync();
                foreach (var item in ordersInMonth12)
                {
                    var refund = await _context.Refunds.Where( o => o.OrderCode == item.OrderCode).FirstOrDefaultAsync();
                    if(refund != null)
                    {
                        _context.Refunds.Remove(refund);
                        await _context.SaveChangesAsync();
                    }

                    _context.Orders.Remove(item);
                    await _context.SaveChangesAsync();
                }


                await _context.SaveChangesAsync();
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Seed data successful ";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Seed data fail 2" + ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Seed data fail3 " + ex.Message;
                return RedirectToAction("Index");
            }

        }
    }
}
