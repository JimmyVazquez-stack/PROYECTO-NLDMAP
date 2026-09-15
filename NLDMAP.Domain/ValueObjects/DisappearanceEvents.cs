using System.Runtime.InteropServices.JavaScript;

namespace NLDMAP.Domain.ValueObjects;

public sealed class DisappearanceEvents
{
    public  DateTime EventsDate { get; init; }
    public  TimeSpan EventsHour { get; init; }
    public  string Circumstance { get; init; } //Rumbo al trabajo ...
    public  string FactsDescription { get; init; } //como, cuando y donde
    public  bool ReporterPresent { get; set; } //si / no
        
    //Georreferenciacion
    public  string Street  { get; init; }
    public  string City  { get; init; }
    public  string State { get; init; }
    public  string Municipality { get; init; }
        
    //Postgis y Neo4j
    public double Latitude { get; init; }
    public double Longitude { get; init; }

    public DisappearanceEvents(
        DateTime eventsDate,
        TimeSpan eventsHour,
        string circumstance,
        string factsDescription,
        bool reporterPresent,
        string street,
        string city,
        string state,
        string municipality,
        double latitude,
        double longitude)
    {
        if (eventsDate >DateTime.UtcNow)
            throw new ArgumentException("La fecha de los hechos no puede ser futura.", nameof(eventsDate));
        
        if (string.IsNullOrWhiteSpace(circumstance))
            throw new ArgumentException("La circunstancia de desaparición es obligatoria.", nameof(circumstance));
        
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("El estado es obligatorio.", nameof(state));

        if (string.IsNullOrWhiteSpace(municipality))
            throw new ArgumentException("El municipio es obligatorio.", nameof(municipality));
        
        if(latitude < -90 || latitude > 90 ) 
            throw new ArgumentOutOfRangeException(nameof(latitude), "La latitud debe estar entre -90 y 90 grados.");
        
        if(longitude < -180 || longitude > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude), "La longitud debe estar entre -180 y 180 grados.");

        EventsDate = eventsDate;
        EventsHour = eventsHour;
        Circumstance = circumstance;
        FactsDescription = factsDescription;
        ReporterPresent = reporterPresent;
        Street = street;
        State = state;
        Municipality = municipality;
        Latitude = latitude;
        Longitude = longitude;
    }
} 
