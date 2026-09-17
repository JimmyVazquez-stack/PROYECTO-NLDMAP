namespace NLDMAP.Domain.ValueObjects;

public class Coordinate
{
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public string GeoSource { get; private set; } // Contenido String: "GPS", "HIBRIDO", "MANUAL"
    public DateTime Timestamp { get; private set; }

    public Coordinate(double latitude, double longitude, string geoSource = "GPS")
    {
        if (latitude < -90 || latitude > 90) 
            throw new ArgumentException("Latitud inválida, debe estar entre -90 y 90.");
            
        if (longitude < -180 || longitude > 180) 
            throw new ArgumentException("Longitud inválida, debe estar entre -180 y 180.");

        Latitude = latitude;
        Longitude = longitude;
        GeoSource = geoSource;
        Timestamp = DateTime.UtcNow;
    }
}