using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace TaskTracker.Web.Services;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _http;

    public AuthHeaderHandler(IHttpContextAccessor http) => _http = http;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _http.HttpContext?.Session.GetString("jwt");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return base.SendAsync(request, cancellationToken);
    }
}
