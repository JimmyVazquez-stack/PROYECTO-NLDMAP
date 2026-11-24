using MongoDB.Bson;
using MongoDB.Driver;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Catalogs;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Infrastructure.Catalogs;

public sealed class MongoOcupacionStore(IMongoDatabase database)
    : MongoCatalogStore<Ocupacion, OcupacionRow>(database, "catalogo_ocupacion", ["nombreOcupacion", "descripcionOcupacion"]),
        IOcupacionStore
{
    public Task<PageResult<OcupacionRow>> GetPageAsync(OcupacionQuery query) =>
        GetPageAsync(query.Search, query.Page);

    protected override OcupacionRow Read(BsonDocument document) => new(
        ReadId(document),
        Text(document, "nombreOcupacion") ?? "",
        Text(document, "descripcionOcupacion"));

    protected override Guid GetId(Ocupacion item) => item.Id;

    protected override BsonDocument Write(Ocupacion item) => new()
    {
        ["nombreOcupacion"] = TextValue(item.NombreOcupacion),
        ["descripcionOcupacion"] = TextValue(item.DescripcionOcupacion)
    };
}
