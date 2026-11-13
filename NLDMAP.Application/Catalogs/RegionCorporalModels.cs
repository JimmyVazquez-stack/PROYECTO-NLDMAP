using System.ComponentModel.DataAnnotations;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

//Aqui creamos 3 clases una para el query otra para la informacion
//a mostrar el "row" o fila y la entrada 

public sealed record RegionCorporalQuery(
    string? Search = null,
    int Page = 1
    );

public sealed record RegionCorporalRow(
    Guid Id,
    string Region,
    int Orden,
    string Version
);

public sealed class RegionCorporalInput
{
    public Guid? Id { get; set; }
    public string Version { get; set; } = "";

    [Required(ErrorMessage = "Escribe el nombre de la region coporal.")]
    public string Region { get; set; }

    [Required(ErrorMessage = "El orden debe estar entre 0 y 9999")]
    public int Orden { get; set; }
}