using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using monitoring_management.Services;

namespace monitoring_management.Controllers;

public class AdminController : Controller
{
    public IActionResult Index()
    {
        var sidebar = SidebarData.AdminMenu();
        ViewBag.Sidebar = sidebar;
        
        return View();
    }
}
