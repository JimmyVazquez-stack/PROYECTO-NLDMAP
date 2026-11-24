namespace NLDMAP.Domain.Catalogs;

public sealed class TipoCalzado
{
    public Guid Id { get; }
    public string NombreCalzado { get; }

    public TipoCalzado(Guid id, string nombreCalzado)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador es obligatorio.", nameof(id));

        Id = id;
        NombreCalzado = CatalogoValores.TextoObligatorio(nombreCalzado, nameof(nombreCalzado));
    }
}
