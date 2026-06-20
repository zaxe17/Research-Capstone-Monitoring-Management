using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using monitoring_management.Models;
using monitoring_management.Services;

namespace monitoring_management.Controllers;

public class StudentController : Controller
{
    private readonly ApplicationDbContext _context;
    private const int PageSize = 5; // adjust mo kung ilan gusto mong papers per page

    public StudentController(ApplicationDbContext context)
    {
        _context = context;
    }

    private List<SidebarModel> GetSidebar()
    {
        return SidebarData.StudentMenu() ?? new List<SidebarModel>();
    }

    // Robust ownership check: matches UploadedBy (trimmed, case-insensitive)
    // OR matches a Leader entry in ResearchMembers for this paper, so a single
    // bad/stale UploadedBy value doesn't silently hide the Edit button.
    private static bool IsLeaderOf(ResearchPaper paper, string? studentId)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            return false;

        var sid = studentId.Trim();

        var uploadedByMatch = !string.IsNullOrWhiteSpace(paper.UploadedBy)
            && string.Equals(paper.UploadedBy.Trim(), sid, StringComparison.OrdinalIgnoreCase);

        var leaderMemberMatch = paper.ResearchMembers != null
            && paper.ResearchMembers.Any(m =>
                m.Role == MemberRole.Leader
                && !string.IsNullOrWhiteSpace(m.StudentId)
                && string.Equals(m.StudentId.Trim(), sid, StringComparison.OrdinalIgnoreCase));

        return uploadedByMatch || leaderMemberMatch;
    }

    private async Task SetCurrentPaperInfoAsync(string studentId)
    {
        var info = await _context.ResearchMembers
            .Include(m => m.ResearchPaper)
            .Where(m => m.StudentId == studentId)
            .OrderByDescending(m => m.ResearchPaper.DateUploaded)
            .Select(m => new
            {
                Role = m.Role.ToString(),
                Title = m.ResearchPaper.Title
            })
            .FirstOrDefaultAsync();

        ViewBag.CurrentPaperRole = info?.Role;
        ViewBag.CurrentPaperTitle = info?.Title;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        ViewBag.Sidebar = GetSidebar();

        var studentId = HttpContext.Session.GetString("StudentId");
        if (!string.IsNullOrEmpty(studentId))
        {
            await SetCurrentPaperInfoAsync(studentId);
        }

        var query = _context.ResearchPapers
            .Include(p => p.Category)
            .Include(p => p.Student)
            .Where(p => p.Status == PaperStatus.Approved)
            .OrderByDescending(p => p.DateUploaded);

        int totalCount = await query.CountAsync();
        int totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
        if (totalPages < 1) totalPages = 1;
        if (page < 1) page = 1;
        if (page > totalPages) page = totalPages;

        var papers = await query
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        ViewBag.Papers = papers;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalCount = totalCount;

        return View();
    }

    public async Task<IActionResult> MyWorks()
    {
        ViewBag.Sidebar = GetSidebar();

        var studentId = HttpContext.Session.GetString("StudentId");

        if (string.IsNullOrEmpty(studentId))
            return RedirectToAction("Index", "Login");

        await SetCurrentPaperInfoAsync(studentId);

        var papers = await _context.ResearchMembers
            .Include(m => m.ResearchPaper).ThenInclude(p => p.Category)
            .Where(m => m.StudentId == studentId)
            .Select(m => new
            {
                PaperId = m.ResearchPaper.PaperId,
                Title = m.ResearchPaper.Title,
                Role = m.Role.ToString(),
                Category = m.ResearchPaper.Category != null
                    ? m.ResearchPaper.Category.CategoryName
                    : "Uncategorized",
                Status = m.ResearchPaper.Status.ToString().ToUpper()
            })
            .ToListAsync();

        ViewBag.AsLeader = papers.Count(p => p.Role == "Leader");
        ViewBag.AsMember = papers.Count(p => p.Role == "Member");
        ViewBag.PendingCount = papers.Count(p => p.Status == "PENDING");
        ViewBag.ApprovedCount = papers.Count(p => p.Status == "APPROVED");
        ViewBag.DraftCount = papers.Count(p => p.Status == "DRAFT");

        ViewBag.Papers = papers;

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> PaperDetails(string id)
    {
        ViewBag.Sidebar = GetSidebar();

        var studentId = HttpContext.Session.GetString("StudentId");
        if (string.IsNullOrEmpty(studentId))
            return RedirectToAction("Index", "Login");

        var paper = await _context.ResearchPapers
            .Include(p => p.Category)
            .Include(p => p.Student)
            .Include(p => p.ResearchMembers)
                .ThenInclude(m => m.Student)
            .FirstOrDefaultAsync(p => p.PaperId == id);

        if (paper == null)
            return NotFound();

        // Edit button only shows for the leader of this paper, regardless of
        // entry point (Repository or My Works) and regardless of Status
        // (Pending or Approved both editable).
        ViewBag.IsOwner = IsLeaderOf(paper, studentId);

        return View(paper);
    }

    // Loads the existing paper into the same-shaped view model used by Submit,
    // so Edit_submit.cshtml can reuse the Submit form layout pre-filled with current data.
    // Works regardless of Status (Pending or Approved) — only the leader can edit.
    [HttpGet]
    public async Task<IActionResult> EditSubmit(string id)
    {
        ViewBag.Sidebar = GetSidebar();

        var studentId = HttpContext.Session.GetString("StudentId");
        if (string.IsNullOrEmpty(studentId))
            return RedirectToAction("Index", "Login");

        var paper = await _context.ResearchPapers
            .Include(p => p.ResearchMembers)
                .ThenInclude(m => m.Student)
            .FirstOrDefaultAsync(p => p.PaperId == id);

        if (paper == null)
            return NotFound();

        if (!IsLeaderOf(paper, studentId))
        {
            TempData["ErrorMessage"] = "Only the leader who submitted this paper can edit it.";
            return RedirectToAction("PaperDetails", new { id });
        }

        var leaderMember = paper.ResearchMembers.FirstOrDefault(m => m.Role == MemberRole.Leader);
        var existingMembers = paper.ResearchMembers
            .Where(m => m.Role != MemberRole.Leader)
            .ToList();

        var model = new SubmitResearchViewModel
        {
            Title = paper.Title,
            Description = paper.Description,
            CategoryId = paper.CategoryId,
            Year = paper.Year,
            Keywords = paper.Keywords
        };

        ViewBag.PaperId = paper.PaperId;
        ViewBag.LeaderMember = leaderMember;
        ViewBag.ExistingMembers = existingMembers;
        ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
        ViewBag.Years = GetAcademicYearOptions();

        // Explicit view name so this renders Views/Student/Edit_submit.cshtml
        // (action name "EditSubmit" would otherwise default to "EditSubmit.cshtml").
        return View("Edit_submit", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSubmit(string id, SubmitResearchViewModel model)
    {
        ViewBag.Sidebar = GetSidebar();

        var studentId = HttpContext.Session.GetString("StudentId");
        if (string.IsNullOrEmpty(studentId))
            return RedirectToAction("Index", "Login");

        var paper = await _context.ResearchPapers
            .Include(p => p.ResearchMembers)
                .ThenInclude(m => m.Student)
            .FirstOrDefaultAsync(p => p.PaperId == id);

        if (paper == null)
            return NotFound();

        if (!IsLeaderOf(paper, studentId))
        {
            TempData["ErrorMessage"] = "Only the leader who submitted this paper can edit it.";
            return RedirectToAction("PaperDetails", new { id });
        }

        if (!ModelState.IsValid)
        {
            ViewBag.PaperId = id;
            ViewBag.LeaderMember = paper.ResearchMembers.FirstOrDefault(m => m.Role == MemberRole.Leader);
            ViewBag.ExistingMembers = paper.ResearchMembers.Where(m => m.Role != MemberRole.Leader).ToList();
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            ViewBag.Years = GetAcademicYearOptions();
            return View("Edit_submit", model);
        }

        // Update paper details. Status is intentionally left untouched here —
        // editing content does not change whether it's Pending/Approved/Draft.
        await _context.Database.ExecuteSqlRawAsync(
            @"UPDATE research_papers
              SET title = {0}, description = {1}, keywords = {2}, category_id = {3}, year = {4}
              WHERE paper_id = {5}",
            model.Title, model.Description, model.Keywords, model.CategoryId, model.Year, id);

        // Replace the non-leader member list with whatever was submitted on the form.
        // NOTE: assumes the role column stores the literal strings "Leader"/"Member",
        // matching how the original Submit POST inserts these values.
        await _context.Database.ExecuteSqlRawAsync(
            @"DELETE FROM research_members WHERE paper_id = {0} AND role <> 'Leader'",
            id);

        if (model.Members != null)
        {
            foreach (var m in model.Members)
            {
                if (string.IsNullOrWhiteSpace(m.MemberName))
                    continue;

                await _context.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO research_members (paper_id, student_id, member_name, role)
                      VALUES ({0}, NULLIF({1}, ''), {2}, {3})",
                    id,
                    m.StudentId ?? string.Empty,
                    m.MemberName,
                    "Member");
            }
        }

        TempData["SuccessMessage"] = "Changes saved.";
        return RedirectToAction("PaperDetails", new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Submit()
    {
        ViewBag.Sidebar = GetSidebar();

        var studentId = HttpContext.Session.GetString("StudentId");

        if (string.IsNullOrEmpty(studentId))
            return RedirectToAction("Index", "Login");

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.StudentId == studentId);

        if (student == null)
            return RedirectToAction("Index", "Login");

        await SetCurrentPaperInfoAsync(studentId);

        var leaderMember = new ResearchMember
        {
            MemberId   = string.Empty,
            PaperId    = string.Empty,
            StudentId  = student.StudentId,
            Student    = student,
            MemberName = student.FullName,
            Role       = MemberRole.Leader
        };

        ViewBag.LeaderMember = leaderMember;
        ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
        ViewBag.Years = GetAcademicYearOptions();

        return View(new SubmitResearchViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(SubmitResearchViewModel model)
    {
        ViewBag.Sidebar = GetSidebar();

        var studentId = HttpContext.Session.GetString("StudentId");

        if (string.IsNullOrEmpty(studentId))
            return RedirectToAction("Index", "Login");

        var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == studentId);
        if (student == null)
            return RedirectToAction("Index", "Login");

        if (!ModelState.IsValid)
        {
            await SetCurrentPaperInfoAsync(studentId);
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            ViewBag.Years = GetAcademicYearOptions();
            ViewBag.LeaderMember = new ResearchMember
            {
                StudentId  = student.StudentId,
                Student    = student,
                MemberName = student.FullName,
                Role       = MemberRole.Leader
            };
            return View(model);
        }

        var status = model.Status == "Draft" ? "Draft" : "Pending";

        await _context.Database.ExecuteSqlRawAsync(
            @"INSERT INTO research_papers (title, description, keywords, category_id, year, uploaded_by, status)
              VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})",
            model.Title, model.Description, model.Keywords, model.CategoryId, model.Year, studentId, status);

        var newPaperId = await _context.ResearchPapers
            .Where(p => p.UploadedBy == studentId)
            .OrderByDescending(p => p.DateUploaded)
            .Select(p => p.PaperId)
            .FirstOrDefaultAsync();

        if (!string.IsNullOrEmpty(newPaperId))
        {
            await _context.Database.ExecuteSqlRawAsync(
                @"INSERT INTO research_members (paper_id, student_id, member_name, role)
                  VALUES ({0}, {1}, {2}, {3})",
                newPaperId, studentId, student.FullName, "Leader");

            if (model.Members != null)
            {
                foreach (var m in model.Members)
                {
                    if (string.IsNullOrWhiteSpace(m.MemberName))
                        continue;

                    await _context.Database.ExecuteSqlRawAsync(
                        @"INSERT INTO research_members (paper_id, student_id, member_name, role)
                          VALUES ({0}, NULLIF({1}, ''), {2}, {3})",
                        newPaperId,
                        m.StudentId ?? string.Empty,
                        m.MemberName,
                        "Member");
                }
            }
        }

        TempData["SuccessMessage"] = status == "Draft"
            ? "Saved as draft."
            : "Research submitted for review.";

        return RedirectToAction("MyWorks");
    }

    [HttpGet]
    public async Task<IActionResult> SearchStudents(string term)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            return Json(new List<object>());

        var currentStudentId = HttpContext.Session.GetString("StudentId");

        var results = await _context.Students
            .Where(s => s.StudentId != currentStudentId &&
                        (s.FullName.Contains(term) || s.StudentNo.Contains(term)))
            .OrderBy(s => s.FullName)
            .Take(8)
            .Select(s => new
            {
                studentId = s.StudentId,
                fullName  = s.FullName,
                studentNo = s.StudentNo
            })
            .ToListAsync();

        return Json(results);
    }

    private static List<int> GetAcademicYearOptions()
    {
        int currentYear = DateTime.Now.Year;
        var years = new List<int>();
        for (int y = currentYear - 2; y <= currentYear + 1; y++)
            years.Add(y);
        return years;
    }
}