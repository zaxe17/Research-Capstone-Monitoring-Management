using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using monitoring_management.Models;
using monitoring_management.Services;

namespace monitoring_management.Controllers;

public class RepositoryController : Controller
{
    private readonly ApplicationDbContext _context;
    private const int PageSize = 5; // adjust mo kung ilan gusto mong papers per page

    public RepositoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, int? categoryId, int? year, int page = 1)
    {
        var sidebar = SidebarData.StudentMenu();
        ViewBag.Sidebar = sidebar;

        var query = _context.ResearchPapers
            .Include(p => p.Category)
            .Include(p => p.Student)
            .Where(p => p.Status == PaperStatus.Approved);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                p.Title.Contains(search) ||
                (p.Keywords != null && p.Keywords.Contains(search)) ||
                (p.Student != null && p.Student.FullName.Contains(search)));
        }

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (year.HasValue)
            query = query.Where(p => p.Year == year.Value);

        int totalCount = await query.CountAsync();
        int totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
        if (totalPages < 1) totalPages = 1;
        if (page < 1) page = 1;
        if (page > totalPages) page = totalPages;

        var papers = await query
            .OrderByDescending(p => p.DateUploaded)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        ViewBag.Categories = await _context.Categories
            .OrderBy(c => c.CategoryName)
            .ToListAsync();

        ViewBag.Years = await _context.ResearchPapers
            .Select(p => p.Year)
            .Distinct()
            .OrderByDescending(y => y)
            .ToListAsync();

        ViewBag.SearchTerm = search;
        ViewBag.SelectedCategory = categoryId;
        ViewBag.SelectedYear = year;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalCount = totalCount;

        return View(papers);
    }

    public async Task<IActionResult> Details(string id)
    {
        var sidebar = SidebarData.StudentMenu();
        ViewBag.Sidebar = sidebar;

        var paper = await _context.ResearchPapers
            .Include(p => p.Category)
            .Include(p => p.Student)
            .Include(p => p.ResearchMembers).ThenInclude(m => m.Student)
            .FirstOrDefaultAsync(p => p.PaperId == id);

        if (paper == null)
            return NotFound();

        return View(paper);
    }
}