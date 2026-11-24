namespace NLDMAP.Domain.Catalogs;

public sealed class Tez
{
    // Monk Skin Tone Scale, Ellis Monk y Google, CC BY 4.0. https://skintone.google/
    public static IReadOnlyDictionary<string, string> TonosMonk { get; } =
        new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Monk 01"] = "#F6EDE4",
                ["Monk 02"] = "#F3E7DB",
                ["Monk 03"] = "#F7EAD0",
                ["Monk 04"] = "#EADABA",
                ["Monk 05"] = "#D7BD96",
                ["Monk 06"] = "#A07E56",
                ["Monk 07"] = "#825C43",
                ["Monk 08"] = "#604134",
                ["Monk 09"] = "#3A312A",
                ["Monk 10"] = "#292420"
            });

    public Guid Id { get; }
    public string NombreColor { get; }
    public string ColorHex { get; }

    public Tez(Guid id, string nombreColor)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador de la tez es obligatorio.", nameof(id));

        nombreColor = CatalogoValores.TextoObligatorio(nombreColor, nameof(nombreColor));
        if (!TonosMonk.TryGetValue(nombreColor, out var colorHex))
            throw new ArgumentException("Selecciona un tono entre Monk 01 y Monk 10.", nameof(nombreColor));

        Id = id;
        NombreColor = "Monk " + nombreColor[5..];
        ColorHex = colorHex;
    }
}
