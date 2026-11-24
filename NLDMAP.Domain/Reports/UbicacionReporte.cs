namespace NLDMAP.Domain.Reports;

public sealed record UbicacionReporte(string? CodigoPostal, string? Estado, string? Municipio,
    string? Asentamiento, string? Calle, string? Referencias, CoordenadasReporte? Coordenadas = null);
