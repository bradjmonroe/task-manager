using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskTracker.Web.Services;

namespace TaskTracker.Web.Pages.Account;

public class LoginModel : PageModel
{
    private readonly ApiClient _api;
    public LoginModel(ApiClient api) => _api = api;

    [BindProperty, EmailAddress, Required]
    public string Email { get; set; } = "";

    [BindProperty, Required]
    public string Password { get; set; } = "";

    [TempData]
    public string? Error { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var (ok, err) = await _api.LoginAsync(Email, Password);
        if (!ok)
        {
            Error = err ?? "Login failed.";
            return Page();
        }

        return RedirectToPage("/Tasks/Index");
    }
}
