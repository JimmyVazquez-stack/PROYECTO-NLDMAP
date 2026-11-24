using System.Text.RegularExpressions;

namespace NLDMAP.Domain.Catalogs;

public sealed class TipoSena
{
    public const int NombreMaxLength = 80;
    
    public Guid Id { get; }
    public string Nombre { get; }

    public TipoSena(Guid id, string nombre)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador unico es obligatorio");
        
        nombre = (nombre ?? "").Trim();
        if (nombre.Length is 0 or > NombreMaxLength)
            throw new ArgumentException($"El nombre debe tener de 1 a {NombreMaxLength} caracteres ");

        Id = id;
        Nombre = nombre;
    }
}