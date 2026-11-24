namespace NLDMAP.Server.Components.Pages.Catalogs;

public partial class CatalogsList
{
    private static readonly CatalogEntry[] Catalogos =
    [
        new("Tez", "/admin/catalogos/tez", "Tonos de piel de la escala Monk."),
        new("Regiones corporales", "/admin/catalogos/regiones-corporales",
            "Zonas del cuerpo para registrar señas particulares."),
        new("Tipos de seña", "/admin/catalogos/tipos-sena", "Cicatrices, tatuajes y otras señas particulares."),
        new("Códigos postales (SEPOMEX)", "/admin/catalogos/sepomex",
            "Consulta y carga oficial de códigos postales, municipios y asentamientos."),
        .. CatalogoViews.Todos.Select(catalogo => new CatalogEntry(
            catalogo.Nombre, $"/admin/catalogos/{catalogo.Ruta}", catalogo.Descripcion))
    ];

    private sealed record CatalogEntry(string Nombre, string Ruta, string? Descripcion = null);
}
