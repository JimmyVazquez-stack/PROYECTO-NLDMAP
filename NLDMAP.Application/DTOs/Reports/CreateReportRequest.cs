using NLDMAP.Domain.ValueObjects;

namespace NLDMAP.Application.DTOs;

public class CreateReportRequest
{
    //Datos de la persona
    public string MissingPersonName { get; set; } = string.Empty;
    public int MissingPersonAge { get; set; }
    public string MissingPersonGender { get; set; } = string.Empty;
    public double? MissingPersonHeight { get; set; }
    public decimal MissingPersonWeight { get; set; }
    
    //nucleo minimo segun diagrama A-01
    public string ClothingDescription { get; set; } = string.Empty; //Vestimenta
    public string DistinctiveFeatures { get; set; } = string.Empty; //Senas particulares
    public string PhotoBase64 { get; set; } = string.Empty; //Foto enviada en texto
    
    //Datos de los hechos
    public DateTime DisappearanceDate { get; set; }
    public  TimeSpan DisappearanceHour {get; set;}
    public string Circumstance{ get; set; } //Circunstancia de desaparicion - Rumbo al trabajo ...
    public string FactsDescription { get; set; } = string.Empty; //Descripcion de los hechos
    public bool ReporterPresent { get; set; } //si / no
    
    //Ubicacion
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Municipality { get; set; } = string.Empty;
    public double LastSightingLatitude { get; set; }
    public double LastSightingLongitude { get; set; }
    
    //Datos del reporte
    public Guid CreatorId { get; set; }
    public string MissingPersonRelation { get; set; } = string.Empty;
    public bool ConsentUseExclusive { get; set; }
    public bool RequestInformationPublic { get; set; }
}