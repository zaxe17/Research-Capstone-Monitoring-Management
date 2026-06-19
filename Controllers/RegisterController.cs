using Microsoft.AspNetCore.Mvc;
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
    public IActionResult Index()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        bool emailExists = await _context.Students.AnyAsync(s => s.Email == model.Email);
        if (emailExists)
        {
            ModelState.AddModelError("Email", "An account with this email already exists.");
            return View(model);
        }

        bool studentNoExists = await _context.Students.AnyAsync(s => s.StudentNo == model.StudentNo);
        if (studentNoExists)
        {
            ModelState.AddModelError("StudentNo", "This student number is already registered.");
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
}