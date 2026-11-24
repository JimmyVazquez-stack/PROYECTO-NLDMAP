using System.Text.RegularExpressions;

namespace NLDMAP.Domain.Catalogs;

internal static class CatalogoValores
{
    internal static string TextoObligatorio(string? valor, string parametro)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("El valor es obligatorio.", parametro);

        return valor.Trim();
    }

    internal static string ColorHex(string? valor)
    {
        var colorHex = (valor ?? "").Trim().ToUpperInvariant();
        if (!Regex.IsMatch(colorHex, @"\A#[0-9A-F]{6}\z"))
            throw new ArgumentException("El color debe tener el formato #RRGGBB.", nameof(colorHex));

        return colorHex;
    }
}
