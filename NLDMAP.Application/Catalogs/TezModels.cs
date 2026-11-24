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
    string NombreColor,
    string? ColorHex);
//Esta se utiliza para cuando nececitemos actulizar o agregar nuevas entradas
public sealed class TezInput
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Selecciona un tono Monk."), StringLength(80)]
    public string NombreColor { get; set; } = "";

}