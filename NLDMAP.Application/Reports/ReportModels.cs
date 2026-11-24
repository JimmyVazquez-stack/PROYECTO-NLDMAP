namespace NLDMAP.Application.Reports;

public sealed class ReportInput
{
    public const int TraitDetailMaxLength = 500;

    public Guid Id { get; } = Guid.NewGuid();
    public string? Nombres { get; set; }
    public string? PrimerApellido { get; set; }
    public string? SegundoApellido { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string? Curp { get; set; }
    public string? Apodos { get; set; }
    public Guid? SexoId { get; set; }
    public Guid? OrientacionSexualId { get; set; }
    public Guid? NacionalidadId { get; set; }
    public Guid? OcupacionId { get; set; }
    public int? EstaturaCm { get; set; }
    public Guid? ComplexionId { get; set; }
    public Guid? TezId { get; set; }
    public ReportTraitInput? Cabello { get; set; }
    public List<ReportTraitInput> Prendas { get; } = [];
    public ReportTraitInput? Calzado { get; set; }
    public List<ReportMarkInput> Senas { get; } = [];
    public IReadOnlyCollection<Guid> DiscapacidadesIds { get; set; } = [];
    public DateTime? FechaHechos { get; set; }
    public TimeSpan? HoraHechos { get; set; }
    public DateTime? FechaDeteccion { get; set; }
    public Guid? CircunstanciaId { get; set; }
    public string? DescripcionHechos { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Estado { get; set; }
    public string? Municipio { get; set; }
    public string? Asentamiento { get; set; }
    public string? Calle { get; set; }
    public string? Referencias { get; set; }
    public double? Latitud { get; set; }
    public double? Longitud { get; set; }
    public string? MarcaVehiculo { get; set; }
    public string? ModeloVehiculo { get; set; }
    public int? AnoVehiculo { get; set; }
    public Guid? ColorVehiculoId { get; set; }
    public string? PlacaVehiculo { get; set; }
    public Guid? ParentescoReportanteId { get; set; }
    public Guid? ParentescoPersonaRelacionadaId { get; set; }
    public string? NombrePersonaRelacionada { get; set; }
    public string? DescripcionPersonaRelacionada { get; set; }
    public bool AutorizaPublicacion { get; set; }
    public bool MantenerAnonimato { get; set; } = true;
}

public sealed class ReportTraitInput
{
    public Guid? TipoId { get; set; }
    public string? Detalle { get; set; }
}

public sealed class ReportMarkInput
{
    public Guid? TipoId { get; set; }
    public Guid? RegionId { get; set; }
    public string? Detalle { get; set; }
}

public sealed record ReportRow(string Id, DateTime RegistradoUtc, string EstadoRevision,
    string Persona, string? Municipio, string? Estado, string? NombreReportante = null);

public sealed record ReportCatalogOption(Guid Id, string Nombre, string? ColorHex = null);

public sealed record ReportCatalogOptions(
    IReadOnlyList<ReportCatalogOption> Sexos,
    IReadOnlyList<ReportCatalogOption> Orientaciones,
    IReadOnlyList<ReportCatalogOption> Nacionalidades,
    IReadOnlyList<ReportCatalogOption> Ocupaciones,
    IReadOnlyList<ReportCatalogOption> Tonos,
    IReadOnlyList<ReportCatalogOption> Circunstancias,
    IReadOnlyList<ReportCatalogOption> TiposCabello,
    IReadOnlyList<ReportCatalogOption> TiposPrenda,
    IReadOnlyList<ReportCatalogOption> TiposCalzado,
    IReadOnlyList<ReportCatalogOption> TiposSena,
    IReadOnlyList<ReportCatalogOption> RegionesCorporales,
    IReadOnlyList<ReportCatalogOption> Colores,
    IReadOnlyList<ReportCatalogOption> Parentescos,
    IReadOnlyList<ReportCatalogOption> Discapacidades,
    IReadOnlyList<ReportCatalogOption> Complexiones);

public sealed record AnalysisReport(string Folio, DateOnly? FechaHechos, string? Municipio,
    string? Circunstancia, NLDMAP.Domain.Reports.CoordenadasReporte? Coordenadas);

public sealed record ReportMapPoint(double Latitud, double Longitud, string? Municipio, string? Estado,
    string? Nombre = null);
