using Microsoft.AspNetCore.Mvc;
using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Contract_Monthly_Claim_System.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string email, string role)
        {
            // Simple validation to ensure role is selected
            if (string.IsNullOrEmpty(role))
            {
                ViewBag.Error = "Please select a role.";
                return View();
            }

            // Redirect based on selected role without credential validation
            switch (role)
            {
                case "Lecturer":
                    return RedirectToAction("Index", "LectureClaim");
                case "Coordinator":
                    return RedirectToAction("Index", "Coordinator");
                case "Manager":
                    return RedirectToAction("Index", "Manager");
                default:
                    ViewBag.Error = "Invalid role selected.";
                    return View();
            }
        }
    }
}
