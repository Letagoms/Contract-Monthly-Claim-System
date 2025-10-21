using Microsoft.AspNetCore.Mvc;
using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Models;
using Contract_Monthly_Claim_System.Services;
using Microsoft.EntityFrameworkCore;

namespace Contract_Monthly_Claim_System.Controllers
{
    public class RegisterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RegisterController> _logger;

        public RegisterController(ApplicationDbContext context, ILogger<RegisterController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string FirstName, string LastName, string Email, string Password, string ConfirmPassword, string role)
        {
            _logger.LogInformation("Registration attempt started");
            
            // Validate required fields
            if (string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName) || 
                string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(role))
            {
                ViewBag.Error = "Please fill in all required fields.";
                return View();
            }

            // Validate password match
            if (Password != ConfirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            // Validate password length
            if (Password.Length < 6)
            {
                ViewBag.Error = "Password must be at least 6 characters long.";
                return View();
            }

            try
            {
                // ✅ FIX #3: Check if email already exists (Email Uniqueness)
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);
                if (existingUser != null)
                {
                    _logger.LogWarning($"Registration failed: Email {Email} already exists");
                    ViewBag.Error = "This email is already registered. Please login or use a different email.";
                    return View();
                }

                // ✅ FIX #1: Hash password before storing
                var hashedPassword = PasswordHasher.HashPassword(Password);

                // Create user object
                var user = new User
                {
                    Name = $"{FirstName} {LastName}",
                    Email = Email,
                    PasswordHash = hashedPassword,
                    Role = role,
                    CreatedDate = DateTime.Now
                };

                _logger.LogInformation($"Created user object: {user.Name}, {user.Email}, {user.Role}");

                // Add user to context
                _context.Users.Add(user);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation("User successfully saved to database");
                    
                    // ✅ FIX #2: Set session after successful registration
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
                        default:
                            ViewBag.Error = "Invalid role selected.";
                            return View();
                    }
                }
                else
                {
                    ViewBag.Error = "Registration failed: No data was saved to database.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving user to database");
                ViewBag.Error = $"Registration failed: {ex.Message}";
                return View();
            }
        }
    }
}
