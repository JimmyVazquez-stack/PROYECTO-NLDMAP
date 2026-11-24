using MongoDB.Bson;
using MongoDB.Driver;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Catalogs;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Infrastructure.Catalogs;

public sealed class MongoVehiculoStore(IMongoDatabase database)
    : MongoCatalogStore<Vehiculo, VehiculoRow>(database, "catalogo_vehiculo", ["marca"]),
        IVehiculoStore
{
    public Task<PageResult<VehiculoRow>> GetPageAsync(VehiculoQuery query) =>
        GetPageAsync(query.Search, query.Page);

    protected override VehiculoRow Read(BsonDocument document) => new(
        ReadId(document),
        Text(document, "marca") ?? "");

    protected override Guid GetId(Vehiculo item) => item.Id;

    protected override BsonDocument Write(Vehiculo item) => new()
    {
        ["marca"] = TextValue(item.Marca)
    };
}
