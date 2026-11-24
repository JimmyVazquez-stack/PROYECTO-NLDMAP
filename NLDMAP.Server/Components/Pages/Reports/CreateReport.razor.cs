using Microsoft.AspNetCore.Components;
using MudBlazor;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Reports;
using NLDMAP.Domain.Catalogs;
using NLDMAP.Domain.Reports;

namespace NLDMAP.Server.Components.Pages.Reports;

public partial class CreateReport
{
    private const int ReviewStep = 4;
    [Inject] private ReportService Reports { get; set; } = default!;
    [Inject] private ILogger<CreateReport> Logger { get; set; } = default!;

    private readonly ReportInput input = new();
    private ReportCatalogOptions? options;
    private Reportante? reporter;
    private IReadOnlyList<AsentamientoPostal> postalMatches = [];
    private IReadOnlyList<ReportLocationMap.Pin> locationPins = [];
    private string? postalNotice;
    private string? folio;
    private string? reviewPhysicalTraits;
    private string? reviewVehicle;
    private ReportCatalogOption? vehicleColor;
    private string? error;
    private int activeStep;
    private bool loading;
    private bool saving;
    private bool searchingPostal;
    private bool hasAliases;
    private bool hasPersonalDetails;
    private bool hasDisabilities;
    private bool hasCircumstance;
    private bool hasIncidentDates;
    private bool hasAddress;
    private bool hasMapPoint;
    private bool hasReporterRelationship;
    private bool hasVehicle;
    private bool hasInvolvedPerson;

    private string ReviewName => JoinKnown(" ", input.Nombres, input.PrimerApellido, input.SegundoApellido, input.Apodos);
    private string ReviewLocation => JoinKnown(", ", input.Asentamiento, input.Municipio, input.Estado, input.CodigoPostal);

    protected override Task OnInitializedAsync() => LoadCatalogsAsync();

    private async Task LoadCatalogsAsync()
    {
        loading = true;
        error = null;
        try
        {
            (options, reporter) = await Reports.GetCreationFormAsync();
        }
        catch (AdministrationException exception)
        {
            error = exception.Message;
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "No se pudieron cargar los datos del formulario de reporte");
            error = "No se pudieron cargar los datos del formulario. Inténtalo nuevamente.";
        }
        finally
        {
            loading = false;
        }
    }

    private async Task BeforeStepAsync(StepperInteractionEventArgs action)
    {
        if (saving)
        {
            action.Cancel = true;
            return;
        }

        error = null;
        if (action.Action != StepAction.Complete)
            return;

        try
        {
            switch (action.StepIndex)
            {
                case 0:
                    ReportService.ValidatePerson(input);
                    reviewPhysicalTraits = ReportService.DescribePhysicalTraits(input, options!);
                    break;
                case 1: ReportService.ValidateFacts(input); break;
                case 2: ReportService.ValidateLocation(input); break;
                case 3:
                    ReportService.ValidateVehicle(input);
                    reviewVehicle = JoinKnown(" · ", input.MarcaVehiculo, input.ModeloVehiculo,
                        input.AnoVehiculo?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        vehicleColor?.Nombre, input.PlacaVehiculo);
                    if (reviewVehicle == "Sin datos")
                        reviewVehicle = null;
                    break;
                case ReviewStep:
                    saving = true;
                    StateHasChanged();
                    folio = await Reports.CreateAsync(input);
                    break;
            }
        }
        catch (AdministrationException exception)
        {
            error = exception.Message;
            action.Cancel = true;
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "No se pudo guardar el reporte");
            error = "No se pudo guardar el reporte. Puedes intentarlo nuevamente.";
            action.Cancel = true;
        }
        finally
        {
            saving = false;
        }
    }

    private Task<IEnumerable<ReportCatalogOption>> SearchVehicleColors(string? search, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var colors = options?.Colores ?? [];
        var term = search?.Trim();
        return Task.FromResult<IEnumerable<ReportCatalogOption>>(string.IsNullOrEmpty(term) ? colors :
            colors.Where(color => System.Globalization.CultureInfo.CurrentCulture.CompareInfo.IndexOf(
                color.Nombre, term, System.Globalization.CompareOptions.IgnoreCase | System.Globalization.CompareOptions.IgnoreNonSpace) >= 0));
    }

    private void SelectVehicleColor(ReportCatalogOption? color)
    {
        vehicleColor = color;
        input.ColorVehiculoId = color?.Id;
    }

    private string? DisabilityName(Guid id) =>
        options?.Discapacidades.FirstOrDefault(disability => disability.Id == id)?.Nombre;

    private void ClearPersonalDetails()
    {
        input.SexoId = null;
        input.OrientacionSexualId = null;
        input.NacionalidadId = null;
        input.OcupacionId = null;
    }

    private void ClearIncidentDates()
    {
        input.FechaHechos = null;
        input.HoraHechos = null;
        input.FechaDeteccion = null;
    }

    private void ClearAddress()
    {
        PostalCodeChanged(null);
        input.Estado = null;
        input.Municipio = null;
        input.Asentamiento = null;
        input.Calle = null;
        input.Referencias = null;
    }

    private void ClearVehicle()
    {
        input.MarcaVehiculo = null;
        input.ModeloVehiculo = null;
        input.AnoVehiculo = null;
        input.PlacaVehiculo = null;
        SelectVehicleColor(null);
    }

    private void ClearInvolvedPerson()
    {
        input.NombrePersonaRelacionada = null;
        input.ParentescoPersonaRelacionadaId = null;
        input.DescripcionPersonaRelacionada = null;
    }

    private void PostalCodeChanged(string? value)
    {
        input.CodigoPostal = value;
        postalMatches = [];
        postalNotice = null;
    }

    private async Task FindPostalAsync()
    {
        var code = input.CodigoPostal?.Trim();
        if (code is null || code.Length != 5 || !code.All(char.IsAsciiDigit))
        {
            postalNotice = "Escribe un código postal de cinco dígitos.";
            return;
        }

        searchingPostal = true;
        postalNotice = null;
        try
        {
            var matches = await Reports.FindPostalAsync(code);
            if (code != input.CodigoPostal?.Trim())
                return;

            postalMatches = matches;
            if (matches.Count == 0)
                postalNotice = "No hay resultados en SEPOMEX. Puedes escribir la ubicación manualmente.";
        }
        catch (Exception exception)
        {
            Logger.LogWarning(exception, "No se pudo consultar SEPOMEX para el reporte");
            postalNotice = "No se pudo consultar SEPOMEX. Puedes escribir la ubicación manualmente.";
        }
        finally
        {
            searchingPostal = false;
        }
    }

    private void SelectPostal(string? key)
    {
        if (postalMatches.FirstOrDefault(item => item.Clave == key) is not { } match)
            return;

        input.Estado = match.Estado;
        input.Municipio = match.Municipio;
        input.Asentamiento = match.Asentamiento;
    }

    private void SelectCoordinates(CoordenadasReporte point) => SetCoordinates(point.Latitud, point.Longitud);
    private void SetLatitude(double? value) => SetCoordinates(value, input.Longitud);
    private void SetLongitude(double? value) => SetCoordinates(input.Latitud, value);
    private void ClearCoordinates() => SetCoordinates(null, null);

    private void SetCoordinates(double? latitude, double? longitude)
    {
        input.Latitud = latitude;
        input.Longitud = longitude;
        locationPins = latitude is { } lat && longitude is { } lng && CoordenadasReporte.EsValida(lat, lng)
            ? [new ReportLocationMap.Pin(lat, lng)]
            : [];
    }

    private static string JoinKnown(string separator, params string?[] parts)
    {
        var text = string.Join(separator, parts.Where(part => !string.IsNullOrWhiteSpace(part)));
        return text.Length == 0 ? "Sin datos" : text;
    }
}
