using Microsoft.AspNetCore.Mvc;
using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Filters;
using Microsoft.EntityFrameworkCore;

namespace Contract_Monthly_Claim_System.Controllers
{
    [SessionAuthorization]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, bool allTime = false)
        {
            // Query for approved claims only (both Coordinator and Manager approved)
            var query = _context.Claims
                .Include(c => c.User)
                .Where(c => c.CoordinatorStatus == "Approved" && c.ManagerStatus == "Approved");

            // Apply date filtering if not "all time"
            if (!allTime && startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(c => c.SubmittedDate >= startDate.Value && c.SubmittedDate <= endDate.Value);
            }

            var approvedClaims = await query
                .OrderByDescending(c => c.SubmittedDate)
                .ToListAsync();

            // Calculate totals
            var totalClaims = approvedClaims.Count;
            var totalAmount = approvedClaims.Sum(c => c.TotalAmount);

            ViewBag.TotalClaims = totalClaims;
            ViewBag.TotalAmount = totalAmount;
            ViewBag.ApprovedClaims = approvedClaims;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd") ?? DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.AllTime = allTime;

            return View();
        }
    }
}