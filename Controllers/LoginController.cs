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
        public async Task<IActionResult> Index(string email, string role)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Role == role);

            if (user != null)
            {
                // Store user info in session or authentication
                if (role == "Lecturer")
                    return RedirectToAction("Index", "LectureClaim");
                if (role == "Coordinator")
                    return RedirectToAction("Index", "Coordinator");
                if (role == "Manager")
                    return RedirectToAction("Index", "Manager");
            }

            // Fallback
            ViewBag.Error = "Invalid credentials or role selected.";
            return View();
        }
    }
}
