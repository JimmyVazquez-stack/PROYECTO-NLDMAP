using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NLDMAP.Application.Catalogs;

namespace NLDMAP.Infrastructure.Catalogs;

// La representación BSON pertenece a Infrastructure; Domain usa tipos de .NET.
[BsonIgnoreExtraElements]
internal sealed class TezDocument
{
    [BsonId, BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }
    [BsonElement("colorPiel")]
    public string ColorPiel { get; set; } = "";
    [BsonElement("colorHex")] 
    public string ColorHex { get; set; } = "";
    [BsonElement("orden")]
    public int Orden { get; set; }
    [BsonElement("version")]
    public string Version { get; set; } = "";
    [BsonElement("creadoEnUtc")]
    public DateTime CreadoEnUtc { get; set; }
    [BsonElement("creadoPor")]
    public string CreadoPor { get; set; } = "";
    [BsonElement("actualizadoEnUtc")]
    public DateTime ActualizadoEnUtc { get; set; }
    [BsonElement("actualizadoPor")]
    public string ActualizadoPor { get; set; } = "";

    public TezRow ToRow() => new(Id, ColorPiel, ColorHex, Orden, Version);
}
