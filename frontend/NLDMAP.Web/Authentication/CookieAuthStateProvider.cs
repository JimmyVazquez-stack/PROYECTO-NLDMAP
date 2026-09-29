using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NLDMAP.Web.Authentication;

public class CookieAuthStateProvider(
    HttpClient http,
    IConfiguration configuration,
    ILogger<CookieAuthStateProvider> logger)
    : AuthenticationStateProvider
{
    private static AuthenticationState Anonymous =>
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState>
        GetAuthenticationStateAsync()
    {
        var baseUrl = configuration["ApiBaseUrl"]
            ?? throw new InvalidOperationException(
                "Falta configurar ApiBaseUrl.");

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            new Uri(new Uri(baseUrl), "api/auth/me"));

        request.SetBrowserRequestCredentials(
            BrowserRequestCredentials.Include);

        try
        {
            using var response = await http.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return Anonymous;
            }

            response.EnsureSuccessStatusCode();

            var user = await response.Content
                .ReadFromJsonAsync<UserSession>();

            if (user is null)
            {
                return Anonymous;
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.Email),
                new(ClaimTypes.Email, user.Email)
            };

            claims.AddRange(user.Roles.Select(
                role => new Claim(ClaimTypes.Role, role)));

            var identity = new ClaimsIdentity(
                claims, "IdentityCookie");

            return new AuthenticationState(
                new ClaimsPrincipal(identity));
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(
                exception, "No se pudo consultar la sesión.");

            return Anonymous;
        }
    }

    public async Task RefreshAsync()
    {
        var state = await GetAuthenticationStateAsync();

        NotifyAuthenticationStateChanged(
            Task.FromResult(state));
    }

    public class UserSession
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string[] Roles { get; set; } = [];
    }
}