namespace NLDMAP.Domain.Reports;

public sealed record PersonaReportada(
    string? Nombres, string? PrimerApellido, string? SegundoApellido, DateTime? FechaNacimiento,
    string? Apodos, string? Sexo, string? OrientacionSexual, string? Nacionalidad,
    string? Ocupacion, string? NombreTez, string? ColorHexTez, string? RasgosFisicos)
{
    public string? Complexion { get; init; }
    public int? EstaturaCm { get; init; }
    public string? Curp { get; init; }
}
