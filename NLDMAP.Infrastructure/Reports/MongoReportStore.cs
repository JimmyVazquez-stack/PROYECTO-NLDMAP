using MongoDB.Bson;
using MongoDB.Driver;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Reports;
using NLDMAP.Domain.Reports;

namespace NLDMAP.Infrastructure.Reports;

public sealed class MongoReportStore(IMongoDatabase database) : IReportStore
{
    private const int PageSize = 20;
    private readonly IMongoCollection<BsonDocument> reports = database.GetCollection<BsonDocument>("reportes");

    public async Task CreateAsync(Reporte report)
    {
        try
        {
            await reports.InsertOneAsync(ReportDocument.Write(report));
        }
        catch (MongoWriteException exception) when (exception.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            if (await GetAsync(report.Id, report.AutorId) is null)
                throw new AdministrationException("El folio ya existe. Recarga el formulario.");
        }
    }

    public async Task<PageResult<ReportRow>> GetPageAsync(string? authorId, int page)
    {
        var filter = AuthorFilter(authorId);
        var total = checked((int)await reports.CountDocumentsAsync(filter));
        page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling((double)total / PageSize)));
        var documents = await reports.Aggregate().Match(filter)
            .AppendStage<BsonDocument>(new BsonDocument("$addFields", new BsonDocument("fechaOrden",
                new BsonDocument("$ifNull", new BsonArray { "$fechaRegistro", "$RegistradoUtc" }))))
            .Sort(Builders<BsonDocument>.Sort.Descending("fechaOrden").Ascending("_id"))
            .Skip((page - 1) * PageSize).Limit(PageSize).ToListAsync();

        var rows = documents.Select(document =>
        {
            var report = ReportDocument.Read(document);
            var name = string.Join(" ", new[] { report.Persona.Nombres, report.Persona.PrimerApellido, report.Persona.SegundoApellido }
                .Where(part => !string.IsNullOrWhiteSpace(part)));
            return new ReportRow(report.Id, report.RegistradoUtc, report.EstadoRevision,
                name.Length > 0 ? name : report.Persona.Apodos ?? "Sin nombre registrado",
                report.Ubicacion.Municipio, report.Ubicacion.Estado, report.Reportante?.NombreCompleto);
        }).ToArray();

        return new PageResult<ReportRow>(rows, total, page, PageSize);
    }

    public async Task<Reporte?> GetAsync(string id, string? authorId)
    {
        var filters = Builders<BsonDocument>.Filter;
        var identifier = filters.Eq("folioInterno", id) | filters.Eq("_id", id);
        if (Guid.TryParse(id, out var reportId))
            identifier |= filters.Eq("_id", new BsonBinaryData(reportId, GuidRepresentation.Standard));

        var filter = AuthorFilter(authorId) & identifier;
        var document = await reports.Find(filter).FirstOrDefaultAsync();
        return document is null ? null : ReportDocument.Read(document);
    }

    public async Task<IReadOnlyList<ReportMapPoint>> GetMapAsync(bool publicOnly, string? authorId)
    {
        var filters = Builders<BsonDocument>.Filter;
        var filter = filters.Type("ubicacion.coordenadas", BsonType.Document);
        if (publicOnly)
        {
            filter &= filters.Eq("consentimiento", true)
                & filters.Eq("estadoRevision.codigo", "aprobado")
                & filters.Eq("estadoPublicacion", "publicado");
        }
        else
        {
            filter &= AuthorFilter(authorId);
        }

        var projection = Builders<BsonDocument>.Projection
            .Include("ubicacion.coordenadas").Include("ubicacion.estado").Include("catalogos.municipio").Exclude("_id");
        if (!publicOnly)
        {
            projection = projection.Include("descripcionPersona.nombres")
                .Include("descripcionPersona.primerApellido").Include("descripcionPersona.segundoApellido");
        }

        const int limit = 1000;
        var documents = await reports.Find(filter).Project(projection)
            .Sort(Builders<BsonDocument>.Sort.Descending("fechaRegistro").Ascending("_id"))
            .Limit(limit).ToListAsync();

        var points = new List<ReportMapPoint>();
        foreach (var row in documents)
        {
            var location = row["ubicacion"].AsBsonDocument;
            if (ReportDocument.ReadCoordinates(location) is not { } point)
                continue;

            var catalogs = row.GetValue("catalogos", BsonNull.Value) as BsonDocument;
            var municipality = ReadText(catalogs, "municipio");
            var state = ReadText(location, "estado");
            if (publicOnly)
            {
                points.Add(new ReportMapPoint(Math.Round(point.Latitud, 3), Math.Round(point.Longitud, 3),
                    municipality, state));
            }
            else
            {
                var person = row.GetValue("descripcionPersona", BsonNull.Value) as BsonDocument;
                var name = string.Join(" ", new[] { ReadText(person, "nombres"),
                    ReadText(person, "primerApellido"), ReadText(person, "segundoApellido") }
                    .Where(part => !string.IsNullOrWhiteSpace(part)));
                points.Add(new ReportMapPoint(point.Latitud, point.Longitud, municipality, state,
                    name.Length == 0 ? null : name));
            }
        }

        return points;
    }

    private static string? ReadText(BsonDocument? row, string field) =>
        row?.GetValue(field, BsonNull.Value) is BsonString text ? text.Value : null;

    private static FilterDefinition<BsonDocument> AuthorFilter(string? authorId)
    {
        var filters = Builders<BsonDocument>.Filter;
        if (authorId is null)
            return filters.Empty;

        var filter = filters.Eq("AutorId", authorId) | filters.Eq("idUsuario", authorId);
        if (Guid.TryParse(authorId, out var author))
            filter |= filters.Eq("idUsuario", new BsonBinaryData(author, GuidRepresentation.Standard));

        return filter;
    }
}
