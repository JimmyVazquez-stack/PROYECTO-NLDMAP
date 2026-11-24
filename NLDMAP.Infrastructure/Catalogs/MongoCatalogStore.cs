using System.Globalization;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using NLDMAP.Application.Administration;

namespace NLDMAP.Infrastructure.Catalogs;

public abstract class MongoCatalogStore<TEntity, TRow>(
    IMongoDatabase database, string collectionName, string[] textFields, string? numericField = null)
{
    private const int PageSize = 20;
    private readonly IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(collectionName);

    protected abstract TRow Read(BsonDocument document);
    protected abstract Guid GetId(TEntity item);
    protected abstract BsonDocument Write(TEntity item);

    protected async Task<PageResult<TRow>> GetPageAsync(string? search, int page)
    {
        var filters = Builders<BsonDocument>.Filter;
        var filter = filters.Empty;
        var text = (search ?? "").Trim();
        if (text.Length > 120)
            text = text[..120];

        if (text.Length > 0)
        {
            var expression = new BsonRegularExpression(Regex.Escape(text), "i");
            var matches = textFields.Select(field => filters.Regex(field, expression)).ToList();
            if (numericField is not null &&
                int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var number))
                matches.Add(filters.Eq(numericField, number));
            filter = filters.Or(matches);
        }

        var total = checked((int)await collection.CountDocumentsAsync(filter));
        page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling((double)total / PageSize)));
        var sort = Builders<BsonDocument>.Sort.Ascending(textFields[0]).Ascending("_id");
        var documents = await collection.Find(filter).Sort(sort)
            .Skip((page - 1) * PageSize).Limit(PageSize).ToListAsync();
        return new PageResult<TRow>(documents.Select(Read).ToArray(), total, page, PageSize);
    }

    public Task CreateAsync(TEntity item, string actorId)
    {
        var now = DateTime.UtcNow;
        var document = Write(item);
        document["_id"] = BinaryId(GetId(item));
        document["creadoEnUtc"] = now;
        document["creadoPor"] = actorId;
        document["actualizadoEnUtc"] = now;
        document["actualizadoPor"] = actorId;
        return collection.InsertOneAsync(document);
    }

    public async Task UpdateAsync(TEntity item, string actorId)
    {
        var builder = Builders<BsonDocument>.Update;
        var updates = Write(item).Select(field => builder.Set(field.Name, field.Value)).ToList();
        updates.Add(builder.Set("actualizadoEnUtc", DateTime.UtcNow)
            .Set("actualizadoPor", actorId).Unset("orden").Unset("version"));
        var result = await collection.UpdateOneAsync(IdFilter(GetId(item)), builder.Combine(updates));
        EnsureExists(result.MatchedCount);
    }

    public async Task DeleteAsync(Guid id)
    {
        var result = await collection.DeleteOneAsync(IdFilter(id));
        EnsureExists(result.DeletedCount);
    }

    protected static Guid ReadId(BsonDocument document) =>
        document["_id"].AsBsonBinaryData.ToGuid(GuidRepresentation.Standard);

    protected static string? Text(BsonDocument document, string field) =>
        document.TryGetValue(field, out var value) && value.IsString ? value.AsString : null;

    protected static int? Integer(BsonDocument document, string field) =>
        document.TryGetValue(field, out var value) && value.IsInt32 ? value.AsInt32 : null;

    protected static BsonValue TextValue(string? value) =>
        value is null ? BsonNull.Value : new BsonString(value);

    private static BsonBinaryData BinaryId(Guid id) => new(id, GuidRepresentation.Standard);

    private static FilterDefinition<BsonDocument> IdFilter(Guid id) =>
        Builders<BsonDocument>.Filter.Eq("_id", BinaryId(id));

    private static void EnsureExists(long count)
    {
        if (count == 0)
            throw new AdministrationException("La opción ya no existe. Actualiza el catálogo.");
    }
}
