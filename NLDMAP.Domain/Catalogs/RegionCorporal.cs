namespace NLDMAP.Domain.Catalogs;

public sealed class RegionCorporal
{
    public const int RegionMaxLength = 80;
    
    public Guid Id { get; }
    public string Region { get; }
    public int Orden { get;  }

    public RegionCorporal(Guid id, string region, int orden)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador  de la region es necesario");
        //Lo que hace esta funcion es preguntar si "region" es null, si es null usa el valor de la derecha "" y le 
        //aplica la funcion trim al string esto hace la eliminacion de los espacion en blanco al inicio y al final de 
        //la cadena
        region = (region ?? "").Trim();
        
        if (region.Length > RegionMaxLength)
            throw new ArgumentException("La region debe tener de 1 a 80 caracteres");
        if (orden is < 0 or > 9999)
            throw new ArgumentException("El orden debe ser entre 0 y 9999");



        Id = id;
        Region = region;
        Orden = orden;
    }
}