using MudBlazor;

namespace NLDMAP.Server.Components.Layout;

public partial class AdministrationLayout
{
    private bool _drawerOpen = false;
    
    private void ToggleDrawer()
    {
        _drawerOpen = !_drawerOpen;
    }

    private readonly MudTheme _theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#FFFFFF",
            Secondary = "#FFFFFF",

            Background = "#000000",
            Surface = "#FFFFFF",

            AppbarBackground = "#000000",
            AppbarText = "#000000",

            DrawerBackground = "#FFFFFF",
            DrawerText = "#000000",
            DrawerIcon = "#000000",

            TextPrimary = "#000000",
            TextSecondary = "#000000",

            Divider = "#E0E0E0"
        },

        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "12px",
            DrawerWidthLeft = "264px"
        }
    };
}
