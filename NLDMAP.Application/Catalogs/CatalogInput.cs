namespace NLDMAP.Application.Catalogs;

internal static class CatalogInput
{
    internal static string Required(string? value, string label, int limit) =>
        Optional(value, label, limit) ?? throw new ArgumentException($"Completa el campo «{label}».");

    internal static string? Optional(string? value, string label, int limit)
    {
        var text = value?.Trim();
        if (text?.Length > limit)
            throw new ArgumentException($"El campo «{label}» admite hasta {limit} caracteres.");

        return string.IsNullOrEmpty(text) ? null : text;
    }
}
