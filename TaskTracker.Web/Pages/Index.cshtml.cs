using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TaskTracker.Web.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        var hasToken = !string.IsNullOrEmpty(HttpContext.Session.GetString("jwt"));
        return hasToken
            ? RedirectToPage("/Tasks/Index")
            : RedirectToPage("/Account/Login");
    }
}
