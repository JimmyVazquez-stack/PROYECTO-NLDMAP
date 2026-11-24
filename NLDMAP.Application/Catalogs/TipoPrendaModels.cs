using System.ComponentModel.DataAnnotations;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public sealed record TipoPrendaQuery(
    string? Search = null,
    int Page = 1
    );

public sealed record TipoPrendaRow(
    Guid Id,
    string Nombre);

public sealed class TipoPrendaInput
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Escribe el nombre del tipo prenda")]
    public string Nombre { get; set; } = "";

}
