using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TaskTracker.Web.Services;

namespace TaskTracker.Web.Pages.Tasks;

public class IndexModel : PageModel
{
    private readonly ApiClient _api;
    public IndexModel(ApiClient api) => _api = api;

    public List<ApiClient.TaskItem> Tasks { get; private set; } = new();

    [BindProperty, Required, MinLength(1)]
    public string Title { get; set; } = "";

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("jwt")))
            return RedirectToPage("/Account/Login");

        Tasks = await _api.GetTasksAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("jwt")))
            return RedirectToPage("/Account/Login");

        if (!ModelState.IsValid)
        {
            Tasks = await _api.GetTasksAsync();
            return Page();
        }

        await _api.CreateTaskAsync(Title);
        return RedirectToPage(); // refresh list
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("jwt")))
            return RedirectToPage("/Account/Login");

        await _api.ToggleTaskAsync(id);
        return RedirectToPage();
    }

    public IActionResult OnPostLogout()
    {
        _api.Logout();
        return RedirectToPage("/Account/Login");
    }
}
