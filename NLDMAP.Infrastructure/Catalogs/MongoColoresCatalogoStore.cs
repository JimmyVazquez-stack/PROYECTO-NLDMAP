using MongoDB.Bson;
using MongoDB.Driver;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Catalogs;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Infrastructure.Catalogs;

public sealed class MongoColoresCatalogoStore(IMongoDatabase database)
    : MongoCatalogStore<ColoresCatalogo, ColoresCatalogoRow>(database, "catalogo_color", ["colorNombre", "colorHex"]),
        IColoresCatalogoStore
{
    public Task<PageResult<ColoresCatalogoRow>> GetPageAsync(ColoresCatalogoQuery query) =>
        GetPageAsync(query.Search, query.Page);

    protected override ColoresCatalogoRow Read(BsonDocument document) => new(
        ReadId(document),
        Text(document, "colorNombre") ?? "",
        Text(document, "colorHex") ?? "");

    protected override Guid GetId(ColoresCatalogo item) => item.Id;

    protected override BsonDocument Write(ColoresCatalogo item) => new()
    {
        ["colorNombre"] = TextValue(item.ColorNombre),
        ["colorHex"] = TextValue(item.ColorHex)
    };
}
