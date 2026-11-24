namespace NLDMAP.Domain.Catalogs;

public sealed class Genero
{
    public Guid Id { get; }
    public string NombreSexo { get; }

    public Genero(Guid id, string nombreSexo)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador es obligatorio.", nameof(id));

        Id = id;
        NombreSexo = CatalogoValores.TextoObligatorio(nombreSexo, nameof(nombreSexo));
    }
}
