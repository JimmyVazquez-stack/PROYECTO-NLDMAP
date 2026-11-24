namespace NLDMAP.Domain.Reports;

public sealed record Reporte(
    string Id, string AutorId, DateTime RegistradoUtc, string EstadoRevision,
    PersonaReportada Persona, HechosReporte Hechos, UbicacionReporte Ubicacion,
    DatosAdicionalesReporte DatosAdicionales, bool AutorizaPublicacion, bool MantenerAnonimato)
{
    public Reportante? Reportante { get; init; }
}
