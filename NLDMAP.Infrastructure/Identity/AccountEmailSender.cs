using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NLDMAP.Infrastructure.Identity;

public sealed class AccountEmailSender(
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger<AccountEmailSender> logger)
{
    public string Link(string path, params (string Key, string Value)[] query)
    {
        var configured = configuration["PublicAppUrl"];
        if (!Uri.TryCreate(configured, UriKind.Absolute, out var origin) ||
            (origin.Scheme != "https" && !(environment.IsDevelopment() && origin.IsLoopback && origin.Scheme == "http")))
            throw new InvalidOperationException("Configura PublicAppUrl con el origen HTTPS de la aplicación.");
        return new Uri(origin.AbsoluteUri.TrimEnd('/') + "/" + path.TrimStart('/')).AbsoluteUri + "?" +
            string.Join("&", query.Select(x => Uri.EscapeDataString(x.Key) + "=" + Uri.EscapeDataString(x.Value)));
    }

    public async Task SendAsync(string recipient, string subject, string text)
    {
        var mode = configuration["Email:Mode"];
        var pickup = environment.IsDevelopment() && string.Equals(mode, "Pickup", StringComparison.OrdinalIgnoreCase);
        if (!pickup && !string.Equals(mode, "Smtp", StringComparison.OrdinalIgnoreCase))
            throw ConfigurationError("Email:Mode");

        var sender = configuration["Email:From"];
        if (pickup && string.IsNullOrWhiteSpace(sender))
            sender = "noreply@nldmap.local";
        if (!MailAddress.TryCreate(sender, out var from))
            throw ConfigurationError("Email:From");

        using var message = new MailMessage
        {
            From = from,
            Subject = subject,
            Body = text
        };
        message.To.Add(recipient);
        using var client = new SmtpClient();
        if (pickup)
        {
            var directory = Path.Combine(environment.ContentRootPath, "App_Data", "mail");
            Directory.CreateDirectory(directory);
            client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
            client.PickupDirectoryLocation = directory;
            client.EnableSsl = false;
        }
        else
        {
            client.Host = RequiredSetting("Email:Host");
            var configuredPort = configuration["Email:Port"];
            var port = 587;
            if (!string.IsNullOrWhiteSpace(configuredPort) &&
                (!int.TryParse(configuredPort, out port) || port is < 1 or > 65535))
                throw ConfigurationError("Email:Port");

            client.Port = port;
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(RequiredSetting("Email:Username"), RequiredSetting("Email:Password"));
        }

        try
        {
            await client.SendMailAsync(message);
        }
        catch (SmtpException exception)
        {
            var cause = exception.GetBaseException();
            logger.LogWarning(
                "Falló el envío de correo. Estado SMTP: {StatusCode}. Causa: {Cause}. Error de red: {NetworkError}.",
                (int)exception.StatusCode, cause.GetType().Name, (cause as SocketException)?.SocketErrorCode);
            throw;
        }
    }

    private string RequiredSetting(string key)
    {
        var value = configuration[key];
        return string.IsNullOrWhiteSpace(value) ? throw ConfigurationError(key) : value;
    }

    private InvalidOperationException ConfigurationError(string key)
    {
        logger.LogWarning("Falta configurar el correo o el valor no es válido para {Setting}.", key);
        return new InvalidOperationException($"Revisa la configuración de {key}.");
    }
}
