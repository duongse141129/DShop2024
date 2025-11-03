using App.Utilities;
using AppMvc.Areas.Blog.Models;
using Bogus;
using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.Models.Blog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AppMvc.Areas.Blog.Controllers
{
    [Area("Blog")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
    public class PostController : Controller
    {
        private readonly DShopContext _context;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string sidebar = "blog";

        public PostController(DShopContext context, IWebHostEnvironment webHostEnvironment, UserManager<AppUserModel> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string search,string subject_by, [FromQuery(Name = "p")]int currentPage, int pagesSize)
        {
            ViewBag.sidebar = Menu.Admin.Blog;
            IQueryable<PostModel> posts = _context.Posts.Where(p => p.Status!= 0)
                                        .Include(p => p.CreateBy)
                                        .Include(p => p.PostSubjects)
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
                        .Include(p => p.PostSubjects)
                        .ThenInclude(pc =>pc.Subject)
                        .OrderByDescending(p => p.IsPin)
                        .ThenByDescending(p => p.DateUpdated)
                        .ToListAsync();
            return View(postsInPage);
        }


        public async Task<IActionResult> Details(int? id)
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
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
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
                                if (System.IO.File.Exists(oldFilePath))
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
                        if (System.IO.File.Exists(oldFilePath))
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



        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> SeedPostSubject()
        {
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

            var user = _userManager.GetUserAsync(this.User).Result;
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
        public async Task<IActionResult> SeedWishList()
        {
            try
            {
                var customers = await (from u in _context.Users
                                       join ur in _context.UserRoles on u.Id equals ur.UserId
                                       join r in _context.Roles on ur.RoleId equals r.Id
                                       where r.Name == RoleName.Customer
                                       select u).ToListAsync();
                //var products = await _context.Products.Where(p => p.Status != 0 && p.Stock >0).Select(g => new { id = g.Id }).ToListAsync();
                Random rnd = new Random();
                foreach (var cus in customers)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        int productId = rnd.Next(32, 82);
                        var check = await _context.WishLists.Where(u => u.UserId == cus.Id && u.ProductId == productId).AnyAsync();
                        if (check != true)
                        {
                            WishListModel wish = new WishListModel
                            {
                                UserId = cus.Id,
                                ProductId = productId,
                            };
                            await _context.WishLists.AddAsync(wish);
                            await _context.SaveChangesAsync();
                        }
                    }

                }
                await _context.SaveChangesAsync();
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Seed data successful ";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Seed data fail "+ ex.Message;
                return RedirectToAction("Index");
            }

        }

        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> SeedCreateProduct()
        {
            try
            {
                var products = await _context.Products.Where(p => p.Status != 0).OrderBy(p => p.Id).ToListAsync();
                Random rnd = new Random();
                DateTime dateTime = new DateTime(2024, 1, 1);
                foreach (var product in products)
                {
                    dateTime = dateTime.AddDays(rnd.Next(1,20));
                    if(dateTime > DateTime.Now)
                    {
                        dateTime = DateTime.Now;
                    }
                    product.CreateDate = dateTime;
                    _context.Products.Update(product);
                    await _context.SaveChangesAsync();
                }
                await _context.SaveChangesAsync();
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Seed data successful ";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Seed data fail " + ex.Message;
                return RedirectToAction("Index");
            }

        }




    }
}
