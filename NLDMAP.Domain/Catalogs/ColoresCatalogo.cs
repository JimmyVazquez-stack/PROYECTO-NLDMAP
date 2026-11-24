namespace NLDMAP.Domain.Catalogs;

public sealed class ColoresCatalogo
{
    public Guid Id { get; }
    public string ColorNombre { get; }
    public string ColorHex { get; }

    public ColoresCatalogo(Guid id, string colorNombre, string colorHex)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador es obligatorio.", nameof(id));

        Id = id;
        ColorNombre = CatalogoValores.TextoObligatorio(colorNombre, nameof(colorNombre));
        ColorHex = CatalogoValores.ColorHex(colorHex);
    }
}
