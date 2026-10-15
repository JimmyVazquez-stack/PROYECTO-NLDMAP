using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.Extensions.Configuration;

namespace NLDMAP.Web.Administration;

public sealed class AdminApiClient(
    HttpClient http,
    IConfiguration configuration)
{
    public Task<SessionSnapshot> GetSessionAsync(CancellationToken cancellationToken = default) =>
        GetJsonAsync<SessionSnapshot>("api/auth/me", cancellationToken);

    public Task<RoleDefinition[]> GetRolesAsync(CancellationToken cancellationToken = default) =>
        GetJsonAsync<RoleDefinition[]>("api/admin/roles", cancellationToken);

    public Task<AccountPage> GetUsersAsync(
        int pageNumber,
        int pageSize,
        string search,
        CancellationToken cancellationToken = default)
    {
        var path = "api/admin/users?pageNumber="
            + pageNumber.ToString(CultureInfo.InvariantCulture)
            + "&pageSize=" + pageSize.ToString(CultureInfo.InvariantCulture)
            + "&search=" + Uri.EscapeDataString(search);

        return GetJsonAsync<AccountPage>(path, cancellationToken);
    }

    public async Task ChangeRoleAsync(
        string userId,
        string roleName,
        bool remove,
        CancellationToken cancellationToken = default)
    {
        var path = "api/admin/users/" + Uri.EscapeDataString(userId) + "/roles";
        using var response = await SendAsync(
            remove ? HttpMethod.Delete : HttpMethod.Put,
            path, new { roleName }, useCsrf: true, cancellationToken);
    }

    public async Task<RoleDefinition> UpdatePermissionsAsync(
        RoleDefinition role,
        IEnumerable<string> permissions,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(
            HttpMethod.Put,
            "api/admin/roles/permissions",
            new
            {
                roleName = role.Name,
                expectedVersion = role.Version,
                permissions = permissions.OrderBy(value => value, StringComparer.Ordinal).ToArray()
            },
            useCsrf: true,
            cancellationToken);

        return await ReadJsonAsync<RoleDefinition>(response, cancellationToken);
    }

    public Task<AccountDetails> GetUserDetailsAsync(
        string userId,
        CancellationToken cancellationToken = default) =>
        GetJsonAsync<AccountDetails>(
            "api/admin/users/" + Uri.EscapeDataString(userId), cancellationToken);

    public Task<ProfileHistory> GetUserHistoryAsync(
        string userId,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        GetJsonAsync<ProfileHistory>(
            "api/admin/users/" + Uri.EscapeDataString(userId)
            + "/history?pageNumber=" + pageNumber.ToString(CultureInfo.InvariantCulture)
            + "&pageSize=" + pageSize.ToString(CultureInfo.InvariantCulture),
            cancellationToken);

    public async Task<AccountDetails> UpdateUserProfileAsync(
        AccountDetails account,
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(
            HttpMethod.Put,
            "api/admin/users/" + Uri.EscapeDataString(account.Id),
            new
            {
                firstName,
                lastName,
                expectedVersion = account.Version
            },
            useCsrf: true,
            cancellationToken);

        return await ReadJsonAsync<AccountDetails>(response, cancellationToken);
    }

    private async Task<T> GetJsonAsync<T>(
        string path,
        CancellationToken cancellationToken)
    {
        using var response = await SendAsync(
            HttpMethod.Get, path, null, useCsrf: false, cancellationToken);

        return await ReadJsonAsync<T>(response, cancellationToken);
    }

    public Task<AccountStatusHistory> GetAccountStatusHistoryAsync(
        string userId, int pageNumber = 1, int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        GetJsonAsync<AccountStatusHistory>(
            "api/admin/users/" + Uri.EscapeDataString(userId)
            + "/status/history?pageNumber=" + pageNumber.ToString(CultureInfo.InvariantCulture)
            + "&pageSize=" + pageSize.ToString(CultureInfo.InvariantCulture), cancellationToken);

    public async Task<AccountDetails> UpdateAccountStatusAsync(
        AccountDetails account, bool isActive, string reason,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Put,
            "api/admin/users/" + Uri.EscapeDataString(account.Id) + "/status",
            new { isActive, reason, expectedVersion = account.Version }, useCsrf: true, cancellationToken);
        return await ReadJsonAsync<AccountDetails>(response, cancellationToken);
    }

    private static async Task<T> ReadJsonAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            var value = await response.Content.ReadFromJsonAsync<T>(
                cancellationToken: cancellationToken);

            return value ?? throw new ApiException("La API devolvió una respuesta vacía.");
        }
        catch (JsonException)
        {
            throw new ApiException("La API devolvió una respuesta que no se pudo interpretar.");
        }
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string path,
        object? body,
        bool useCsrf,
        CancellationToken cancellationToken)
    {
        var value = configuration["ApiBaseUrl"];

        if (!Uri.TryCreate(value, UriKind.Absolute, out var baseUri)
            || (baseUri.Scheme != Uri.UriSchemeHttps && baseUri.Scheme != Uri.UriSchemeHttp))
        {
            throw new ApiException("Revisa la configuración ApiBaseUrl del frontend.");
        }

        // Treat ApiBaseUrl as a directory even if its final slash was omitted.
        baseUri = new Uri(baseUri.AbsoluteUri.TrimEnd('/') + "/");

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));

        try
        {
            using var request = new HttpRequestMessage(method, new Uri(baseUri, path));
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            if (body is not null)
            {
                request.Content = JsonContent.Create(body);
            }

            if (useCsrf)
            {
                // Fetch a token for the current session; do not retain it across account changes.
                var token = await GetJsonAsync<CsrfToken>("api/auth/csrf", timeout.Token);

                if (token.HeaderName != "X-CSRF-TOKEN" || string.IsNullOrWhiteSpace(token.RequestToken))
                {
                    throw new ApiException("No se pudo obtener la protección de la solicitud.");
                }

                request.Headers.Add(token.HeaderName, token.RequestToken);
            }

            var response = await http.SendAsync(
                request, HttpCompletionOption.ResponseContentRead, timeout.Token);

            if (!response.IsSuccessStatusCode)
            {
                using (response)
                {
                    throw await CreateExceptionAsync(response, timeout.Token);
                }
            }

            return response;
        }
        catch (HttpRequestException)
        {
            throw new ApiException("No se pudo conectar con la API. Revisa HTTPS y la conexión.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ApiException(
                "La solicitud tardó demasiado. Recarga los datos antes de repetir un cambio.");
        }
    }

    private static async Task<ApiException> CreateExceptionAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var message = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Tu sesión no está disponible. Inicia sesión de nuevo.",
            HttpStatusCode.Forbidden => "Tu cuenta no tiene permiso para esta operación.",
            HttpStatusCode.Conflict => "Los datos cambiaron. Recarga la información antes de continuar.",
            HttpStatusCode.BadRequest => "Revisa los datos de la solicitud.",
            _ => "No se pudo completar la operación."
        };

        if (response.StatusCode is HttpStatusCode.BadRequest
            or HttpStatusCode.Conflict or HttpStatusCode.NotFound)
        {
            try
            {
                using var document = await JsonDocument.ParseAsync(
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    cancellationToken: cancellationToken);
                var root = document.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("errors", out var errors)
                        && errors.ValueKind == JsonValueKind.Object)
                    {
                        var messages = errors.EnumerateObject()
                            .Where(property => property.Value.ValueKind == JsonValueKind.Array)
                            .SelectMany(property => property.Value.EnumerateArray())
                            .Where(item => item.ValueKind == JsonValueKind.String)
                            .Select(item => item.GetString())
                            .Where(item => !string.IsNullOrWhiteSpace(item))
                            .Distinct(StringComparer.Ordinal)
                            .ToArray();

                        if (messages.Length > 0)
                        {
                            message = string.Join(" ", messages);
                        }
                    }
                    else if (root.TryGetProperty("message", out var detail)
                        && detail.ValueKind == JsonValueKind.String)
                    {
                        message = detail.GetString() ?? message;
                    }
                    else if (root.TryGetProperty("title", out var title)
                        && title.ValueKind == JsonValueKind.String)
                    {
                        message = title.GetString() ?? message;
                    }
                }
            }
            catch (JsonException)
            {
                // Keep the status-specific message for non-JSON error responses.
            }
        }

        return new ApiException(message, response.StatusCode);
    }
}
