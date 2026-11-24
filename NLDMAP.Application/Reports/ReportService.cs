using System.Globalization;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Catalogs;
using NLDMAP.Domain.Catalogs;
using NLDMAP.Domain.Reports;

namespace NLDMAP.Application.Reports;

public sealed class ReportService(ICurrentActor currentActor, ReportAccess access,
    IReportStore store, ReportCatalogs catalogs, ISepomexStore postal)
{
    public async Task<(ReportCatalogOptions Catalogs, Reportante Reporter)> GetCreationFormAsync()
    {
        var reporter = await access.RequireReporterAsync(await currentActor.GetAsync());
        return (await catalogs.GetAsync(), reporter);
    }

    public async Task<IReadOnlyList<AsentamientoPostal>> FindPostalAsync(string code)
    {
        await RequireCreateAsync();
        if (!IsPostalCode(code))
            return [];

        var matches = new List<AsentamientoPostal>();
        for (var page = 1; ; page++)
        {
            var result = await postal.GetPageAsync(code, page);
            matches.AddRange(result.Items.Where(item => item.CodigoPostal == code));
            if (page >= result.TotalPages)
                return matches;
        }
    }

    public async Task<string> CreateAsync(ReportInput input)
    {
        var actor = await currentActor.GetAsync();
        var reporter = await access.RequireReporterAsync(actor);
        ValidatePerson(input);
        ValidateFacts(input);
        ValidateLocation(input);
        ValidateVehicle(input);

        var options = await catalogs.GetAsync();
        var tone = Resolve(options.Tonos, input.TezId, "tez");
        var color = Resolve(options.Colores, input.ColorVehiculoId, "color del vehículo");
        var report = new Reporte(input.Id.ToString("N").ToUpperInvariant(), actor.UserId, DateTime.UtcNow, "Pendiente de revisión",
            new PersonaReportada(Text(input.Nombres, 120), Text(input.PrimerApellido, 120),
                Text(input.SegundoApellido, 120), Date(input.FechaNacimiento), Text(input.Apodos, 120),
                Resolve(options.Sexos, input.SexoId, "sexo")?.Nombre,
                Resolve(options.Orientaciones, input.OrientacionSexualId, "orientación sexual")?.Nombre,
                Resolve(options.Nacionalidades, input.NacionalidadId, "nacionalidad")?.Nombre,
                Resolve(options.Ocupaciones, input.OcupacionId, "ocupación")?.Nombre,
                tone?.Nombre, tone?.ColorHex, DescribePhysicalTraits(input, options))
            {
                Complexion = Resolve(options.Complexiones, input.ComplexionId, "complexión")?.Nombre,
                EstaturaCm = input.EstaturaCm,
                Curp = NormalizeCurp(input.Curp)
            },
            new HechosReporte(Date(input.FechaHechos), input.HoraHechos?.ToString(@"hh\:mm", CultureInfo.InvariantCulture),
                Date(input.FechaDeteccion), Resolve(options.Circunstancias, input.CircunstanciaId, "circunstancia")?.Nombre,
                Text(input.DescripcionHechos, 4000)!),
            new UbicacionReporte(Text(input.CodigoPostal, 5), Text(input.Estado, 120), Text(input.Municipio, 120),
                Text(input.Asentamiento, 160), Text(input.Calle, 200), Text(input.Referencias, 1000),
                input.Latitud is { } latitude && input.Longitud is { } longitude
                    ? new CoordenadasReporte(latitude, longitude) : null),
            new DatosAdicionalesReporte(Text(input.MarcaVehiculo, 120), Text(input.ModeloVehiculo, 120),
                color?.Nombre, Text(input.PlacaVehiculo, 30), Text(input.NombrePersonaRelacionada, 160),
                Text(input.DescripcionPersonaRelacionada, 1000))
            {
                ParentescoReportante = Resolve(options.Parentescos, input.ParentescoReportanteId, "vínculo de quien reporta")?.Nombre,
                ParentescoPersonaRelacionada = Resolve(options.Parentescos, input.ParentescoPersonaRelacionadaId, "vínculo de la persona involucrada")?.Nombre,
                AnoVehiculo = input.AnoVehiculo,
                ColorHexVehiculo = color?.ColorHex
            }, input.AutorizaPublicacion, input.MantenerAnonimato)
        {
            Reportante = reporter
        };
        await store.CreateAsync(report);
        return report.Id;
    }

    public async Task<IReadOnlyList<AnalysisReport>> GetAnalysisAsync(bool includeLocalExamples)
    {
        await access.RequireAnalysisAsync(await currentActor.GetAsync());
        return await store.GetAnalysisAsync(includeLocalExamples);
    }

    public Task<IReadOnlyList<ReportMapPoint>> GetPublicMapAsync() =>
        store.GetMapAsync(publicOnly: true, authorId: null);

    public async Task<IReadOnlyList<ReportMapPoint>> GetPrivateMapAsync(bool all)
    {
        var actor = await currentActor.GetAsync();
        await access.RequireReadAsync(actor, all);
        return await store.GetMapAsync(publicOnly: false, all ? null : actor.UserId);
    }

    public async Task<PageResult<ReportRow>> GetPageAsync(bool all, int page)
    {
        var actor = await currentActor.GetAsync();
        await access.RequireReadAsync(actor, all);
        return await store.GetPageAsync(all ? null : actor.UserId, page);
    }

    public async Task<Reporte?> GetAsync(bool all, string id)
    {
        var actor = await currentActor.GetAsync();
        await access.RequireReadAsync(actor, all);
        return await store.GetAsync(id, all ? null : actor.UserId);
    }

    public static void ValidatePerson(ReportInput input)
    {
        if (input.EstaturaCm is <= 0)
            throw new AdministrationException("Escribe una estatura mayor que cero en centímetros.");

        var curp = NormalizeCurp(input.Curp);
        if (string.IsNullOrWhiteSpace(input.Nombres) && string.IsNullOrWhiteSpace(input.Apodos) &&
            curp is null && input.EstaturaCm is null && input.TezId is null && input.ComplexionId is null &&
            input.Calzado?.TipoId is null && input.Cabello?.TipoId is null && input.DiscapacidadesIds.Count == 0 &&
            !input.Prendas.Any(prenda => prenda.TipoId.HasValue) && !input.Senas.Any(sena => sena.TipoId.HasValue))
            throw new AdministrationException("Indica el nombre, un apodo, la CURP o una descripción física de la persona.");
        ValidatePastDate(input.FechaNacimiento, "La fecha de nacimiento");
    }

    public static string? DescribePhysicalTraits(ReportInput input, ReportCatalogOptions options)
    {
        var hair = Resolve(options.TiposCabello, input.Cabello?.TipoId, "tipo de cabello");
        var footwear = Resolve(options.TiposCalzado, input.Calzado?.TipoId, "tipo de calzado");
        var parts = new List<string?> { DescribeTrait("Cabello", hair, input.Cabello?.Detalle) };
        foreach (var prenda in input.Prendas)
        {
            var tipo = Resolve(options.TiposPrenda, prenda.TipoId, "tipo de prenda");
            parts.Add(DescribeTrait("Ropa", tipo, prenda.Detalle));
        }

        parts.Add(DescribeTrait("Calzado", footwear, input.Calzado?.Detalle));
        foreach (var sena in input.Senas)
        {
            var tipo = Resolve(options.TiposSena, sena.TipoId, "tipo de seña particular");
            if (tipo is null)
                continue;

            var region = Resolve(options.RegionesCorporales, sena.RegionId, "región corporal");
            parts.Add(DescribeTrait("Seña particular", tipo, sena.Detalle, region?.Nombre));
        }

        if (input.DiscapacidadesIds.Count > 0)
        {
            var disabilities = input.DiscapacidadesIds.Distinct()
                .Select(id => Resolve(options.Discapacidades, id, "discapacidad")!.Nombre);
            parts.Add($"Discapacidades: {string.Join(" · ", disabilities)}");
        }

        var description = string.Join("\n", parts.OfType<string>());
        return description.Length == 0 ? null : description;
    }

    private static string? DescribeTrait(string label, ReportCatalogOption? option, string? detail, string? region = null)
    {
        if (option is null)
            return null;

        var description = string.Join(" · ", new[] { option.Nombre, region, Text(detail, ReportInput.TraitDetailMaxLength) }.OfType<string>());
        return $"{label}: {description}";
    }

    public static void ValidateFacts(ReportInput input)
    {
        if (string.IsNullOrWhiteSpace(input.DescripcionHechos))
            throw new AdministrationException("Describe los hechos para continuar.");
        ValidatePastDate(input.FechaHechos, "La fecha de los hechos");
        ValidatePastDate(input.FechaDeteccion, "La fecha en que se percató");
        if (input.FechaHechos?.Date > input.FechaDeteccion?.Date)
            throw new AdministrationException("La fecha en que se percató debe ser igual o posterior a la fecha de los hechos.");
        if (input.HoraHechos is { } time && (time < TimeSpan.Zero || time >= TimeSpan.FromDays(1)))
            throw new AdministrationException("Indica una hora válida.");
    }

    public static void ValidateLocation(ReportInput input)
    {
        if (Text(input.CodigoPostal, 5) is { } code && !IsPostalCode(code))
            throw new AdministrationException("Escribe el código postal con cinco dígitos.");
        if (input.Latitud.HasValue != input.Longitud.HasValue)
            throw new AdministrationException("Indica tanto la latitud como la longitud.");
        if (input.Latitud is { } latitude && input.Longitud is { } longitude &&
            !CoordenadasReporte.EsValida(latitude, longitude))
            throw new AdministrationException("Verifica los rangos de la latitud y la longitud.");
    }

    public static void ValidateVehicle(ReportInput input)
    {
        if (input.AnoVehiculo is < 1 or > 9999)
            throw new AdministrationException("Escribe un año del vehículo entre 1 y 9999.");
    }

    private async Task RequireCreateAsync()
    {
        var actor = await currentActor.GetAsync();
        if (!await access.CanCreateAsync(actor))
            throw new AdministrationException("Tu cuenta no tiene permiso para crear reportes.");
    }

    private static void ValidatePastDate(DateTime? value, string label)
    {
        if (value?.Date > DateTime.Now.Date)
            throw new AdministrationException($"{label} debe ser hoy o anterior.");
    }

    private static DateTime? Date(DateTime? value) =>
        value is { } date ? DateTime.SpecifyKind(date.Date, DateTimeKind.Utc) : null;

    private static bool IsPostalCode(string code) => code.Length == 5 && code.All(char.IsAsciiDigit);

    private static string? NormalizeCurp(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var curp = value.Trim();
        if (curp.Length != 18 || !curp.All(char.IsAsciiLetterOrDigit))
            throw new AdministrationException("Escribe los 18 caracteres alfanuméricos de la CURP, sin espacios.");

        return curp.ToUpperInvariant();
    }

    private static string? Text(string? value, int limit)
    {
        var text = value?.Trim();
        if (text?.Length > limit)
            throw new AdministrationException($"Hay un campo que supera el máximo de {limit} caracteres.");
        return string.IsNullOrEmpty(text) ? null : text;
    }

    private static ReportCatalogOption? Resolve(IReadOnlyList<ReportCatalogOption> options, Guid? id, string label) =>
        id is null ? null : options.FirstOrDefault(option => option.Id == id)
            ?? throw new AdministrationException($"La opción de {label} ya no existe. Recarga el formulario.");
}
