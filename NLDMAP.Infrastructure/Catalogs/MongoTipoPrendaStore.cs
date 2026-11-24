using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Catalogs;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Infrastructure.Catalogs;

public sealed class MongoTipoPrendaStore : ITipoPrendaStore
{
    private const int PageSize = 20;
    private readonly IMongoCollection<TipoPrendaDocument> _collection;

    public MongoTipoPrendaStore(IMongoDatabase database)
        => _collection = database.GetCollection<TipoPrendaDocument>("catalogo_tipo_prenda");

    public async Task<PageResult<TipoPrendaRow>> GetPageAsync(TipoPrendaQuery query)
    {
        var filters = Builders<TipoPrendaDocument>.Filter;
        var filter = filters.Empty;
        var search = (query.Search ?? "").Trim();

        if (search.Length > 120)
            search = search[..120];

        if (search.Length > 0)
        {
            var expression = new BsonRegularExpression(Regex.Escape(search), "i");
            filter = filters.Regex(document => document.Nombre, expression);
        }
        var total = checked((int) await _collection.CountDocumentsAsync(filter));
        var totalPages = Math.Max(1, (int)Math.Ceiling((double)total / PageSize));
        var page = Math.Clamp(query.Page, 1, totalPages);

        var documents = await _collection.Find(filter)
            .SortBy(document => document.Nombre)
            .ThenBy(document => document.Id)
            .Skip((page - 1) * PageSize)
            .Limit(PageSize)
            .ToListAsync();
        var rows = documents.Select(document => document.ToRow()).ToArray();
        return new PageResult<TipoPrendaRow>(rows, total, page, PageSize);
    }

    public async Task<TipoPrendaRow?> GetAsync(Guid id)
    {
        var document = await _collection.Find(item => item.Id == id)
            .FirstOrDefaultAsync();

        return document?.ToRow();
    }

    public Task CreateAsync(TipoVestimenta tipoPrenda, string actorId)
    {
        var now = DateTime.UtcNow;
        var document = new TipoPrendaDocument
        {
            Id = tipoPrenda.Id,
            Nombre = tipoPrenda.NombreVestimenta,
            CreadoEnUtc = now,
            CreadoPor = actorId,
            ActualizadoEnUtc = now,
            ActualizadoPor = actorId
        };

        return _collection.InsertOneAsync(document);
    }

    public async Task UpdateAsync(
        TipoVestimenta tipoPrenda, string actorId)
    {
        var filter = Builders<TipoPrendaDocument>.Filter.Where(document =>
            document.Id == tipoPrenda.Id);

        var update = Builders<TipoPrendaDocument>.Update
            .Set(document => document.Nombre, tipoPrenda.NombreVestimenta)
            .Set(document => document.ActualizadoEnUtc, DateTime.UtcNow)
            .Set(document => document.ActualizadoPor, actorId)
            .Unset("orden")
            .Unset("version");

        var result = await _collection.UpdateOneAsync(filter, update);

        if (result.MatchedCount == 0)
        {
            throw new AdministrationException(
                "La prenda ya no existe. Actualiza el catálogo y vuelve a intentarlo.");
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var result = await _collection.DeleteOneAsync(document => document.Id == id);
        if (result.DeletedCount == 0)
            throw new AdministrationException("La opción ya no existe. Actualiza el catálogo.");
    }
}
