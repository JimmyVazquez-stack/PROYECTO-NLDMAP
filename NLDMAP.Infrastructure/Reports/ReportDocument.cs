using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NLDMAP.Domain.Reports;

namespace NLDMAP.Infrastructure.Reports;

internal static class ReportDocument
{
    internal static BsonDocument Write(Reporte report) => new()
    {
        ["_id"] = Identifier(report.Id),
        ["folioInterno"] = report.Id,
        ["idUsuario"] = Identifier(report.AutorId),
        ["reportante"] = report.Reportante is { } reporter
            ? new BsonDocument
            {
                ["nombreCompleto"] = TextValue(reporter.NombreCompleto),
                ["correo"] = TextValue(reporter.Correo),
                ["telefono"] = TextValue(reporter.Telefono)
            }
            : BsonNull.Value,
        ["fechaRegistro"] = report.RegistradoUtc,
        ["estadoRevision"] = new BsonDocument
        {
            ["codigo"] = "pendiente",
            ["nombre"] = report.EstadoRevision
        },
        ["descripcionPersona"] = new BsonDocument
        {
            ["nombres"] = TextValue(report.Persona.Nombres),
            ["primerApellido"] = TextValue(report.Persona.PrimerApellido),
            ["segundoApellido"] = TextValue(report.Persona.SegundoApellido),
            ["fechaNacimiento"] = DateValue(report.Persona.FechaNacimiento),
            ["estaturaCm"] = report.Persona.EstaturaCm is { } height ? new BsonInt32(height) : BsonNull.Value,
            ["curp"] = TextValue(report.Persona.Curp),
            ["apodos"] = TextValue(report.Persona.Apodos),
            ["descripcionIdentificativa"] = TextValue(report.Persona.RasgosFisicos)
        },
        ["hechos"] = new BsonDocument
        {
            ["fechaHechos"] = DateValue(report.Hechos.Fecha),
            ["horaHechos"] = TextValue(report.Hechos.Hora),
            ["fechaPercato"] = DateValue(report.Hechos.FechaDeteccion),
            ["descripcionHechos"] = report.Hechos.Descripcion
        },
        ["ubicacion"] = new BsonDocument
        {
            ["codigoPostal"] = TextValue(report.Ubicacion.CodigoPostal),
            ["estado"] = TextValue(report.Ubicacion.Estado),
            ["asentamiento"] = TextValue(report.Ubicacion.Asentamiento),
            ["calle"] = TextValue(report.Ubicacion.Calle),
            ["referencias"] = TextValue(report.Ubicacion.Referencias),
            ["coordenadas"] = report.Ubicacion.Coordenadas is { } point
                ? new BsonDocument { ["latitud"] = point.Latitud, ["longitud"] = point.Longitud }
                : BsonNull.Value
        },
        ["catalogos"] = new BsonDocument
        {
            ["sexo"] = TextValue(report.Persona.Sexo),
            ["orientacionSexual"] = TextValue(report.Persona.OrientacionSexual),
            ["nacionalidad"] = TextValue(report.Persona.Nacionalidad),
            ["complexion"] = TextValue(report.Persona.Complexion),
            ["ocupacion"] = TextValue(report.Persona.Ocupacion),
            ["tez"] = new BsonDocument
            {
                ["nombre"] = TextValue(report.Persona.NombreTez),
                ["colorHex"] = TextValue(report.Persona.ColorHexTez)
            },
            ["circunstancia"] = TextValue(report.Hechos.Circunstancia),
            ["municipio"] = TextValue(report.Ubicacion.Municipio)
        },
        ["vehiculo"] = new BsonDocument
        {
            ["marca"] = TextValue(report.DatosAdicionales.MarcaVehiculo),
            ["modelo"] = TextValue(report.DatosAdicionales.ModeloVehiculo),
            ["ano"] = report.DatosAdicionales.AnoVehiculo is { } year ? new BsonInt32(year) : BsonNull.Value,
            ["color"] = TextValue(report.DatosAdicionales.ColorVehiculo),
            ["colorHex"] = TextValue(report.DatosAdicionales.ColorHexVehiculo),
            ["numeroPlaca"] = TextValue(report.DatosAdicionales.PlacaVehiculo)
        },
        ["personaInvolucrada"] = new BsonDocument
        {
            ["nombre"] = TextValue(report.DatosAdicionales.NombrePersonaRelacionada),
            ["parentescoConVictima"] = TextValue(report.DatosAdicionales.ParentescoPersonaRelacionada),
            ["descripcion"] = TextValue(report.DatosAdicionales.DescripcionPersonaRelacionada)
        },
        ["parentescoReportante"] = TextValue(report.DatosAdicionales.ParentescoReportante),
        ["consentimiento"] = report.AutorizaPublicacion,
        ["anonimo"] = report.MantenerAnonimato
    };

    internal static Reporte Read(BsonDocument row)
    {
        if (row.Contains("RegistradoUtc"))
        {
            // El primer formulario escribía directamente el modelo de dominio.
            var original = new BsonDocument(row);
            original.Remove("fechaOrden");
            return BsonSerializer.Deserialize<Reporte>(original);
        }

        var persona = Section(row, "descripcionPersona");
        var hechos = Section(row, "hechos");
        var ubicacion = Section(row, "ubicacion");
        var catalogos = Section(row, "catalogos");
        var tez = Section(catalogos, "tez");
        var revision = Section(row, "estadoRevision");
        var vehiculo = Section(row, "vehiculo");
        var relacionada = Section(row, "personaInvolucrada");

        return new Reporte(Text(row, "folioInterno") ?? ReadIdentifier(row, "_id"), ReadIdentifier(row, "idUsuario"),
            Date(row, "fechaRegistro") ?? DateTime.UnixEpoch, Text(revision, "nombre") ?? "Pendiente de revisión",
            new PersonaReportada(Text(persona, "nombres"), Text(persona, "primerApellido"), Text(persona, "segundoApellido"),
                Date(persona, "fechaNacimiento"), Text(persona, "apodos"), Text(catalogos, "sexo"),
                Text(catalogos, "orientacionSexual"), Text(catalogos, "nacionalidad"), Text(catalogos, "ocupacion"),
                Text(tez, "nombre") ?? Text(tez, "nombreColor") ?? Text(tez, "colorPiel"), Text(tez, "colorHex"),
                DescribePerson(persona))
            {
                Complexion = Text(catalogos, "complexion"),
                EstaturaCm = persona.GetValue("estaturaCm", BsonNull.Value) is BsonInt32 height ? height.Value : null,
                Curp = Text(persona, "curp")
            },
            new HechosReporte(Date(hechos, "fechaHechos"), Text(hechos, "horaHechos"), Date(hechos, "fechaPercato"),
                Text(catalogos, "circunstancia"), Text(hechos, "descripcionHechos") ?? ""),
            new UbicacionReporte(Text(ubicacion, "codigoPostal"), Text(ubicacion, "estado"),
                Text(catalogos, "municipio") ?? Text(ubicacion, "municipio"), Text(ubicacion, "asentamiento"),
                Text(ubicacion, "calle"), Text(ubicacion, "referencias"), ReadCoordinates(ubicacion)),
            new DatosAdicionalesReporte(Text(vehiculo, "marca"), Text(vehiculo, "modelo"), Text(vehiculo, "color"),
                Text(vehiculo, "numeroPlaca"), Text(relacionada, "nombre"), Text(relacionada, "descripcion"))
            {
                ParentescoReportante = Text(row, "parentescoReportante"),
                ParentescoPersonaRelacionada = Text(relacionada, "parentescoConVictima"),
                AnoVehiculo = vehiculo.GetValue("ano", BsonNull.Value) is BsonInt32 year ? year.Value : null,
                ColorHexVehiculo = Text(vehiculo, "colorHex")
            },
            Boolean(row, "consentimiento", false), Boolean(row, "anonimo", true))
        {
            Reportante = row.GetValue("reportante", BsonNull.Value) is BsonDocument reporter
                ? new Reportante(Text(reporter, "nombreCompleto"), Text(reporter, "correo"), Text(reporter, "telefono"))
                : null
        };
    }

    internal static CoordenadasReporte? ReadCoordinates(BsonDocument location)
    {
        var point = Section(location, "coordenadas");
        var latitude = point.GetValue("latitud", BsonNull.Value);
        var longitude = point.GetValue("longitud", BsonNull.Value);
        return latitude.IsNumeric && longitude.IsNumeric &&
            CoordenadasReporte.EsValida(latitude.ToDouble(), longitude.ToDouble())
            ? new CoordenadasReporte(latitude.ToDouble(), longitude.ToDouble())
            : null;
    }

    private static string? DescribePerson(BsonDocument persona)
    {
        var descriptions = new[] { "descripcionIdentificativa", "cabello", "vestimenta", "calzado" }
            .Select(field => Text(persona, field)).Where(value => !string.IsNullOrWhiteSpace(value));
        var description = string.Join(" · ", descriptions);
        return description.Length == 0 ? null : description;
    }

    private static BsonValue Identifier(string value) => Guid.TryParse(value, out var id)
        ? new BsonBinaryData(id, GuidRepresentation.Standard) : new BsonString(value);

    private static string ReadIdentifier(BsonDocument doc, string field)
    {
        var value = doc.GetValue(field, BsonNull.Value);
        if (value.IsString)
            return value.AsString;

        return value is BsonBinaryData { SubType: BsonBinarySubType.UuidStandard } binary
            ? binary.ToGuid(GuidRepresentation.Standard).ToString() : "";
    }

    private static BsonDocument Section(BsonDocument doc, string field) =>
        doc.GetValue(field, BsonNull.Value) is BsonDocument section ? section : new BsonDocument();

    private static string? Text(BsonDocument doc, string field) =>
        doc.GetValue(field, BsonNull.Value) is BsonString text ? text.Value : null;

    private static DateTime? Date(BsonDocument doc, string field) =>
        doc.GetValue(field, BsonNull.Value) is BsonDateTime date ? date.ToUniversalTime() : null;

    private static bool Boolean(BsonDocument doc, string field, bool fallback) =>
        doc.GetValue(field, BsonNull.Value) is BsonBoolean flag ? flag.Value : fallback;

    private static BsonValue TextValue(string? text) => text is null ? BsonNull.Value : new BsonString(text);
    private static BsonValue DateValue(DateTime? date) => date is null ? BsonNull.Value : new BsonDateTime(date.Value);
}
