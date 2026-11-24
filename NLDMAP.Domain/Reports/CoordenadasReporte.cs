namespace NLDMAP.Domain.Reports;

public sealed record CoordenadasReporte
{
    public double Latitud { get; }
    public double Longitud { get; }

    public CoordenadasReporte(double latitud, double longitud)
    {
        if (!EsValida(latitud, longitud))
            throw new ArgumentException("La latitud debe estar entre −90 y 90 y la longitud entre −180 y 180.");

        Latitud = latitud;
        Longitud = longitud;
    }

    public static bool EsValida(double latitud, double longitud) =>
        latitud is >= -90 and <= 90 && longitud is >= -180 and <= 180;
}
