using MongoDB.Bson;
using MongoDB.Driver;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Catalogs;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Infrastructure.Catalogs;

public sealed class MongoDiscapacidadStore(IMongoDatabase database)
    : MongoCatalogStore<Discapacidad, DiscapacidadRow>(database, "catalogo_discapacidad", ["nombreDiscapacidad"]),
        IDiscapacidadStore
{
    public Task<PageResult<DiscapacidadRow>> GetPageAsync(DiscapacidadQuery query) =>
        GetPageAsync(query.Search, query.Page);

    protected override DiscapacidadRow Read(BsonDocument document) => new(
        ReadId(document),
        Text(document, "nombreDiscapacidad") ?? "");

    protected override Guid GetId(Discapacidad item) => item.Id;

    protected override BsonDocument Write(Discapacidad item) => new()
    {
        ["nombreDiscapacidad"] = TextValue(item.NombreDiscapacidad)
    };
}
