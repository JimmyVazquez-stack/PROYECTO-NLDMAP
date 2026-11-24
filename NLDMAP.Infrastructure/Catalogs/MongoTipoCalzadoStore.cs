using MongoDB.Bson;
using MongoDB.Driver;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Catalogs;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Infrastructure.Catalogs;

public sealed class MongoTipoCalzadoStore(IMongoDatabase database)
    : MongoCatalogStore<TipoCalzado, TipoCalzadoRow>(database, "catalogo_tipo_calzado", ["nombreCalzado"]),
        ITipoCalzadoStore
{
    public Task<PageResult<TipoCalzadoRow>> GetPageAsync(TipoCalzadoQuery query) =>
        GetPageAsync(query.Search, query.Page);

    protected override TipoCalzadoRow Read(BsonDocument document) => new(
        ReadId(document),
        Text(document, "nombreCalzado") ?? "");

    protected override Guid GetId(TipoCalzado item) => item.Id;

    protected override BsonDocument Write(TipoCalzado item) => new()
    {
        ["nombreCalzado"] = TextValue(item.NombreCalzado)
    };
}
