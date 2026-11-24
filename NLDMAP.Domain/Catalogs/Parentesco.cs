namespace NLDMAP.Domain.Catalogs;

public sealed class Parentesco
{
    public Guid Id { get; }
    public string NombreParentesco { get; }

    public Parentesco(Guid id, string nombreParentesco)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador es obligatorio.", nameof(id));

        Id = id;
        NombreParentesco = CatalogoValores.TextoObligatorio(nombreParentesco, nameof(nombreParentesco));
    }
}
