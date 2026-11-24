using MongoDB.Bson;
using MongoDB.Driver;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Catalogs;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Infrastructure.Catalogs;

public sealed class MongoGeneroStore(IMongoDatabase database)
    : MongoCatalogStore<Genero, GeneroRow>(database, "catalogo_genero", ["nombreSexo"]),
        IGeneroStore
{
    public Task<PageResult<GeneroRow>> GetPageAsync(GeneroQuery query) =>
        GetPageAsync(query.Search, query.Page);

    protected override GeneroRow Read(BsonDocument document) => new(
        ReadId(document),
        Text(document, "nombreSexo") ?? "");

    protected override Guid GetId(Genero item) => item.Id;

    protected override BsonDocument Write(Genero item) => new()
    {
        ["nombreSexo"] = TextValue(item.NombreSexo)
    };
}
