using MongoDB.Bson;
using MongoDB.Driver;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Catalogs;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Infrastructure.Catalogs;

public sealed class MongoTipoCabelloStore(IMongoDatabase database)
    : MongoCatalogStore<TipoCabello, TipoCabelloRow>(database, "catalogo_tipo_cabello", ["nombreCabello", "largoCabello"]),
        ITipoCabelloStore
{
    public Task<PageResult<TipoCabelloRow>> GetPageAsync(TipoCabelloQuery query) =>
        GetPageAsync(query.Search, query.Page);

    protected override TipoCabelloRow Read(BsonDocument document) => new(
        ReadId(document),
        Text(document, "nombreCabello") ?? "",
        Text(document, "largoCabello") ?? "");

    protected override Guid GetId(TipoCabello item) => item.Id;

    protected override BsonDocument Write(TipoCabello item) => new()
    {
        ["nombreCabello"] = TextValue(item.NombreCabello),
        ["largoCabello"] = TextValue(item.LargoCabello)
    };
}
