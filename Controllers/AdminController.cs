using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using monitoring_management.Models;
using monitoring_management.Services;

namespace monitoring_management.Controllers;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
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

    // FIX: dati "int id" yung parameter, pero "string" pala yung actual type
    // ng PaperId sa model (CS0019 error). Ginawang "string id" para tumugma.
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

    // FIX: "Id" dito ginawang "string" din para tumugma sa PaperId type.
    public class UpdateStatusRequest
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest request)
    {
        if (!Enum.TryParse<PaperStatus>(request.Status, true, out var status))
        {
            return BadRequest("Invalid status value.");
        }

        var paper = await _context.ResearchPapers.FindAsync(request.Id);
        if (paper == null)
        {
            return NotFound();
        }

        paper.Status = status;
        await _context.SaveChangesAsync();

        return Ok();
    }
}