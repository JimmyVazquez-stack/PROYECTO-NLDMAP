using MongoDB.Bson;
using MongoDB.Driver;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Catalogs;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Infrastructure.Catalogs;

public sealed class MongoNacionalidadStore(IMongoDatabase database)
    : MongoCatalogStore<Nacionalidad, NacionalidadRow>(database, "catalogo_nacionalidad", ["nombreNacionalidad"]),
        INacionalidadStore
{
    public Task<PageResult<NacionalidadRow>> GetPageAsync(NacionalidadQuery query) =>
        GetPageAsync(query.Search, query.Page);

    protected override NacionalidadRow Read(BsonDocument document) => new(
        ReadId(document),
        Text(document, "nombreNacionalidad") ?? "");

    protected override Guid GetId(Nacionalidad item) => item.Id;

    protected override BsonDocument Write(Nacionalidad item) => new()
    {
        ["nombreNacionalidad"] = TextValue(item.NombreNacionalidad)
    };
}
