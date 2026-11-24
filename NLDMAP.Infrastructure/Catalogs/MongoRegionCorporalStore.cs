using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Catalogs;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Infrastructure.Catalogs;

public sealed class MongoRegionCorporalStore : IRegionCorporalStore
{
    private const int PageSize = 20;
    private readonly IMongoCollection<RegionCorporalDocument> collection;

    public MongoRegionCorporalStore(IMongoDatabase database)
    {
        collection = database.GetCollection<RegionCorporalDocument>(
            "catalogo_region_corporal");
    }

    public async Task<PageResult<RegionCorporalRow>> GetPageAsync(
        RegionCorporalQuery query)
    {
        var filters = Builders<RegionCorporalDocument>.Filter;
        var filter = filters.Empty;
        var search = (query.Search ?? "").Trim();

        if (search.Length > 0)
        {
            var expression = new BsonRegularExpression(Regex.Escape(search), "i");
            filter = filters.Regex(document => document.Region, expression);
        }

        var total = checked((int)await collection.CountDocumentsAsync(filter));
        var totalPages = Math.Max(1, (int)Math.Ceiling((double)total / PageSize));
        var page = Math.Clamp(query.Page, 1, totalPages);

        var documents = await collection.Find(filter)
            .SortBy(document => document.Region)
            .ThenBy(document => document.Id)
            .Skip((page - 1) * PageSize)
            .Limit(PageSize)
            .ToListAsync();

        var rows = documents.Select(document => document.ToRow()).ToArray();
        return new PageResult<RegionCorporalRow>(rows, total, page, PageSize);
    }

    public async Task<RegionCorporalRow?> GetAsync(Guid id)
    {
        var document = await collection.Find(item => item.Id == id)
            .FirstOrDefaultAsync();

        return document?.ToRow();
    }

    public Task CreateAsync(RegionCorporal region, string actorId)
    {
        var document = new RegionCorporalDocument
        {
            Id = region.Id,
            Region = region.Region,
            ActualizadoEnUtc = DateTime.UtcNow,
            ActualizadoPor = actorId
        };

        return collection.InsertOneAsync(document);
    }

    public async Task UpdateAsync(
        RegionCorporal region, string actorId)
    {
        var filter = Builders<RegionCorporalDocument>.Filter.Where(document =>
            document.Id == region.Id);

        var update = Builders<RegionCorporalDocument>.Update
            .Set(document => document.Region, region.Region)
            .Set(document => document.ActualizadoEnUtc, DateTime.UtcNow)
            .Set(document => document.ActualizadoPor, actorId)
            .Unset("orden")
            .Unset("version");

        var result = await collection.UpdateOneAsync(filter, update);

        if (result.MatchedCount == 0)
        {
            throw new AdministrationException(
                "La región ya no existe. Actualiza el catálogo y vuelve a intentarlo.");
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var result = await collection.DeleteOneAsync(document => document.Id == id);
        if (result.DeletedCount == 0)
            throw new AdministrationException("La opción ya no existe. Actualiza el catálogo.");
    }
}
