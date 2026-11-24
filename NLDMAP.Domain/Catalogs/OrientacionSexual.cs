namespace NLDMAP.Domain.Catalogs;

public sealed class OrientacionSexual
{
    public Guid Id { get; }
    public string NombreOrientacionSexual { get; }
    public string? DescripcionOrientacionSexual { get; }

    public OrientacionSexual(Guid id, string nombreOrientacionSexual, string? descripcionOrientacionSexual)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador es obligatorio.", nameof(id));

        Id = id;
        NombreOrientacionSexual = CatalogoValores.TextoObligatorio(nombreOrientacionSexual, nameof(nombreOrientacionSexual));
        DescripcionOrientacionSexual = string.IsNullOrWhiteSpace(descripcionOrientacionSexual) ? null : descripcionOrientacionSexual.Trim();
    }
}
