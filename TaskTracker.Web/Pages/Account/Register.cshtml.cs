using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskTracker.Web.Services;

namespace TaskTracker.Web.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly ApiClient _api;
    public RegisterModel(ApiClient api) => _api = api;

    [BindProperty, EmailAddress, Required]
    public string Email { get; set; } = "";

    [BindProperty, Required, MinLength(6)]
    public string Password { get; set; } = "";

    [TempData]
    public string? Error { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var (ok, err) = await _api.RegisterAsync(Email, Password);
        if (!ok)
        {
            Error = err ?? "Registration failed.";
            return Page();
        }

        return RedirectToPage("/Tasks/Index");
    }
}
