using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Neo4j.Driver;

namespace NLDMAP.Infrastructure.Graph;

public sealed class ReportGraphSync(
    IMongoDatabase mongoDatabase,
    IDriver graphDriver,
    IConfiguration configuration)
{
    private const string SyncQuery = """
        UNWIND $reports AS row
        MERGE (r:Reporte {idReporte: row.idReporte})
        SET r.folioInterno = row.folioInterno

        WITH r, row
        OPTIONAL MATCH (r)-[anterior:SE_CLASIFICA_SEGUN]->(c:Circunstancia)
        WHERE row.tipoCircunstancia IS NULL OR c.tipo IS NULL OR c.tipo <> row.tipoCircunstancia
        DELETE anterior

        WITH DISTINCT r, row
        FOREACH (_ IN CASE WHEN row.tipoCircunstancia IS NULL THEN [] ELSE [1] END |
            MERGE (actual:Circunstancia {tipo: row.tipoCircunstancia})
            MERGE (r)-[:SE_CLASIFICA_SEGUN]->(actual)
        )

        RETURN count(*) AS procesados
        """;

    public async Task<int> RunAsync()
    {
        var reports = mongoDatabase.GetCollection<BsonDocument>("reportes");
        var projection = Builders<BsonDocument>.Projection
            .Include("folioInterno")
            .Include("catalogos.circunstancia")
            .Include("Hechos.Circunstancia");
        var documents = await reports.Find(FilterDefinition<BsonDocument>.Empty)
            .Project<BsonDocument>(projection).ToListAsync();
        if (documents.Count == 0)
            return 0;

        var rows = documents.Select(document =>
        {
            var id = ReadId(document["_id"]);
            var folio = document.GetValue("folioInterno", BsonNull.Value) is BsonString text
                ? text.Value : id;
            var type = ReadType(document, "catalogos", "circunstancia")
                ?? ReadType(document, "Hechos", "Circunstancia");

            return new Dictionary<string, object?>
            {
                ["idReporte"] = id,
                ["folioInterno"] = folio,
                ["tipoCircunstancia"] = type
            };
        }).ToArray();

        var database = configuration["Neo4j:Database"] ?? "neo4j";
        await using var session = graphDriver.AsyncSession(options => options.WithDatabase(database));
        return await session.ExecuteWriteAsync(async transaction =>
        {
            var cursor = await transaction.RunAsync(SyncQuery, new { reports = rows });
            var result = await cursor.SingleAsync();
            return checked((int)result["procesados"].As<long>());
        });
    }

    private static string? ReadType(BsonDocument document, string section, string field)
    {
        if (document.GetValue(section, BsonNull.Value) is BsonDocument value &&
            value.GetValue(field, BsonNull.Value) is BsonString type &&
            !string.IsNullOrWhiteSpace(type.Value))
            return type.Value.Trim();

        return null;
    }

    private static string ReadId(BsonValue value) => value switch
    {
        BsonBinaryData binary => binary.ToGuid(GuidRepresentation.Standard).ToString("D"),
        BsonString text => Guid.TryParse(text.Value, out var id) ? id.ToString("D") : text.Value,
        BsonObjectId id => id.Value.ToString(),
        _ => throw new InvalidOperationException($"Tipo de identificador no compatible: {value.BsonType}.")
    };
}
