namespace NLDMAP.Domain.Catalogs;

public sealed class Discapacidad
{
    public Guid Id { get; }
    public string NombreDiscapacidad { get; }

    public Discapacidad(Guid id, string nombreDiscapacidad)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador es obligatorio.", nameof(id));

        Id = id;
        NombreDiscapacidad = CatalogoValores.TextoObligatorio(nombreDiscapacidad, nameof(nombreDiscapacidad));
    }
}
