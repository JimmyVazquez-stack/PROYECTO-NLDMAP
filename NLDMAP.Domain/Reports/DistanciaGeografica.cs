namespace NLDMAP.Domain.Reports;

public static class DistanciaGeografica
{
    public static double Kilometros(CoordenadasReporte origen, double latitud, double longitud)
    {
        const double gradosARadianes = Math.PI / 180;
        const double radioTierraKm = 6371.0088;
        var latInicial = origen.Latitud * gradosARadianes;
        var latFinal = latitud * gradosARadianes;
        var diferenciaLatitud = (latFinal - latInicial) / 2;
        var diferenciaLongitud = (longitud - origen.Longitud) * gradosARadianes / 2;
        var senoLatitud = Math.Sin(diferenciaLatitud);
        var senoLongitud = Math.Sin(diferenciaLongitud);
        var haversine = senoLatitud * senoLatitud +
            Math.Cos(latInicial) * Math.Cos(latFinal) * senoLongitud * senoLongitud;
        return 2 * radioTierraKm * Math.Asin(Math.Sqrt(Math.Clamp(haversine, 0, 1)));
    }
}
