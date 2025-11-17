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

        public async Task<IActionResult> Index()
        {
            // Get all users with their role information
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedDate)
                .ToListAsync();

            // Get all claims with user details
            var claims = await _context.Claims
                .Include(c => c.User)
                .OrderByDescending(c => c.SubmittedDate)
                .ToListAsync();

            ViewBag.Users = users;
            ViewBag.Claims = claims;

            return View();
        }
    }
}