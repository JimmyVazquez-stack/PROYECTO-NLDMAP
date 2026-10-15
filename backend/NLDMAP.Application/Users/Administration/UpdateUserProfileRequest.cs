using System.ComponentModel.DataAnnotations;

namespace NLDMAP.Application.Users.Administration;

public sealed class UpdateUserProfileRequest
{
    [Required(ErrorMessage = "Escribe el nombre.")]
    [StringLength(200, ErrorMessage = "El nombre no debe superar los 200 caracteres.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe los apellidos.")]
    [StringLength(200, ErrorMessage = "Los apellidos no deben superar los 200 caracteres.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vuelve a cargar la versión de la cuenta.")]
    [StringLength(128, ErrorMessage = "La versión de la cuenta no es válida.")]
    public string ExpectedVersion { get; set; } = string.Empty;
}
