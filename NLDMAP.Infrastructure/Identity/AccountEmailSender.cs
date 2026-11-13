using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace NLDMAP.Infrastructure.Identity;

public sealed class AccountEmailSender(IConfiguration configuration, IHostEnvironment environment)
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
        using var message = new MailMessage(configuration["Email:From"] ?? "noreply@nldmap.local", recipient, subject, text);
        using var client = new SmtpClient();
        var mode = configuration["Email:Mode"];
        if (environment.IsDevelopment() && string.Equals(mode, "Pickup", StringComparison.OrdinalIgnoreCase))
        {
            var directory = Path.Combine(environment.ContentRootPath, "App_Data", "mail");
            Directory.CreateDirectory(directory);
            client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
            client.PickupDirectoryLocation = directory;
            client.EnableSsl = false;
        }
        else
        {
            if (!string.Equals(mode, "Smtp", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(configuration["Email:Host"]))
                throw new InvalidOperationException("Configura Email:Mode=Smtp y el servidor de correo.");
            client.Host = configuration["Email:Host"]!;
            client.Port = configuration.GetValue("Email:Port", 587);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(configuration["Email:Username"], configuration["Email:Password"]);
        }
        await client.SendMailAsync(message);
    }
}
