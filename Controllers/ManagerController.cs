using Microsoft.AspNetCore.Mvc;
using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Filters;
using Microsoft.EntityFrameworkCore;

namespace Contract_Monthly_Claim_System.Controllers
{
    [SessionAuthorization]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ManagerController> _logger;

        public ManagerController(ApplicationDbContext context, ILogger<ManagerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // Get ALL claims with user details - no filtering
            var claims = await _context.Claims
                .Include(c => c.User)
                .OrderByDescending(c => c.SubmittedDate)
                .ToListAsync();

            return View(claims);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveClaim(int claimId)
        {
            try
            {
                var claim = await _context.Claims.FindAsync(claimId);
                if (claim == null)
                {
                    TempData["ErrorMessage"] = "Claim not found.";
                    return RedirectToAction("Index");
                }

                claim.ManagerStatus = "Approved";
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Manager approved claim ID: {claimId}");
                TempData["SuccessMessage"] = "Claim approved successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error approving claim ID: {claimId}");
                TempData["ErrorMessage"] = "Failed to approve claim.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RejectClaim(int claimId)
        {
            try
            {
                var claim = await _context.Claims.FindAsync(claimId);
                if (claim == null)
                {
                    TempData["ErrorMessage"] = "Claim not found.";
                    return RedirectToAction("Index");
                }

                claim.ManagerStatus = "Rejected";
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Manager rejected claim ID: {claimId}");
                TempData["SuccessMessage"] = "Claim rejected successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error rejecting claim ID: {claimId}");
                TempData["ErrorMessage"] = "Failed to reject claim.";
            }

            return RedirectToAction("Index");
        }
    }
}
