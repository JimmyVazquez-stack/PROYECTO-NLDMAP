using MongoDB.Bson;
using MongoDB.Driver;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Catalogs;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Infrastructure.Catalogs;

public sealed class MongoOrientacionSexualStore(IMongoDatabase database)
    : MongoCatalogStore<OrientacionSexual, OrientacionSexualRow>(database, "catalogo_orientacion_sexual", ["nombreOrientacionSexual", "descripcionOrientacionSexual"]),
        IOrientacionSexualStore
{
    public Task<PageResult<OrientacionSexualRow>> GetPageAsync(OrientacionSexualQuery query) =>
        GetPageAsync(query.Search, query.Page);

    protected override OrientacionSexualRow Read(BsonDocument document) => new(
        ReadId(document),
        Text(document, "nombreOrientacionSexual") ?? "",
        Text(document, "descripcionOrientacionSexual"));

    protected override Guid GetId(OrientacionSexual item) => item.Id;

    protected override BsonDocument Write(OrientacionSexual item) => new()
    {
        ["nombreOrientacionSexual"] = TextValue(item.NombreOrientacionSexual),
        ["descripcionOrientacionSexual"] = TextValue(item.DescripcionOrientacionSexual)
    };
}
