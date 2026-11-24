namespace NLDMAP.Domain.Catalogs;

public sealed class Circunstancia
{
    public Guid Id { get; }
    public string Tipo { get; }
    public string? DescripcionCircunstancia { get; }

    public Circunstancia(Guid id, string tipo, string? descripcionCircunstancia)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador es obligatorio.", nameof(id));

        Id = id;
        Tipo = CatalogoValores.TextoObligatorio(tipo, nameof(tipo));
        DescripcionCircunstancia = string.IsNullOrWhiteSpace(descripcionCircunstancia) ? null : descripcionCircunstancia.Trim();
    }
}
