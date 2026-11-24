namespace NLDMAP.Domain.Catalogs;

public sealed class Ocupacion
{
    public Guid Id { get; }
    public string NombreOcupacion { get; }
    public string? DescripcionOcupacion { get; }

    public Ocupacion(Guid id, string nombreOcupacion, string? descripcionOcupacion)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador es obligatorio.", nameof(id));

        Id = id;
        NombreOcupacion = CatalogoValores.TextoObligatorio(nombreOcupacion, nameof(nombreOcupacion));
        DescripcionOcupacion = string.IsNullOrWhiteSpace(descripcionOcupacion) ? null : descripcionOcupacion.Trim();
    }
}
