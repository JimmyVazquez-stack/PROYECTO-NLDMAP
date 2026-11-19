namespace NLDMAP.Server.Components.Pages.Catalogs;

public partial class CatalogsList
{
    private static readonly CatalogEntry[] Catalogos =
    [
        new(
            Nombre: "Tez",
            Descripcion: "Tonos de piel para la descripción física del reporte.",
            Ruta: "/admin/catalogos/tez",
            Disponible: true),

        new(
            Nombre: "Regiones corporales",
            Descripcion: "Zonas del cuerpo que se usan al registrar señas particulares.",
            Ruta: "/admin/catalogos/regiones-corporales",
            Disponible: true),
        new(
        Nombre: "Tipos de seña",
        Descripcion: "Clases de señas particulares, como cicatrices o tatuajes.",
        Ruta: "/admin/catalogos/tipos-sena",
        Disponible: true)
    ];
    private sealed record CatalogEntry(
        string Nombre,
        string Descripcion,
        string Ruta,
        bool Disponible);
}