using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using monitoring_management.Services;

namespace monitoring_management.Controllers;

public class RepositoryController : Controller
{
    public IActionResult Index()
    {
        var sidebar = SidebarData.StudentMenu();
        ViewBag.Sidebar = sidebar;

        return View();
    }
}
