namespace NLDMAP.Domain.Catalogs;

public sealed class Nacionalidad
{
    public Guid Id { get; }
    public string NombreNacionalidad { get; }

    public Nacionalidad(Guid id, string nombreNacionalidad)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador es obligatorio.", nameof(id));

        Id = id;
        NombreNacionalidad = CatalogoValores.TextoObligatorio(nombreNacionalidad, nameof(nombreNacionalidad));
    }
}
