using System.Text.RegularExpressions;

namespace NLDMAP.Domain.Catalogs;

public sealed class Tez
{
    public const int ColorPielMaxLength = 80;

    public Guid Id { get; }
    public string ColorPiel { get; }
    public string ColorHex { get; }
    public int Orden { get; }

    public Tez(Guid id, string colorPiel, string colorHex, int orden)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador de la tez es obligatorio.");
        
        colorPiel = (colorPiel ?? "").Trim();
        colorHex = (colorHex ?? "").Trim().ToUpperInvariant();
        
        if (colorPiel.Length is 0 or > ColorPielMaxLength)
            throw new ArgumentException("El color de piel debe tener de 1 a 80 caracteres.");
        if (!Regex.IsMatch(colorHex, @"\A#[0-9A-F]{6}\Z"))
            throw new ArgumentException("El color debe tener el formato #RRGGBB");
        if (orden is < 0 or > 9999)
            throw new ArgumentException("El orden debe estar entre 0 y 9999.");

        Id = id;
        ColorPiel = colorPiel;
        ColorHex = colorHex;
        Orden = orden;
    }
    
}