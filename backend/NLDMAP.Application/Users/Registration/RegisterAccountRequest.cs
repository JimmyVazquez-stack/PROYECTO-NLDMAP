using System.ComponentModel.DataAnnotations;

namespace NLDMAP.Application.Users.Registration;

/// <summary>
/// Input accepted by the public account registration endpoint for HU-01.
/// Identity manages the persisted account and its password.
/// </summary>
public sealed class RegisterAccountRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo electrónico tiene un formato inválido.")]
    [StringLength(256, ErrorMessage = "El correo electrónico no puede superar los 256 caracteres.")]
    public string Email { get; set; } = string.Empty;

    // UserManager.CreateAsync applies the password policy configured in Program.cs.
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Password { get; set; } = string.Empty;
}
