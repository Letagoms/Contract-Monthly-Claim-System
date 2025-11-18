using Microsoft.AspNetCore.Mvc;
using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Filters;
using Contract_Monthly_Claim_System.Services;
using Microsoft.EntityFrameworkCore;

namespace Contract_Monthly_Claim_System.Controllers
{
    [SessionAuthorization]
    public class EditLectureInfoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EditLectureInfoController> _logger;

        public EditLectureInfoController(ApplicationDbContext context, ILogger<EditLectureInfoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // Get all lecturers for the dropdown
            var lecturers = await _context.Users
                .Where(u => u.Role == "Lecturer")
                .OrderBy(u => u.Name)
                .ToListAsync();

            ViewBag.Lecturers = lecturers;

            return View("~/Views/EditLectureInfo/Index.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(int UserId, string FirstName, string LastName, string Email)
        {
            try
            {
                // Find the user
                var user = await _context.Users.FindAsync(UserId);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Index");
                }

                // Check if email already exists for another user
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == Email && u.Id != UserId);
                
                if (existingUser != null)
                {
                    TempData["ErrorMessage"] = "This email is already in use by another user.";
                    return RedirectToAction("Index");
                }

                // Update only lastname and email (firstname is readonly)
                user.Name = $"{FirstName} {LastName}";
                user.Email = Email;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Profile updated for user {UserId}");
                TempData["SuccessMessage"] = "Profile updated successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                TempData["ErrorMessage"] = "An error occurred while updating the profile.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(int UserId, string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            try
            {
                // Validate passwords match
                if (NewPassword != ConfirmPassword)
                {
                    TempData["ErrorMessage"] = "New passwords do not match.";
                    return RedirectToAction("Index");
                }

                // Validate password length
                if (NewPassword.Length < 6)
                {
                    TempData["ErrorMessage"] = "Password must be at least 6 characters long.";
                    return RedirectToAction("Index");
                }

                // Find the user
                var user = await _context.Users.FindAsync(UserId);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Index");
                }

                // Verify current password
                if (!PasswordHasher.VerifyPassword(CurrentPassword, user.PasswordHash))
                {
                    TempData["ErrorMessage"] = "Current password is incorrect.";
                    return RedirectToAction("Index");
                }

                // Update password
                user.PasswordHash = PasswordHasher.HashPassword(NewPassword);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Password changed for user {UserId}");
                TempData["SuccessMessage"] = "Password changed successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                TempData["ErrorMessage"] = "An error occurred while changing the password.";
            }

            return RedirectToAction("Index");
        }
    }
}