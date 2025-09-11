using System.Net.Http.Json;

namespace TaskTracker.Web.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContext;

    public ApiClient(HttpClient http, IHttpContextAccessor httpContext)
    {
        _http = http;
        _httpContext = httpContext;
    }

    public record RegisterDto(string Email, string Password);
    public record LoginDto(string Email, string Password);
    public record TaskCreateDto(string Title);
    public record TaskItem(int Id, string Title, bool IsDone, DateTime CreatedOn);
    public class TokenResponse
    {
        public string token { get; set; } = "";
        public Guid userId { get; set; }
        public string email { get; set; } = "";
    }

    void SaveToken(string token) =>
        _httpContext.HttpContext!.Session.SetString("jwt", token);

    public async Task<(bool ok, string? error)> RegisterAsync(string email, string password)
    {
        var resp = await _http.PostAsJsonAsync("/api/auth/register", new RegisterDto(email, password));
        if (!resp.IsSuccessStatusCode)
            return (false, await resp.Content.ReadAsStringAsync());

        var body = await resp.Content.ReadFromJsonAsync<TokenResponse>();
        if (body is null || string.IsNullOrEmpty(body.token)) return (false, "No token returned");
        SaveToken(body.token);
        return (true, null);
    }

    public async Task<(bool ok, string? error)> LoginAsync(string email, string password)
    {
        var resp = await _http.PostAsJsonAsync("/api/auth/login", new LoginDto(email, password));
        if (!resp.IsSuccessStatusCode)
            return (false, "Invalid credentials");

        var body = await resp.Content.ReadFromJsonAsync<TokenResponse>();
        if (body is null || string.IsNullOrEmpty(body.token)) return (false, "No token returned");
        SaveToken(body.token);
        return (true, null);
    }

    public async Task<List<TaskItem>> GetTasksAsync() =>
        await _http.GetFromJsonAsync<List<TaskItem>>("/api/tasks") ?? new();

    public async Task<bool> CreateTaskAsync(string title)
    {
        var resp = await _http.PostAsJsonAsync("/api/tasks", new TaskCreateDto(title));
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> ToggleTaskAsync(int id)
    {
        var resp = await _http.PutAsync($"/api/tasks/{id}/toggle", content: null);
        return resp.IsSuccessStatusCode;
    }

    public void Logout() => _httpContext.HttpContext!.Session.Remove("jwt");
}
