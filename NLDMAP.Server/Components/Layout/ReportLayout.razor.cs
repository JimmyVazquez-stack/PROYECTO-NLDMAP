using MudBlazor;

namespace NLDMAP.Server.Components.Layout;

public partial class ReportLayout
{
    private bool _drawerOpen = true;

    private void ToggleDrawer()
    {
        _drawerOpen = !_drawerOpen;
    }

    private readonly MudTheme _theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#173D3A",
            Secondary = "#52675E",
            Background = "#F4F7F6",
            Surface = "#FFFFFF",
            AppbarBackground = "#FFFFFF",
            AppbarText = "#173D3A",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#173D3A",
            DrawerIcon = "#52675E",
            TextPrimary = "#173D3A",
            TextSecondary = "#52675E",
            Divider = "#DCE5E0"
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "12px",
            DrawerWidthLeft = "240px"
        }
    };
}
