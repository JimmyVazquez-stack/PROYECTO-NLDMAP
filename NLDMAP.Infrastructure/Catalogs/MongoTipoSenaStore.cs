using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Catalogs;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Infrastructure.Catalogs;

public sealed class MongoTipoSenaStore : ITipoSenaStore
{
    private const int PageSize = 20;
    private readonly IMongoCollection<TipoSenaDocument> collection;

    public MongoTipoSenaStore(IMongoDatabase database)
    {
        collection = database.GetCollection<TipoSenaDocument>(
            "catalogo_tipo_sena");
    }

    public async Task<PageResult<TipoSenaRow>> GetPageAsync(
        TipoSenaQuery query)
    {
        var filters = Builders<TipoSenaDocument>.Filter;
        var filter = filters.Empty;
        var search = (query.Search ?? "").Trim();

        if (search.Length > 0)
        {
            var expression = new BsonRegularExpression(Regex.Escape(search), "i");
            filter = filters.Regex(document => document.Nombre, expression);
        }

        var total = checked((int)await collection.CountDocumentsAsync(filter));
        var totalPages = Math.Max(1, (int)Math.Ceiling((double)total / PageSize));
        var page = Math.Clamp(query.Page, 1, totalPages);

        var documents = await collection.Find(filter)
            .SortBy(document => document.Orden)
            .ThenBy(document => document.Nombre)
            .ThenBy(document => document.Id)
            .Skip((page - 1) * PageSize)
            .Limit(PageSize)
            .ToListAsync();

        var rows = documents.Select(document => document.ToRow()).ToArray();
        return new PageResult<TipoSenaRow>(rows, total, page, PageSize);
    }

    public async Task<TipoSenaRow?> GetAsync(Guid id)
    {
        var document = await collection.Find(item => item.Id == id)
            .FirstOrDefaultAsync();

        return document?.ToRow();
    }

    public Task CreateAsync(TipoSena tipoSena, string actorId)
    {
        var document = new TipoSenaDocument
        {
            Id = tipoSena.Id,
            Nombre = tipoSena.Nombre,
            Orden = tipoSena.Orden,
            Version = Guid.NewGuid().ToString("N"),
            ActualizadoEnUtc = DateTime.UtcNow,
            ActualizadoPor = actorId
        };

        return collection.InsertOneAsync(document);
    }

    public async Task UpdateAsync(
        TipoSena tipoSena, string expectedVersion, string actorId)
    {
        var filter = Builders<TipoSenaDocument>.Filter.Where(document =>
            document.Id == tipoSena.Id && document.Version == expectedVersion);

        var update = Builders<TipoSenaDocument>.Update
            .Set(document => document.Nombre, tipoSena.Nombre)
            .Set(document => document.Orden, tipoSena.Orden)
            .Set(document => document.Version, Guid.NewGuid().ToString("N"))
            .Set(document => document.ActualizadoEnUtc, DateTime.UtcNow)
            .Set(document => document.ActualizadoPor, actorId);

        var result = await collection.UpdateOneAsync(filter, update);

        if (result.MatchedCount == 0)
        {
            throw new AdministrationException(
                "El tipo de seña cambió o ya no existe. Actualiza el catálogo y vuelve a intentarlo.");
        }
    }
}
