using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NLDMAP.Application.Catalogs;

namespace NLDMAP.Infrastructure.Catalogs;

[BsonIgnoreExtraElements]
internal sealed class TipoSenaDocument
{
    [BsonId, BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonElement("nombre")]
    public string Nombre { get; set; } = "";
    [BsonElement("actualizadoEnUtc")]
    public DateTime ActualizadoEnUtc { get; set; }
    [BsonElement("actualizadoPor")]
    public string ActualizadoPor { get; set; } =  "";

    public TipoSenaRow ToRow() => new TipoSenaRow(Id, Nombre);
}