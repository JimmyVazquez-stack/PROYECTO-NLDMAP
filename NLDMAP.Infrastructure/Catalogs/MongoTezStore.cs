using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Catalogs;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Infrastructure.Catalogs;

public sealed class MongoTezStore : ITezStore
{
    private const int PageSize = 20;
    private readonly IMongoCollection<TezDocument> collection;

    public MongoTezStore(IMongoDatabase database)
        => collection = database.GetCollection<TezDocument>("catalogo_tez");

    public async Task<PageResult<TezRow>> GetPageAsync(TezQuery query)
    {
        var filters = Builders<TezDocument>.Filter;
        var filter = filters.Empty;
        var search = (query.Search ?? "").Trim();
        if (search.Length > 80) search = search[..80];

        var count = checked((int)await collection.CountDocumentsAsync(filter));
        var page = Math.Clamp(query.Page, 1, Math.Max(1, (count + PageSize - 1) / PageSize));
        var documents = await collection.Find(filter)
            .SortBy(x => x.Orden).ThenBy(x => x.ColorPiel).ThenBy(x => x.Id)
            .Skip((page - 1) * PageSize).Limit(PageSize).ToListAsync();
        return new PageResult<TezRow>(documents.Select(x => x.ToRow()).ToArray(), count, page, PageSize);
    }

    public async Task<TezRow?> GetAsync(Guid id)
    {
        var document = await collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        return document?.ToRow();
    }

    public Task CreateAsync(Tez tez, string actorId)
    {
        var now = DateTime.UtcNow;

        var document = new TezDocument
        {
            Id = tez.Id,
            ColorPiel = tez.ColorPiel,
            ColorHex = tez.ColorHex,
            Orden = tez.Orden,
            Version = Guid.NewGuid().ToString("N"),
            CreadoEnUtc = now,
            ActualizadoEnUtc = now,
            CreadoPor = actorId,
            ActualizadoPor = actorId
        };

        return collection.InsertOneAsync(document);
    }

    public async Task UpdateAsync(Tez tez, string expectedVersion, string actorId)
    {
        var filter = Builders<TezDocument>.Filter.Where(x =>
            x.Id == tez.Id && x.Version == expectedVersion);

        var update = MetadataUpdate(actorId)
            .Set(x => x.ColorPiel, tez.ColorPiel)
            .Set(x => x.ColorHex, tez.ColorHex)
            .Set(x => x.Orden, tez.Orden);

        EnsureUpdated(await collection.UpdateOneAsync(filter, update));
    }
    

    private static UpdateDefinition<TezDocument> MetadataUpdate(string actorId) =>
        Builders<TezDocument>.Update.Set(x => x.Version, Guid.NewGuid().ToString("N"))
            .Set(x => x.ActualizadoEnUtc, DateTime.UtcNow)
            .Set(x => x.ActualizadoPor, actorId);

    private static void EnsureUpdated(UpdateResult result)
    {
        if (result.MatchedCount != 1)
            throw new AdministrationException("La opción cambió o ya no existe. Actualiza el catálogo y vuelve a intentarlo.");
    }
    
}
