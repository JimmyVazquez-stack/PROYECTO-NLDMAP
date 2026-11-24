using MongoDB.Bson;
using MongoDB.Driver;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Catalogs;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Infrastructure.Catalogs;

public sealed class MongoComplexionStore(IMongoDatabase database)
    : MongoCatalogStore<Complexion, ComplexionRow>(database, "catalogo_complexion", ["nombreComplexion"]),
        IComplexionStore
{
    public Task<PageResult<ComplexionRow>> GetPageAsync(ComplexionQuery query) =>
        GetPageAsync(query.Search, query.Page);

    protected override ComplexionRow Read(BsonDocument document) => new(
        ReadId(document),
        Text(document, "nombreComplexion") ?? "");

    protected override Guid GetId(Complexion item) => item.Id;

    protected override BsonDocument Write(Complexion item) => new()
    {
        ["nombreComplexion"] = TextValue(item.NombreComplexion)
    };
}
