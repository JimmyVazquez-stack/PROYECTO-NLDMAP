using System.ComponentModel.DataAnnotations;

namespace NLDMAP.Application.Users.Administration;

public sealed class ChangeUserRoleRequest
{
    [Required(ErrorMessage = "Selecciona un rol.")]
    [StringLength(256, ErrorMessage = "El nombre del rol no debe superar los 256 caracteres.")]
    public string RoleName { get; set; } = string.Empty;
}
