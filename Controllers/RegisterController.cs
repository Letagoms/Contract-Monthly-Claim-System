using Microsoft.AspNetCore.Mvc;
using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Models;
using System.Diagnostics;

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
            
            // Debug: Log received data
            _logger.LogInformation($"Received data - FirstName: {FirstName}, LastName: {LastName}, Email: {Email}, Role: {role}");
            
            // Validate that role is selected
            if (string.IsNullOrEmpty(role))
            {
                _logger.LogWarning("Registration failed: No role selected");
                ViewBag.Error = "Please select a role.";
                return View();
            }

            // Validate required fields
            if (string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName) || string.IsNullOrEmpty(Email))
            {
                _logger.LogWarning("Registration failed: Missing required fields");
                ViewBag.Error = "Please fill in all required fields.";
                return View();
            }

            // Test database connection first
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                _logger.LogInformation($"Database connection test: {canConnect}");
                
                if (!canConnect)
                {
                    ViewBag.Error = "Database connection failed. Please check your connection settings.";
                    return View();
                }
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "Database connection test failed");
                ViewBag.Error = $"Database connection error: {dbEx.Message}";
                return View();
            }

            // Create user object
            var user = new User
            {
                Name = $"{FirstName} {LastName}",
                Email = Email,
                Role = role,
                CreatedDate = DateTime.Now
            };

            _logger.LogInformation($"Created user object: {user.Name}, {user.Email}, {user.Role}");

            try
            {
                // Add user to context
                _context.Users.Add(user);
                _logger.LogInformation("User added to context");

                // Save changes
                var result = await _context.SaveChangesAsync();
                _logger.LogInformation($"SaveChanges result: {result} records affected");

                if (result > 0)
                {
                    _logger.LogInformation("User successfully saved to database");
                    ViewBag.Success = "Registration successful!";
                }
                else
                {
                    _logger.LogWarning("SaveChanges returned 0 - no records were saved");
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

            // Redirect based on selected role
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
