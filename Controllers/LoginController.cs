using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using monitoring_management.Models;

namespace monitoring_management.Controllers;

public class LoginController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Registration()
    {
        return View();
    }
}
