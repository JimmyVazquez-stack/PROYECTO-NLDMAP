namespace NLDMAP.Domain.Catalogs;

public sealed class Complexion
{
    public Guid Id { get; }
    public string NombreComplexion { get; }

    public Complexion(Guid id, string nombreComplexion)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador es obligatorio.", nameof(id));

        Id = id;
        NombreComplexion = CatalogoValores.TextoObligatorio(nombreComplexion, nameof(nombreComplexion));
    }
}
