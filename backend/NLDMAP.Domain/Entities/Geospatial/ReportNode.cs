namespace NLDMAP.Domain.Entities.Geospatial
{
    //Nodo tipo 1: Reporte en el espacio
    public class ReportNode
    {
        public Guid ReportId { get; set; }
        public DateTime DisappearanceDateTime { get; set; }
        public string Status { get; set; } = "Active";
    }
    
    //Nodo tipo 2: Ubicacion geografica
    public class LocationNode
    {
        public Guid LocationId { get; set; }
        public string Municipality { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        
        //coordenadas espaciales para plugin APOC
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
    
    //Constantes de las relaciones "Aristas" que unen a los nodos
    public static class GraphRelationships
    {
        public const string LAST_SEEN_AT = "LAST_SEEN_AT"; //Relacion entre el reporte a la ubicacion 
        public const string POSSIBLE_SIGHTSING = "POSSIBLE_SIGHTSING"; //De un usuario a una ubicacion
    }
}
