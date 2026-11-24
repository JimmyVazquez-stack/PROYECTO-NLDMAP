namespace NLDMAP.Domain.Catalogs;

public sealed class TipoCabello
{
    public Guid Id { get; }
    public string NombreCabello { get; }
    public string LargoCabello { get; }

    public TipoCabello(Guid id, string nombreCabello, string largoCabello)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador es obligatorio.", nameof(id));

        Id = id;
        NombreCabello = CatalogoValores.TextoObligatorio(nombreCabello, nameof(nombreCabello));
        LargoCabello = CatalogoValores.TextoObligatorio(largoCabello, nameof(largoCabello));
    }
}
