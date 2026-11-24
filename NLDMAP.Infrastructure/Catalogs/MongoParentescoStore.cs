using MongoDB.Bson;
using MongoDB.Driver;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Catalogs;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Infrastructure.Catalogs;

public sealed class MongoParentescoStore(IMongoDatabase database)
    : MongoCatalogStore<Parentesco, ParentescoRow>(database, "catalogo_parentesco", ["nombreParentesco"]),
        IParentescoStore
{
    public Task<PageResult<ParentescoRow>> GetPageAsync(ParentescoQuery query) =>
        GetPageAsync(query.Search, query.Page);

    protected override ParentescoRow Read(BsonDocument document) => new(
        ReadId(document),
        Text(document, "nombreParentesco") ?? "");

    protected override Guid GetId(Parentesco item) => item.Id;

    protected override BsonDocument Write(Parentesco item) => new()
    {
        ["nombreParentesco"] = TextValue(item.NombreParentesco)
    };
}
