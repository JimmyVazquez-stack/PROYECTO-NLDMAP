using System.Text.RegularExpressions;

namespace NLDMAP.Domain.Catalogs;

public sealed class TipoSena
{
    public const int NombreMaxLength = 80;
    
    public Guid Id { get; }
    public string Nombre { get; }
    public int Orden { get; }


    public TipoSena(Guid id, string nombre, int orden)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador unico es obligatorio");
        
        nombre = (nombre ?? "").Trim();
        if (nombre.Length is 0 or > NombreMaxLength)
            throw new ArgumentException($"El nombre debe tener de 1 a {NombreMaxLength} caracteres ");
        
        if (orden is < 0 or > 9999 )
            throw new ArgumentException($"El orden debe de estar entre 1 y 9999");

        Id = id;
        Nombre = nombre;
        Orden = orden;
    }
}