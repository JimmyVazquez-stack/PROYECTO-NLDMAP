namespace NLDMAP.Domain.ValueObjects;

public class ClothingDescription
{
    public string Top { get; set; } = string.Empty; //playera, chamarra..
    public string Bottom { get; set; } = string.Empty; //pantalon, falda
    public string Footwear { get; set; } = string.Empty; //zapatos, sandalias
    
    //constructor interno sin parametros para driver MongoDB
    //deserializacion JSON
    internal ClothingDescription()
    {
        Top = string.Empty;
        Bottom = string.Empty;
        Footwear = string.Empty;
    }
    
    public ClothingDescription(string top, string bottom, string footwear)
    {
        Top = top;
        Bottom = bottom;
        Footwear = footwear;
    }
} 