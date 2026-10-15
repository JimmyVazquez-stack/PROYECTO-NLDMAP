using System.ComponentModel.DataAnnotations;

namespace NLDMAP.Application.Users.Administration;

public sealed class UserHistoryRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "El número de página debe ser mayor que cero.")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "El tamaño de página debe estar entre 1 y 100.")]
    public int PageSize { get; set; } = 20;
}
