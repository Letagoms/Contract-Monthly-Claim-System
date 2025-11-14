using Microsoft.AspNetCore.Mvc;
using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Models;
using Contract_Monthly_Claim_System.Filters;

namespace Contract_Monthly_Claim_System.Controllers
{
    [SessionAuthorization]
    public class LectureClaimController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LectureClaimController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Constants for validation
        private const decimal MAX_HOURLY_RATE = 625m;
        private const decimal MAX_HOURS_WORKED = 160m;

        public LectureClaimController(ApplicationDbContext context, ILogger<LectureClaimController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var userName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = userName;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitClaim(string LecturerName, string CourseName, DateTime LectureDate, 
            decimal HoursWorked, decimal HourlyRate, string? Description, IFormFile? Document)
        {
            _logger.LogInformation("Claim submission started");

            try
            {
                // Validate hourly rate
                if (HourlyRate > MAX_HOURLY_RATE)
                {
                    ViewBag.Error = $"Hourly rate cannot exceed R{MAX_HOURLY_RATE}. Please enter a valid hourly rate.";
                    ViewBag.UserName = LecturerName;
                    return View("Index");
                }

                // Validate hours worked
                if (HoursWorked > MAX_HOURS_WORKED)
                {
                    ViewBag.Error = $"Hours worked cannot exceed {MAX_HOURS_WORKED} hours. Please enter a valid number of hours.";
                    ViewBag.UserName = LecturerName;
                    return View("Index");
                }

                // Validate positive values
                if (HourlyRate <= 0)
                {
                    ViewBag.Error = "Hourly rate must be greater than zero.";
                    ViewBag.UserName = LecturerName;
                    return View("Index");
                }

                if (HoursWorked <= 0)
                {
                    ViewBag.Error = "Hours worked must be greater than zero.";
                    ViewBag.UserName = LecturerName;
                    return View("Index");
                }

                // Get user ID from session
                var userIdString = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdString))
                {
                    return RedirectToAction("Index", "Login");
                }

                int userId = int.Parse(userIdString);

                // Handle file upload
                string? documentPath = null;
                if (Document != null && Document.Length > 0)
                {
                    // Validate file size (5MB max)
                    if (Document.Length > 5 * 1024 * 1024)
                    {
                        ViewBag.Error = "File size must not exceed 5MB.";
                        ViewBag.UserName = LecturerName;
                        return View("Index");
                    }

                    // Create uploads folder if it doesn't exist
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "claims");
                    Directory.CreateDirectory(uploadsFolder);

                    // Generate unique filename
                    var uniqueFileName = $"{Guid.NewGuid()}_{Document.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Document.CopyToAsync(fileStream);
                    }

                    documentPath = $"/uploads/claims/{uniqueFileName}";
                    _logger.LogInformation($"File uploaded: {documentPath}");
                }

                // Calculate total amount
                var totalAmount = HoursWorked * HourlyRate;

                // Create claim object
                var claim = new Claim
                {
                    UserId = userId,
                    LecturerName = LecturerName,
                    CourseName = CourseName,
                    LectureDate = LectureDate,
                    HoursWorked = HoursWorked,
                    HourlyRate = HourlyRate,
                    TotalAmount = totalAmount,
                    Description = Description,
                    DocumentPath = documentPath,
                    SubmittedDate = DateTime.Now,
                    CoordinatorStatus = "Pending",
                    ManagerStatus = "Pending"
                };

                // Save to database
                _context.Claims.Add(claim);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    _logger.LogInformation($"Claim submitted successfully. ID: {claim.Id}");
                    TempData["SuccessMessage"] = "Claim submitted successfully!";
                    return RedirectToAction("Index", "LectureView");
                }
                else
                {
                    ViewBag.Error = "Failed to submit claim. Please try again.";
                    ViewBag.UserName = LecturerName;
                    return View("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting claim");
                ViewBag.Error = $"Error: {ex.Message}";
                ViewBag.UserName = LecturerName;
                return View("Index");
            }
        }
    }
}
