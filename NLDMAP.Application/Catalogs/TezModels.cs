using System.ComponentModel.DataAnnotations;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

//Esta se ustiliza para cuando pidamos la lsita completa 
public sealed record TezQuery(
    string? Search = null, 
    int Page = 1
    );
//Esto se encarga de mostrar una fila y sus valores para esa fila en caso de listar
public sealed record TezRow(
    Guid Id,
    string ColorPiel,
    string? ColorHex,
    int Orden,
    string Version);
//Esta se utiliza para cuando nececitemos actulizar o agregar nuevas entradas
public sealed class TezInput
{
    public Guid? Id { get; set; }
    public string Version { get; set; } = "";

    [Required(ErrorMessage = "Escribe el nombre del tono."), StringLength(80)]
    public string ColorPiel { get; set; } = "";
    [Required(ErrorMessage = "Selecciona un color de la paleta"), StringLength(80)]
    public string ColorHex { get; set; } = "";

    [Range(0, 9999, ErrorMessage = "El orden debe estar entre 0 y 9999")]
    public int Orden { get; set; }
}