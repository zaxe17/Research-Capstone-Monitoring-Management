using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using monitoring_management.Models;
using monitoring_management.Services;

namespace monitoring_management.Controllers;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var sidebar = SidebarData.AdminMenu();
        ViewBag.Sidebar = sidebar;

        ViewBag.TotalPapers    = await _context.ResearchPapers.CountAsync();
        ViewBag.PendingCount   = await _context.ResearchPapers.CountAsync(p => p.Status == PaperStatus.Pending);
        ViewBag.ApprovedCount  = await _context.ResearchPapers.CountAsync(p => p.Status == PaperStatus.Approved);
        ViewBag.RejectedCount  = await _context.ResearchPapers.CountAsync(p => p.Status == PaperStatus.Rejected);

        var papers = await _context.ResearchPapers
            .Include(p => p.Category)
            .Include(p => p.Student)
            .Include(p => p.ResearchMembers)
            .OrderByDescending(p => p.DateUploaded)
            .ToListAsync();

        return View(papers);
    }

    public async Task<IActionResult> Details(string id)
    {
        var paper = await _context.ResearchPapers
            .Include(p => p.Category)
            .Include(p => p.Student)
            .Include(p => p.ResearchMembers)
            .FirstOrDefaultAsync(p => p.PaperId == id);

        if (paper == null)
        {
            return NotFound();
        }

        ViewBag.Sidebar = SidebarData.AdminMenu();
        ViewBag.ReturnUrl = Url.Action("Index", "Admin");

        return View("~/Views/Repository/Details.cshtml", paper);
    }

    public class UpdateStatusRequest
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Id))
        {
            return BadRequest("Missing paper id.");
        }

        if (!Enum.TryParse<PaperStatus>(request.Status, true, out var status))
        {
            return BadRequest("Invalid status value.");
        }

        var paper = await _context.ResearchPapers.FindAsync(request.Id.Trim());
        if (paper == null)
        {
            return NotFound();
        }

        paper.Status = status;
        await _context.SaveChangesAsync();

        return Ok();
    }

    // FIX: dati Approved-only ang query at may year filter. Ngayon lahat ng
    // status ay nakikita by default (search/category/status filters), dahil
    // ang page mismo ay para mismo sa review/approve/reject/remove workflow.
    public async Task<IActionResult> ManagePapers(string search, string category, string status, int page = 1)
    {
        ViewBag.Sidebar = SidebarData.AdminMenu();

        const int pageSize = 10;

        var query = _context.ResearchPapers
            .Include(p => p.Category)
            .Include(p => p.Student)
            .Include(p => p.ResearchMembers)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                p.Title.Contains(search) ||
                (p.Keywords != null && p.Keywords.Contains(search)) ||
                p.ResearchMembers.Any(m => m.MemberName.Contains(search)) ||
                (p.Student != null && p.Student.FullName.Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category != null && p.Category.CategoryName == category);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaperStatus>(status, true, out var statusFilter))
        {
            query = query.Where(p => p.Status == statusFilter);
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        page = Math.Max(1, Math.Min(page, Math.Max(totalPages, 1)));

        var papers = await query
            .OrderByDescending(p => p.DateUploaded)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.SearchTerm = search;
        ViewBag.SelectedCategory = category;
        ViewBag.SelectedStatus = status;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        return View(papers);
    }

    public class DeletePaperRequest
    {
        public string Id { get; set; } = string.Empty;
    }

    [HttpPost]
    public async Task<IActionResult> DeletePaper([FromBody] DeletePaperRequest request)
    {
        var paper = await _context.ResearchPapers
            .Include(p => p.ResearchMembers)
            .FirstOrDefaultAsync(p => p.PaperId == request.Id);

        if (paper == null)
        {
            return NotFound();
        }

        _context.ResearchPapers.Remove(paper);
        await _context.SaveChangesAsync();

        return Ok();
    }

    // DB-driven Manage Students page.
    // - LinkedPapersCount = distinct papers where student is uploader OR named member (display only).
    // - HasUploadedPapers = the actual delete-block condition, since research_papers.uploaded_by
    //   is a required (non-nullable) FK. Being a member-only (ResearchMembers.StudentId) does NOT
    //   block delete, since that FK is nullable and gets unlinked instead.
    public async Task<IActionResult> ManageStudent(string search, int page = 1)
    {
        ViewBag.Sidebar = SidebarData.AdminMenu();

        const int pageSize = 10;

        var query = _context.Students
            .Include(s => s.ResearchPapers)
            .Include(s => s.ResearchMembers)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s =>
                s.FullName.Contains(search) ||
                s.StudentNo.Contains(search) ||
                s.Email.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        page = Math.Max(1, Math.Min(page, Math.Max(totalPages, 1)));

        var students = await query
            .OrderBy(s => s.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var rows = students.Select(s => new StudentRowViewModel
        {
            StudentId = s.StudentId,
            StudentNo = s.StudentNo,
            FullName = s.FullName,
            Email = s.Email,
            LinkedPapersCount = s.ResearchPapers.Select(p => p.PaperId)
                .Union(s.ResearchMembers.Select(m => m.PaperId))
                .Distinct()
                .Count(),
            HasUploadedPapers = s.ResearchPapers.Any()
        }).ToList();

        ViewBag.SearchTerm = search;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        return View(rows);
    }

    public class DeleteStudentRequest
    {
        public string Id { get; set; } = string.Empty;
    }

    [HttpPost]
    public async Task<IActionResult> DeleteStudent([FromBody] DeleteStudentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Id))
        {
            return BadRequest("Missing student id.");
        }

        var studentId = request.Id.Trim();

        var student = await _context.Students
            .Include(s => s.ResearchPapers)
            .Include(s => s.ResearchMembers)
            .FirstOrDefaultAsync(s => s.StudentId == studentId);

        if (student == null)
        {
            return NotFound();
        }

        if (student.ResearchPapers.Any())
        {
            return BadRequest("Cannot delete: student has uploaded research paper(s). Remove or reassign those papers first.");
        }

        foreach (var member in student.ResearchMembers)
        {
            member.StudentId = null;
        }

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();

        return Ok();
    }
}