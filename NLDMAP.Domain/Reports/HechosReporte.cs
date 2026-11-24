namespace NLDMAP.Domain.Reports;

public sealed record HechosReporte(DateTime? Fecha, string? Hora, DateTime? FechaDeteccion,
    string? Circunstancia, string Descripcion);
