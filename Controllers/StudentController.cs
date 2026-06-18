using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using monitoring_management.Services;

namespace monitoring_management.Controllers;

public class StudentController : Controller
{
    public IActionResult Index()
    {
        var sidebar = SidebarData.StudentMenu();

        var papers = new List<dynamic>
        {
            new {
                Title = "BeeGuard: A Crowdsourced Mobile Platform for Native Bee Conservation and Honey Traceability in Region I and CAR",
                Category = "Mobile Application",
                Year = 2026,
                UploadedBy = "K. Gamayo",
                Status = "APPROVED"
            },
            new {
                Title = "PolliTrace: Blockchain-Based Honey Supply Chain Verification",
                Category = "Web Application",
                Year = 2026,
                UploadedBy = "M. Reyes",
                Status = "APPROVED"
            },
            new {
                Title = "ApisCare: ML-Based Bee Disease Image Classifier for Smallholder Apiarists",
                Category = "Machine Learning",
                Year = 2025,
                UploadedBy = "R. Torres",
                Status = "APPROVED"
            },
        };

        ViewBag.Papers = papers;
        ViewBag.Sidebar = sidebar;

        return View();
    }

    public IActionResult MyWorks()
    {
        var sidebar = SidebarData.StudentMenu();

        var papers = new List<dynamic>
        {
            new {
                Title = "BeeGuard: A Crowdsourced Mobile Platform for Native Bee Conservation and Honey Traceability in Region I and CAR",
                Category = "Mobile Application",
                Role = "Leader",
                Status = "APPROVED"
            },
            new {
                Title = "PolliTrace: Blockchain-Based Honey Supply Chain Verification",
                Category = "Web Application",
                Role = "Member",
                Status = "APPROVED"
            },
        };

        ViewBag.Papers = papers;
        ViewBag.Sidebar = sidebar;

        return View();
    }
}
