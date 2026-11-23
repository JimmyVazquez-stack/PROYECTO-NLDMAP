using MudBlazor;

namespace NLDMAP.Server.Components.Pages.Reports;

public partial class CreateReport
{
    private readonly CreateReportForm _form = new();
    private MudStepper? _stepper;
    private int _activeStep;

    private async Task GoToNextStepAsync()
    {
        if (_stepper is not null && _activeStep < 5)
            await _stepper.NextStepAsync();
    }

    private async Task GoToPreviousStepAsync()
    {
        if (_stepper is not null && _activeStep > 0)
            await _stepper.PreviousStepAsync();
    }

    private void ChangeAgeInformationMode(string? mode)
    {
        _form.AgeInformationMode = mode;

        if (mode != "birth-date")
            _form.BirthDate = null;

        if (mode != "approximate-age")
            _form.ApproximateAgeYears = null;
    }

    // Modelo de captura de la interfaz. No representa una tabla de base de datos.
    private sealed class CreateReportForm
    {
        // Persona reportada
        public string? FirstNames { get; set; }
        public string? PaternalSurname { get; set; }
        public string? MaternalSurname { get; set; }
        public string? IdentifyingDescription { get; set; }
        public string? AgeInformationMode { get; set; }
        public DateTime? BirthDate { get; set; }
        public int? ApproximateAgeYears { get; set; }
        public string? Curp { get; set; }
        public string? Nicknames { get; set; }
        public string? GenderId { get; set; }
        public string? NationalityId { get; set; }
        public string? OccupationId { get; set; }

        // Características y señas. ComplexionId corresponde a Tez en el mock actual.
        public string? ComplexionId { get; set; }
        public decimal? HeightCm { get; set; }
        public decimal? WeightKg { get; set; }
        public string? HairDescription { get; set; }
        public string? ClothingDescription { get; set; }
        public string? FootwearDescription { get; set; }
        public string? BodyRegionId { get; set; }
        public string? MarkTypeId { get; set; }
        public string? MarkDescription { get; set; }

        // Hechos
        public string? CircumstanceId { get; set; }
        public DateTime? EventDate { get; set; }
        public TimeSpan? EventTime { get; set; }
        public DateTime? AwarenessDate { get; set; }
        public string? FactsDescription { get; set; }

        // Ubicación
        public string? PostalCode { get; set; }
        public string? StateId { get; set; }
        public string? MunicipalityId { get; set; }
        public string? SettlementId { get; set; }
        public string? Street { get; set; }
        public string? LocationReferences { get; set; }

        // Información adicional
        public string? VehicleBrand { get; set; }
        public string? VehicleModel { get; set; }
        public string? VehicleColor { get; set; }
        public string? VehiclePlate { get; set; }
        public string? InvolvedPersonName { get; set; }
        public string? InvolvedPersonDescription { get; set; }

        // Decisiones del usuario
        public bool? AuthorizePublication { get; set; }
        public bool? KeepIdentityPrivate { get; set; }
    }

    private sealed record CatalogOption(string Id, string Label);

    // Catálogos de demostración conservados del código proporcionado.
    private static readonly CatalogOption[] GenderOptions =
    [
        new("gender-1", "Femenino · Orientación no especificada"),
        new("gender-2", "Masculino · Orientación no especificada"),
        new("gender-3", "Sexo y orientación no especificados")
    ];

    private static readonly CatalogOption[] NationalityOptions =
    [
        new("nationality-1", "Mexicana"),
        new("nationality-2", "Extranjera")
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
