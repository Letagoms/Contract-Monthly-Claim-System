using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Contract_Monthly_Claim_System.Filters
{
    public class SessionAuthorizationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetString("UserId");
            
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new RedirectToActionResult("Index", "Login", null);
            }
            
            base.OnActionExecuting(context);
        }
    }
}