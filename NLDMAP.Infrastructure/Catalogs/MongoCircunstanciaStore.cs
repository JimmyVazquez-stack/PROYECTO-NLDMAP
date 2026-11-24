using MongoDB.Bson;
using MongoDB.Driver;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Catalogs;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Infrastructure.Catalogs;

public sealed class MongoCircunstanciaStore(IMongoDatabase database)
    : MongoCatalogStore<Circunstancia, CircunstanciaRow>(database, "catalogo_circunstancia", ["tipo", "descripcionCircunstancia"]),
        ICircunstanciaStore
{
    public Task<PageResult<CircunstanciaRow>> GetPageAsync(CircunstanciaQuery query) =>
        GetPageAsync(query.Search, query.Page);

    protected override CircunstanciaRow Read(BsonDocument document) => new(
        ReadId(document),
        Text(document, "tipo") ?? "",
        Text(document, "descripcionCircunstancia"));

    protected override Guid GetId(Circunstancia item) => item.Id;

    protected override BsonDocument Write(Circunstancia item) => new()
    {
        ["tipo"] = TextValue(item.Tipo),
        ["descripcionCircunstancia"] = TextValue(item.DescripcionCircunstancia)
    };
}
