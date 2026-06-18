using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using monitoring_management.Models;

namespace monitoring_management.Controllers;

public class RegisterController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
