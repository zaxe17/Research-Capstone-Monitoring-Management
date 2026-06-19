using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using monitoring_management.Models;
using monitoring_management.Services;

namespace monitoring_management.Controllers;

public class StudentController : Controller
{
    private readonly ApplicationDbContext _context;

    public StudentController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var sidebar = SidebarData.StudentMenu();

        var papers = await _context.ResearchPapers
            .Include(p => p.Category)
            .Include(p => p.Student)
            .Where(p => p.Status == PaperStatus.Approved)
            .OrderByDescending(p => p.DateUploaded)
            .Select(p => new
            {
                Title = p.Title,
                Category = p.Category != null ? p.Category.CategoryName : "Uncategorized",
                Year = p.Year,
                UploadedBy = p.Student != null ? p.Student.FullName : "Unknown",
                Status = p.Status.ToString().ToUpper()
            })
            .ToListAsync();

        ViewBag.Papers = papers;
        ViewBag.Sidebar = sidebar;

        return View();
    }

    public async Task<IActionResult> MyWorks()
    {
        var sidebar = SidebarData.StudentMenu();
        var studentId = HttpContext.Session.GetString("StudentId");

        if (string.IsNullOrEmpty(studentId))
            return RedirectToAction("Index", "Login");

        var papers = await _context.ResearchMembers
            .Include(m => m.ResearchPaper).ThenInclude(p => p.Category)
            .Where(m => m.StudentId == studentId)
            .Select(m => new
            {
                Title = m.ResearchPaper.Title,
                Role = m.Role.ToString(),
                Category = m.ResearchPaper.Category != null ? m.ResearchPaper.Category.CategoryName : "Uncategorized",
                Status = m.ResearchPaper.Status.ToString().ToUpper()
            })
            .ToListAsync();

        // i-compute dito sa controller
        ViewBag.AsLeader = papers.Count(p => p.Role == "Leader");
        ViewBag.AsMember = papers.Count(p => p.Role == "Member");
        ViewBag.PendingCount = papers.Count(p => p.Status == "PENDING");
        ViewBag.ApprovedCount = papers.Count(p => p.Status == "APPROVED");
        ViewBag.DraftCount = papers.Count(p => p.Status == "DRAFT");

        ViewBag.Papers = papers;
        ViewBag.Sidebar = sidebar;

        return View();
    }

    public async Task<IActionResult> Submit()
    {
        var sidebar = SidebarData.StudentMenu();
        var studentId = HttpContext.Session.GetString("StudentId");

        if (string.IsNullOrEmpty(studentId))
            return RedirectToAction("Index", "Login");

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.StudentId == studentId);

        ViewBag.Sidebar = sidebar;
        ViewBag.CurrentStudent = student;
        ViewBag.MemberRole = "Leader"; // or dynamic later

        return View();
    }
}