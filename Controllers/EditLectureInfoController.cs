using Microsoft.AspNetCore.Mvc;
using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Filters;
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

            // Explicitly specify the view path
            return View("~/Views/EditLectureInfo/Index.cshtml");
        }
    }
}