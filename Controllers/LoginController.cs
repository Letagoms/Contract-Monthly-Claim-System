using Microsoft.AspNetCore.Mvc;
using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Models;
using Contract_Monthly_Claim_System.Services;
using Microsoft.EntityFrameworkCore;

namespace Contract_Monthly_Claim_System.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoginController> _logger;

        public LoginController(ApplicationDbContext context, ILogger<LoginController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string email, string password, string role)
        {
            _logger.LogInformation("Login attempt started");
            
            // Validate required fields
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role))
            {
                ViewBag.Error = "Please fill in all fields.";
                return View();
            }

            try
            {
                // Query database for user with matching email and role
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email && u.Role == role);

                if (user == null)
                {
                    _logger.LogWarning($"Login failed: No user found with email {email} and role {role}");
                    ViewBag.Error = "Invalid email, password, or role.";
                    return View();
                }

                // ✅ FIX #1: Verify password hash
                if (!PasswordHasher.VerifyPassword(password, user.PasswordHash))
                {
                    _logger.LogWarning($"Login failed: Invalid password for {email}");
                    ViewBag.Error = "Invalid email, password, or role.";
                    return View();
                }

                _logger.LogInformation($"Login successful for user: {user.Email} with role: {user.Role}");

                // ✅ FIX #2: Set session after successful login
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserRole", user.Role);
                HttpContext.Session.SetString("UserName", user.Name);
                
                // Redirect based on role
                switch (role)
                {
                    case "Lecturer":
                        return RedirectToAction("Index", "LectureClaim");
                    case "Coordinator":
                        return RedirectToAction("Index", "Coordinator");
                    case "Manager":
                        return RedirectToAction("Index", "Manager");
                    case "Admin":
                        return RedirectToAction("Index", "Admin"); // Add Admin controller redirect
                    default:
                        ViewBag.Error = "Invalid role selected.";
                        return View();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login process");
                ViewBag.Error = $"Login failed: {ex.Message}";
                return View();
            }
        }
    }
}
