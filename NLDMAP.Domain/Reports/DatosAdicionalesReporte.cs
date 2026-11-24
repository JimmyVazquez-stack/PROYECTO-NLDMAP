namespace NLDMAP.Domain.Reports;

public sealed record DatosAdicionalesReporte(
    string? MarcaVehiculo, 
    string? ModeloVehiculo,
    string? ColorVehiculo, 
    string? PlacaVehiculo, 
    string? NombrePersonaRelacionada, 
    string? DescripcionPersonaRelacionada)
{
    public string? ParentescoReportante { get; init; }
    public string? ParentescoPersonaRelacionada { get; init; }
    public int? AnoVehiculo { get; init; }
    public string? ColorHexVehiculo { get; init; }
}
