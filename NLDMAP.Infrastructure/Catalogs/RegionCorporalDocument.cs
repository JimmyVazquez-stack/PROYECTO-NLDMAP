using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NLDMAP.Application.Catalogs;

namespace NLDMAP.Infrastructure.Catalogs;

[BsonIgnoreExtraElements]
internal sealed class RegionCorporalDocument
{
    [BsonId, BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    [BsonElement("region")] 
    public string Region { get; set; } = "";
    [BsonElement("orden")] 
    public int Orden { get; set; }
    [BsonElement("version")] 
    public string Version { get; set; } = "";
    [BsonElement("actualizadoEnUtc")] 
    public DateTime ActualizadoEnUtc { get; set; }
    [BsonElement("actualizadoPor")] 
    public string ActualizadoPor { get; set; } =  "";
    
    public RegionCorporalRow ToRow() => new RegionCorporalRow(Id, Region, Orden, Version);
}