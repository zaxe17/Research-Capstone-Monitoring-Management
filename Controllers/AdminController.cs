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
        // DIAGNOSTIC: confirm which physical DB/schema this DbContext is actually hitting.
        var dbName = _context.Database.GetDbConnection().Database;
        var dataSource = _context.Database.GetDbConnection().DataSource;
        _logger.LogInformation("[UpdateStatus] Connected DB={DbName} Source={DataSource} IncomingId='{Id}' Status='{Status}'",
            dbName, dataSource, request.Id, request.Status);

        if (string.IsNullOrWhiteSpace(request.Id))
        {
            _logger.LogWarning("[UpdateStatus] Empty/null Id received.");
            return BadRequest("Missing paper id.");
        }

        if (!Enum.TryParse<PaperStatus>(request.Status, true, out var status))
        {
            _logger.LogWarning("[UpdateStatus] Invalid status value: '{Status}'", request.Status);
            return BadRequest("Invalid status value.");
        }

        var trimmedId = request.Id.Trim();

        var paper = await _context.ResearchPapers
            .FirstOrDefaultAsync(p => p.PaperId == trimmedId);

        if (paper == null)
        {
            _logger.LogWarning("[UpdateStatus] No paper found in DB '{DbName}' with PaperId='{Id}'", dbName, trimmedId);
            return NotFound($"No paper found with id '{trimmedId}' in database '{dbName}'.");
        }

        var oldStatus = paper.Status;
        paper.Status = status;

        var rows = await _context.SaveChangesAsync();

        _logger.LogInformation("[UpdateStatus] PaperId={Id} {OldStatus} -> {NewStatus} | RowsAffected={Rows} | DB={DbName}",
            trimmedId, oldStatus, status, rows, dbName);

        return Ok(new { rowsAffected = rows, db = dbName, paperId = trimmedId, newStatus = status.ToString() });
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

        // ResearchMember -> ResearchPaper FK has DeleteBehavior.Cascade configured
        // in OnModelCreating; ResearchMembers is already tracked via Include, so
        // EF cascades the delete automatically.
        _context.ResearchPapers.Remove(paper);
        await _context.SaveChangesAsync();

        return Ok();
    }

    public async Task<IActionResult> ManageStudent()
    {
        var sidebar = SidebarData.StudentMenu();
        ViewBag.Sidebar = sidebar;

        return View();
    }
}