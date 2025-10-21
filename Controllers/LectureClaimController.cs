using Microsoft.AspNetCore.Mvc;
using Contract_Monthly_Claim_System.Filters;

namespace Contract_Monthly_Claim_System.Controllers
{
    [SessionAuthorization] // ✅ FIX #2: Protect this controller
    public class LectureClaimController : Controller
    {
        public IActionResult Index()
        {
            var userName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = userName;
            return View();
        }
    }
}
