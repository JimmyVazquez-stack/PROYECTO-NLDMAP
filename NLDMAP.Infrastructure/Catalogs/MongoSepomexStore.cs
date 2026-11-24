using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Catalogs;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Infrastructure.Catalogs;

public sealed class MongoSepomexStore(IMongoDatabase database) : ISepomexStore
{
    private const int PageSize = 20;
    private readonly IMongoCollection<BsonDocument> collection =
        database.GetCollection<BsonDocument>("catalogo_sepomex");

    public async Task<PageResult<AsentamientoPostal>> GetPageAsync(string search, int page)
    {
        var filters = Builders<BsonDocument>.Filter;
        var filter = filters.Empty;
        if (search.Length > 0)
        {
            var expression = new BsonRegularExpression(Regex.Escape(search), "i");
            filter = filters.Regex("codigoPostal", expression) | filters.Regex("asentamiento", expression) |
                filters.Regex("municipio", expression) | filters.Regex("estado", expression) |
                filters.Regex("ciudad", expression);
        }

        var total = checked((int)await collection.CountDocumentsAsync(filter));
        page = Math.Clamp(page, 1, Math.Max(1, (total + PageSize - 1) / PageSize));
        var sort = Builders<BsonDocument>.Sort.Ascending("_id");
        var documents = await collection.Find(filter).Sort(sort)
            .Skip((page - 1) * PageSize).Limit(PageSize).ToListAsync();
        return new PageResult<AsentamientoPostal>(documents.Select(Leer).ToArray(), total, page, PageSize);
    }

    public async Task<ResultadoImportacionPostal> ImportAsync(IReadOnlyList<AsentamientoPostal> rows, string actorId)
    {
        var now = DateTime.UtcNow;
        var added = 0;
        var updated = 0;
        foreach (var batch in rows.Chunk(1000))
        {
            var writes = batch.Select(row =>
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", row.Clave);
                var update = Builders<BsonDocument>.Update
                    .SetOnInsert("creadoEnUtc", now)
                    .SetOnInsert("creadoPor", actorId)
                    .Set("codigoPostal", row.CodigoPostal)
                    .Set("asentamiento", row.Asentamiento)
                    .Set("tipoAsentamiento", row.TipoAsentamiento)
                    .Set("municipio", row.Municipio)
                    .Set("estado", row.Estado)
                    .Set("ciudad", row.Ciudad)
                    .Set("claveEstado", row.ClaveEstado)
                    .Set("claveMunicipio", row.ClaveMunicipio)
                    .Set("claveAsentamiento", row.ClaveAsentamiento)
                    .Set("zona", row.Zona)
                    .Set("codigoPostalAdministracion", row.CodigoPostalAdministracion)
                    .Set("claveOficina", row.ClaveOficina)
                    .Set("claveTipoAsentamiento", row.ClaveTipoAsentamiento)
                    .Set("claveCiudad", row.ClaveCiudad);
                return new UpdateOneModel<BsonDocument>(filter, update) { IsUpsert = true };
            });
            var result = await collection.BulkWriteAsync(writes, new BulkWriteOptions { IsOrdered = false });
            added += result.Upserts.Count;
            updated += checked((int)result.ModifiedCount);
        }

        return new ResultadoImportacionPostal(rows.Count, added, updated);
    }

    private static AsentamientoPostal Leer(BsonDocument row) => new(
        row["codigoPostal"].AsString, row["asentamiento"].AsString, row["tipoAsentamiento"].AsString,
        row["municipio"].AsString, row["estado"].AsString, row["ciudad"].AsString,
        row["claveEstado"].AsString, row["claveMunicipio"].AsString, row["claveAsentamiento"].AsString,
        row["zona"].AsString, row["codigoPostalAdministracion"].AsString, row["claveOficina"].AsString,
        row["claveTipoAsentamiento"].AsString, row["claveCiudad"].AsString);
}
