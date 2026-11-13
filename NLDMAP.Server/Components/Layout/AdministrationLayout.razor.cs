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

        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "12px",
            DrawerWidthLeft = "264px"
        }
    };
}
