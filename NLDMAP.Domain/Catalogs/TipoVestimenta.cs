namespace NLDMAP.Domain.Catalogs;

public sealed class TipoVestimenta
{
    public const int NombreMaxLength = 80;

    public Guid Id { get; }
    public string NombreVestimenta { get; }

    public TipoVestimenta(Guid id, string nombreVestimenta)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador es obligatorio.", nameof(id));
        if (nombreVestimenta?.Trim().Length > NombreMaxLength)
            throw new ArgumentException($"El nombre admite hasta {NombreMaxLength} caracteres.", nameof(nombreVestimenta));

        Id = id;
        NombreVestimenta = CatalogoValores.TextoObligatorio(nombreVestimenta, nameof(nombreVestimenta));
    }
}
