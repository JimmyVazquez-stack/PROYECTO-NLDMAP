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
    string Region);

public sealed class RegionCorporalInput
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Escribe el nombre de la region coporal.")]
    public string Region { get; set; } = "";

}