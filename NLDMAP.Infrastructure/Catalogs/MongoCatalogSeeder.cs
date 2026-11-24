using System.Security.Cryptography;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;

namespace NLDMAP.Infrastructure.Catalogs;

public sealed class MongoCatalogSeeder(IMongoDatabase database)
{
    public async Task<int> SeedAsync()
    {
        var inserted = await SeedCatalogAsync("catalogo_complexion", "nombreComplexion",
            ["Delgada", "Media", "Robusta"]);
        inserted += await SeedCatalogAsync("catalogo_discapacidad", "nombreDiscapacidad",
            ["Física o motriz", "Visual", "Auditiva", "Intelectual", "Psicosocial", "Múltiple", "Otra"]);
        inserted += await SeedCatalogAsync("catalogo_parentesco", "nombreParentesco",
            ["Madre", "Padre", "Hija", "Hijo", "Hermana", "Hermano", "Abuela", "Abuelo", "Nieta", "Nieto",
                "Tía", "Tío", "Sobrina", "Sobrino", "Prima", "Primo", "Pareja", "Cónyuge", "Amistad",
                "Persona conocida", "Sin vínculo", "Otro vínculo"]);
        return inserted;
    }

    private async Task<int> SeedCatalogAsync(string collectionName, string nameField, string[] names)
    {
        var filters = Builders<BsonDocument>.Filter;
        var collection = database.GetCollection<BsonDocument>(collectionName);
        var inserted = 0;

        foreach (var name in names)
        {
            var existingName = filters.Eq(nameField, name);
            if (await collection.Find(existingName, new FindOptions { Collation = new Collation("es", strength: CollationStrength.Secondary) })
                .Limit(1).AnyAsync())
                continue;

            // La misma opción usa el mismo identificador en cargas concurrentes o repetidas.
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{collectionName}/{name}"));
            var id = new BsonBinaryData(new Guid(hash.AsSpan(0, 16)), GuidRepresentation.Standard);
            var now = DateTime.UtcNow;
            var insert = Builders<BsonDocument>.Update
                .SetOnInsert(nameField, name)
                .SetOnInsert("creadoEnUtc", now)
                .SetOnInsert("actualizadoEnUtc", now)
                .SetOnInsert("creadoPor", "SeedCatalogs")
                .SetOnInsert("actualizadoPor", "SeedCatalogs");
            var result = await collection.UpdateOneAsync(filters.Eq("_id", id), insert, new UpdateOptions { IsUpsert = true });
            if (result.UpsertedId is not null)
                inserted++;
        }

        return inserted;
    }
}
