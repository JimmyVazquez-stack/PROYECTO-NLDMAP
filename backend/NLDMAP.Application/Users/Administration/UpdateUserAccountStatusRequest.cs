using System.ComponentModel.DataAnnotations;

namespace NLDMAP.Application.Users.Administration;

public sealed class UpdateUserAccountStatusRequest
{
    [Required(ErrorMessage = "Indica el estado de la cuenta.")]
    public bool? IsActive { get; set; }

    [Required(ErrorMessage = "Escribe el motivo del cambio.")]
    [StringLength(1000, ErrorMessage = "El motivo no debe superar los 1000 caracteres.")]
    public string Reason { get; set; } = string.Empty;

    [Required(ErrorMessage = "Actualiza la ficha antes de cambiar el estado.")]
    [StringLength(128)]
    public string ExpectedVersion { get; set; } = string.Empty;
}
