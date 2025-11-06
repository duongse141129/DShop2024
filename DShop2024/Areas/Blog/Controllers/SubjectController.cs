using App.Utilities;
using DShop2024.EnumData;
using DShop2024.Models.Blog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;



namespace DShop2024.Areas.Blog.Controllers
{
    [Area("Blog")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
    public class SubjectController : Controller
    {
        private readonly DShopContext _context;
        private readonly string sidebar = "blog";

        public SubjectController(DShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.sidebar = Menu.Admin.Blog;
            var qr = (from c in _context.Subjects select c).Where(c => c.Status != 0)
                .Include(c => c.ParentSubject)
                .Include(c => c.SubjectChildren);

            var subjects = (await qr.ToListAsync())
                            .Where(c => c.ParentSubject == null && c.Status != 0)
                            .ToList();
                
            return View(subjects);
        }

        public async Task<IActionResult> Details(int? id)
        {
            ViewBag.sidebar = Menu.Admin.Blog;
            if (id == null)
            {
                return NotFound();
            }

            var subject = await _context.Subjects.Where(c => c.Status != 0)
                .Include(c => c.ParentSubject)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subject == null)
            {
                return NotFound();
            }

            return View(subject);
        }

        public async Task<IActionResult> SearchTitle(String search)
        {
            ViewBag.sidebar = Menu.Admin.Blog;
            if (String.IsNullOrEmpty(search))
            {
                return RedirectToAction("Index");
            }
            var subject = await _context.Subjects.Where(c => c.Status != 0)
                            .Include(c => c.ParentSubject)
                            .FirstOrDefaultAsync(m => m.Title.Contains(search));
            if (subject == null)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Not found ";
                return RedirectToAction("Index");
            }
            return RedirectToAction("Details", new {id = subject.Id });

        }

        private void CreateSelectItems(List<SubjectModel> source, List<SubjectModel> des, int level)
        {
            string prefix = string.Concat(Enumerable.Repeat("----", level));
            foreach (var subject in source)
            {
                des.Add(new SubjectModel()
                {
                    Id = subject.Id,
                    Title = prefix + " " + subject.Title
                });
                if (subject.SubjectChildren?.Count > 0)
                {
                    CreateSelectItems(subject.SubjectChildren.Where(s => s.Status!=0).ToList(), des, level + 1);
                }

            }
        }

        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> CreateAsync()
        {
            ViewBag.sidebar = Menu.Admin.Blog;
            var qr = (from c in _context.Subjects select c)
                .Where(s => s.Status != 0)
                .Include(c => c.ParentSubject)
                .Include(c => c.SubjectChildren);

            var subjects = (await qr.Where(s => s.Status != 0).ToListAsync())
                                    .Where(c => c.ParentSubject == null)
                                    .ToList();
            subjects.Insert(0, new SubjectModel()
            {
                Id = -1,
                Title = "There isn't parent subject."

            });

            var items = new List<SubjectModel>();
            CreateSelectItems(subjects, items, 0);
            var selectList = new SelectList(items, "Id", "Title");
            ViewBag.ParentSubjectId = selectList;
            return View();
        }

        [Authorize(Roles = RoleName.Administrator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,SubjectContent,ParentSubjectId")] SubjectModel subject)
        {
            ViewBag.sidebar = Menu.Admin.Blog;
            if (ModelState.IsValid)
            {
                try
                {
                    if (subject.ParentSubjectId == -1) subject.ParentSubjectId = null;
                    subject.Slug = AppUtilities.GenerateSlug(subject.Title);
                    if (await _context.Posts.AnyAsync(p => p.Slug == subject.Slug))
                    {
                        ModelState.AddModelError("Slug", "This url subject already exists.");
                        return View(subject);
                    }
                    subject.Status = 1;
                    _context.Add(subject);
                    await _context.SaveChangesAsync();
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Create subject successful ";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "Create subject fail "+ ex.Message;
                    return View(subject);
                }

            }
            var qr = (from c in _context.Subjects select c).Where(s => s.Status != 0)
                    .Include(c => c.ParentSubject)
                    .Include(c => c.SubjectChildren);

            var Subjects = (await qr.ToListAsync()).Where(s => s.Status != 0)
                            .Where(c => c.ParentSubject == null)
                            .ToList();
            Subjects.Insert(0, new SubjectModel()
            {
                Id = -1,
                Title = "There isn't parent subject."

            });

            var items = new List<SubjectModel>();
            CreateSelectItems(Subjects, items, 0);
            var selectList = new SelectList(items, "Id", "Title");
            ViewData["ParentSubjectId"] = selectList;
            return View(subject);
        }

        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> Edit(int? id)
        {
            ViewBag.sidebar = Menu.Admin.Blog;
            if (id == null)
            {
                return NotFound();
            }

            var Subject = await _context.Subjects.Where(s => s.Status != 0)
                            .FirstOrDefaultAsync(s=> s.Id == id );
            if (Subject == null)
            {
                return NotFound();
            }

            var qr = (from c in _context.Subjects select c).Where(s => s.Status != 0)
                .Include(c => c.ParentSubject)
                .Include(c => c.SubjectChildren);

            var subjects = (await qr.Where(s => s.Status != 0)
                            .ToListAsync())
                            .Where(c => c.ParentSubject == null)
                            .ToList();
            subjects.Insert(0, new SubjectModel()
            {
                Id = -1,
                Title = "There isn't parent subject."

            });

            var items = new List<SubjectModel>();
            CreateSelectItems(subjects, items, 0);
            var selectList = new SelectList(items, "Id", "Title");

            ViewBag.ParentSubjectId = selectList;
            return View(Subject);
        }

        [Authorize(Roles = RoleName.Administrator)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,SubjectContent,Status,ParentSubjectId")] SubjectModel subject)
        {
            ViewBag.sidebar = Menu.Admin.Blog;
            if (id != subject.Id)
            {
                return NotFound();
            }

            bool canUpdate = true;
            if (subject.ParentSubjectId == subject.Id)
            {
                ModelState.AddModelError(string.Empty, "Must select another parent subject. Can not choose itself as the parent subject");
            }

            // check list parent subject 
            if (canUpdate && subject.ParentSubjectId != null)
            {
                var childSubs =
                            (from c in _context.Subjects select c)
                            .Include(c => c.SubjectChildren)
                            .ToList()
                            .Where(c => c.ParentSubjectId == subject.Id);


                // Func check Id 
                Func<List<SubjectModel>, bool> checkSubIds = null;
                checkSubIds = (subs) =>
                {
                    foreach (var sub in subs)
                    {
                        Console.WriteLine(sub.Title);
                        if (sub.Id == subject.ParentSubjectId)
                        {
                            canUpdate = false;
                            ModelState.AddModelError(string.Empty, "Must select another parent subject. Can not choose its child as parent subject");
                            return true;
                        }
                        if (sub.SubjectChildren != null)
                            return checkSubIds(sub.SubjectChildren.ToList());

                    }
                    return false;
                };
                // End Func 
                checkSubIds(childSubs.ToList());
            }

            if (ModelState.IsValid && canUpdate)
            {
                try
                {
                    if (subject.ParentSubjectId == -1)
                        subject.ParentSubjectId = null;

                    var dtc = _context.Subjects.Where(s => s.Status != 0).FirstOrDefault(c => c.Id == id);
                    subject.Slug = AppUtilities.GenerateSlug(subject.Title);
                    if (await _context.Posts.AnyAsync(p => p.Slug == subject.Slug))
                    {
                        ModelState.AddModelError("Slug", "This url subject already exists.");
                        return View(subject);
                    }

                    _context.Entry(dtc).State = EntityState.Detached;
                    _context.Update(subject);
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Edit subject successful ";
                    await _context.SaveChangesAsync();                    
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubjectExists(subject.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        TempData[DShopConst.TEMPDATA_ERROR] = "Edit subject fail ";
                        return View(subject);
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            var qr = (from c in _context.Subjects select c).Where(s => s.Status != 0)
                .Include(c => c.ParentSubject)
                .Include(c => c.SubjectChildren);

            var Subjects = (await qr.ToListAsync()).Where(s => s.Status != 0)
                            .Where(c => c.ParentSubject == null)
                            .ToList();
            Subjects.Insert(0, new SubjectModel()
            {
                Id = -1,
                Title = "There isn't parent subject."

            });

            var items = new List<SubjectModel>();
            CreateSelectItems(Subjects, items, 0);
            var selectList = new SelectList(items, "Id", "Title");

            ViewData["ParentSubjectId"] = selectList;
            return View(subject);
        }

        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> Delete(int? id)
        {
            ViewBag.sidebar = Menu.Admin.Blog;
            if (id == null)
            {
                return NotFound();
            }

            var subject = await _context.Subjects
                .Include(c => c.ParentSubject)
                 .Include(c => c.SubjectChildren)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subject == null)
            {
                return NotFound();
            }
            try
            {
                foreach (var cSubject in subject.SubjectChildren)
                {
                    cSubject.ParentSubjectId = subject.ParentSubjectId;
                }

                _context.Subjects.Remove(subject);
                await _context.SaveChangesAsync();
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Delete subject successful ";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Delete subject fail "+ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }




        private bool SubjectExists(int id)
        {
            return _context.Subjects.Any(e => e.Id == id);
        }



    }
}
