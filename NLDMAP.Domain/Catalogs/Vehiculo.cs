namespace NLDMAP.Domain.Catalogs;

public sealed class Vehiculo
{
    public Guid Id { get; }
    public string Marca { get; }

    public Vehiculo(Guid id, string marca)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador es obligatorio.", nameof(id));

        Id = id;
        Marca = CatalogoValores.TextoObligatorio(marca, nameof(marca));
    }
}
