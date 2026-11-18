using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using NLDMAP.Server.Theming;

namespace NLDMAP.Server.Components.Layout;

public partial class AdministrationLayout
{
    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    private bool _drawerOpen = true;

    // Tema por defecto si todavía no existe preferencia guardada
    private bool _isDarkMode = true;

    private bool _themeLoaded;

    private readonly MudTheme _theme = NldmapTheme.Create();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        try
        {
            var savedTheme =
                await JS.InvokeAsync<bool?>("nldmapTheme.get");

            if (savedTheme.HasValue)
            {
                _isDarkMode = savedTheme.Value;
            }
        }
        catch (JSException)
        {
            // Si localStorage no está disponible,
            // se conserva el tema por defecto.
        }

        _themeLoaded = true;

        await InvokeAsync(StateHasChanged);
    }

    private void ToggleDrawer()
    {
        _drawerOpen = !_drawerOpen;
    }

    private async Task ToggleTheme()
    {
        _isDarkMode = !_isDarkMode;

        if (!_themeLoaded)
            return;

        try
        {
            await JS.InvokeVoidAsync(
                "nldmapTheme.set",
                _isDarkMode
            );
        }
        catch (JSException)
        {
            // El tema sigue funcionando en la pestaña actual.
        }
    }
}