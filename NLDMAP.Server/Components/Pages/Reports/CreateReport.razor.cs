namespace NLDMAP.Server.Components.Pages.Reports;


public partial class CreateReport
{
    private sealed record CatalogOption(string Id, string Label);

    private static readonly CatalogOption[] GenderOptions =
    [
        new("gender-1", "Femenino · Orientación no especificada"),
        new("gender-2", "Masculino · Orientación no especificada"),
        new("gender-3", "Sexo y orientación no especificados")
    ];

    private static readonly CatalogOption[] NationalityOptions =
    [
        new("nationality-1", "Mexicana"),
        new("nationality-2", "Guatemalteca"),
        new("nationality-3", "Colombiana")
    ];

    private static readonly CatalogOption[] OccupationOptions =
    [
        new("occupation-1", "Estudiante"),
        new("occupation-2", "Comerciante"),
        new("occupation-3", "Docente")
    ];

    private static readonly CatalogOption[] ComplexionOptions =
    [
        new("complexion-1", "Clara"),
        new("complexion-2", "Morena clara"),
        new("complexion-3", "Morena oscura")
    ];

    private static readonly CatalogOption[] BodyRegionOptions =
    [
        new("region-1", "Cabeza"),
        new("region-2", "Cuello"),
        new("region-3", "Brazo derecho"),
        new("region-4", "Brazo izquierdo"),
        new("region-5", "Espalda")
    ];

    private static readonly CatalogOption[] MarkTypeOptions =
    [
        new("mark-1", "Cicatriz"),
        new("mark-2", "Tatuaje"),
        new("mark-3", "Lunar"),
        new("mark-4", "Mancha")
    ];

    // Ejemplos visuales, no un catálogo institucional validado.
    private static readonly CatalogOption[] CircumstanceOptions =
    [
        new("circumstance-1", "Se perdió contacto durante un traslado"),
        new("circumstance-2", "No regresó a su domicilio"),
        new("circumstance-3", "Circunstancia desconocida")
    ];

    private static readonly CatalogOption[] MunicipalityOptions =
    [
        new("municipality-1", "Tlaxcala"),
        new("municipality-2", "Apizaco"),
        new("municipality-3", "Huamantla"),
        new("municipality-4", "Chiautempan")
    ];
}