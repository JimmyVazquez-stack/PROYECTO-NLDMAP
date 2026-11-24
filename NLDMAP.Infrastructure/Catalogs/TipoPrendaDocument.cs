using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NLDMAP.Application.Catalogs;

namespace NLDMAP.Infrastructure.Catalogs;

[BsonIgnoreExtraElements]
internal sealed class TipoPrendaDocument
{
    [BsonId, BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set;  }

    [BsonElement("nombre")]
    public string Nombre { get; set; } = "";


    [BsonElement("creadoEnUtc")]
    public DateTime CreadoEnUtc { get; set; }

    [BsonElement("creadoPor")]
    public string CreadoPor { get; set; } = "";

    [BsonElement("actualizadoEnUtc")]
    public DateTime ActualizadoEnUtc { get; set; }

    [BsonElement("actualizadoPor")]
    public string ActualizadoPor { get; set; } = "";

    public TipoPrendaRow ToRow() => new(Id, Nombre);

}