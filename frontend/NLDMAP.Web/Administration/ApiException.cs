using System.Net;

namespace NLDMAP.Web.Administration;

public sealed class ApiException(
    string message,
    HttpStatusCode? statusCode = null) : Exception(message)
{
    public HttpStatusCode? StatusCode { get; } = statusCode;

    public bool IsAccessDenied =>
        StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden;
}
