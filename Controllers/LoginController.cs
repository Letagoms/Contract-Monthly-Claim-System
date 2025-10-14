using Microsoft.AspNetCore.Mvc;

namespace Contract_Monthly_Claim_System.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string role, string FirstName, string LastName, string Email, string Password, string ConfirmPassword)
        {
            // Basic validation
            if (string.IsNullOrEmpty(role))
            {
                ViewBag.Error = "Please select your role.";
                return View();
            }

            // Add your registration logic here (save to database, etc.)
            // For now, we'll just redirect based on role

            if (role == "Lecturer")
                return RedirectToAction("Index", "LectureClaim");
            if (role == "Coordinator")
                return RedirectToAction("Index", "Coordinator");
            if (role == "Manager")
                return RedirectToAction("Index", "Manager");

            // Fallback
            ViewBag.Error = "Invalid role selected.";
            return View();
        }
    }
}
