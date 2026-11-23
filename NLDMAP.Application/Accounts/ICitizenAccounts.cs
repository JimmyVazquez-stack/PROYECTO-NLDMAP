using System.ComponentModel.DataAnnotations;
using NLDMAP.Application.Administration;

namespace NLDMAP.Application.Accounts;

public interface ICitizenAccounts
{
    Task RegisterAsync(CitizenRegistration input);
    Task<CitizenProfile?> GetProfileAsync(ActorSession actor);
    Task SendEmailConfirmationAsync(ActorSession actor);
    Task<CitizenProfile> CompleteProfileAsync(ActorSession actor, CitizenProfileInput input);
}

public sealed class CitizenRegistration
{
    [Required, StringLength(120)]
    public string DisplayName { get; set; } = "";

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = "";

    [Required, StringLength(1024, MinimumLength = 12)]
    public string Password { get; set; } = "";
}

public sealed class CitizenProfileInput
{
    [Required, StringLength(120)]
    public string FullName { get; set; } = "";

    [Required, Phone, StringLength(30)]
    public string PhoneNumber { get; set; } = "";

    [StringLength(4096)]
    public string? EmailCode { get; set; }
}

public sealed record CitizenProfile(
    string DisplayName, string? Email, bool EmailConfirmed, string Role, bool CanCreateReports);
