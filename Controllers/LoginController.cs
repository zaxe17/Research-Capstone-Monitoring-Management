using Microsoft.AspNetCore.Mvc;
using monitoring_management.Models;

namespace monitoring_management.Controllers;

public class LoginController : Controller
{
    private readonly ApplicationDbContext _context;

    public LoginController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var admin = _context.Admins.FirstOrDefault(a => a.Username == model.Email);
        if (admin != null && BCrypt.Net.BCrypt.Verify(model.Password, admin.Password))
        {
            HttpContext.Session.SetString("AdminId", admin.AdminId.ToString());
            HttpContext.Session.SetString("Role", "Admin");
            HttpContext.Session.SetString("FullName", admin.Username);
            return RedirectToAction("Index", "Admin");
        }

        var student = _context.Students.FirstOrDefault(s => s.Email == model.Email);
        if (student != null && BCrypt.Net.BCrypt.Verify(model.Password, student.Password))
        {
            HttpContext.Session.SetString("StudentId", student.StudentId);
            HttpContext.Session.SetString("Role", "Student");
            HttpContext.Session.SetString("FullName", student.FullName);
            HttpContext.Session.SetString("StudentNo", student.StudentNo);
            HttpContext.Session.SetString("Program", student.Program);
            return RedirectToAction("Index", "Student");
        }

        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View(model);
    }
}