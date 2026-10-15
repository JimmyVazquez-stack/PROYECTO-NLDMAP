using System.ComponentModel.DataAnnotations;

namespace NLDMAP.Application.Users.Administration;

public sealed class UpdateRolePermissionsRequest
{
    [Required(ErrorMessage = "Selecciona un rol.")]
    [StringLength(256, ErrorMessage = "El nombre del rol no debe superar los 256 caracteres.")]
    public string RoleName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vuelve a cargar la versión del rol.")]
    [StringLength(128, ErrorMessage = "La versión del rol no es válida.")]
    public string ExpectedVersion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Indica los permisos del rol, aunque sea una lista vacía.")]
    [MaxLength(24, ErrorMessage = "La lista no debe superar los 24 permisos.")]
    public string[]? Permissions { get; set; }
}

