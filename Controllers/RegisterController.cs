using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using monitoring_management.Models;

namespace monitoring_management.Controllers;

public class RegisterController : Controller
{
    private readonly ApplicationDbContext _context;

    public RegisterController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new RegisterViewModel
        {
            ProgramList = await GetProgramListAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.ProgramList = await GetProgramListAsync();
            return View(model);
        }

        bool emailExists = await _context.Students.AnyAsync(s => s.Email == model.Email);
        if (emailExists)
        {
            ModelState.AddModelError("Email", "An account with this email already exists.");
            model.ProgramList = await GetProgramListAsync();
            return View(model);
        }

        bool studentNoExists = await _context.Students.AnyAsync(s => s.StudentNo == model.StudentNo);
        if (studentNoExists)
        {
            ModelState.AddModelError("StudentNo", "This student number is already registered.");
            model.ProgramList = await GetProgramListAsync();
            return View(model);
        }

        var fullName = string.Join(" ", new[] { model.FirstName, model.MiddleName, model.LastName }
            .Where(n => !string.IsNullOrWhiteSpace(n)));

        var student = new Student
        {
            // Placeholder only — the MySQL trigger (trg_student_id) overwrites this
            // unconditionally on INSERT. EF just needs a non-null key to track the entity.
            StudentId = Guid.NewGuid().ToString("N").Substring(0, 11).ToUpper(),
            StudentNo = model.StudentNo,
            Program = model.Program,
            FullName = fullName,
            Email = model.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(model.Password)
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Account created successfully. Please sign in.";
        return RedirectToAction("Index", "Login");
    }

    private async Task<List<SelectListItem>> GetProgramListAsync()
    {
        var programs = await _context.AcademicPrograms
            .OrderBy(p => p.SortOrder)
            .ToListAsync();

        var groups = new Dictionary<string, SelectListGroup>();
        var list = new List<SelectListItem>();

        foreach (var p in programs)
        {
            if (!groups.TryGetValue(p.CollegeName, out var group))
            {
                group = new SelectListGroup { Name = p.CollegeName };
                groups[p.CollegeName] = group;
            }

            list.Add(new SelectListItem
            {
                Value = p.Code,
                Text = p.Code,
                Group = group
            });
        }

        return list;
    }
}