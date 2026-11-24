using System.ComponentModel.DataAnnotations;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public sealed record TipoSenaQuery(
    string? Search = null, 
    int Page = 1
    );

public sealed record TipoSenaRow(
    Guid Id,
    string Nombre);

public sealed class TipoSenaInput
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Escribe el nombre del tipo de seña.")]
    [StringLength(TipoSena.NombreMaxLength)]
    public string Nombre { get; set; } = "";

}