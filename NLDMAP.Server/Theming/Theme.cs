
using MudBlazor;

namespace NLDMAP.Server.Theming;

public static class NldmapTheme
{
    public static MudTheme CreatePublic()
    {
        var theme = new MudTheme
        {
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "12px"
            },
            PaletteLight = new PaletteLight
            {
                Primary = "#155E59",
                PrimaryContrastText = "#FFFFFF",
                Secondary = "#496B60",
                SecondaryContrastText = "#FFFFFF",
                Background = "#F6F1E6",
                BackgroundGray = "#E7EFE9",
                Surface = "#FFFFFF",
                TextPrimary = "#202E2B",
                TextSecondary = "#475A54",
                AppbarBackground = "#155E59",
                AppbarText = "#FFFFFF",
                Divider = "#D1DDD6",
                LinesDefault = "#D1DDD6"
            }
        };

        theme.Typography.Default.FontFamily = ["system-ui", "-apple-system", "Segoe UI", "sans-serif"];
        return theme;
    }

    public static MudTheme Create()
    {
        var theme = new MudTheme
        {
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "10px",
                DrawerWidthLeft = "264px"
            }
        };

        // ========================================
        // MODO CLARO
        // ========================================

        var light = theme.PaletteLight;

        // Barra superior
        light.AppbarBackground = Colors.BlueGray.Darken3;
        light.AppbarText = Colors.Shades.White;

        // Menú lateral
        light.DrawerBackground = Colors.BlueGray.Darken3;
        light.DrawerText = Colors.Shades.White;
        light.DrawerIcon = Colors.BlueGray.Lighten3;

        // Color principal
        light.Primary = Colors.BlueGray.Lighten2;
        light.PrimaryContrastText = Colors.BlueGray.Darken4;

        // Color secundario: acciones complementarias
        light.Secondary = Colors.Blue.Lighten2;
        light.SecondaryContrastText = Colors.Shades.Black;

        // Fondo de páginas y superficies
        light.Background = Colors.Gray.Lighten4;
        light.Surface = Colors.Shades.White;

        // Textos
        light.TextPrimary = Colors.BlueGray.Darken4;
        light.TextSecondary = Colors.BlueGray.Darken1;

        // Bordes y separadores
        light.Divider = Colors.BlueGray.Lighten2;
        light.LinesDefault = Colors.BlueGray.Lighten2;

        // Colores de estado
        light.Success = Colors.Green.Darken2;
        light.Warning = Colors.Orange.Darken2;
        light.Error = Colors.Red.Darken2;
        light.Info = Colors.Blue.Darken2;

        // ========================================
        // MODO OSCURO
        // ========================================

        var dark = theme.PaletteDark;

        // Barra superior
        dark.AppbarBackground = Colors.BlueGray.Darken4;
        dark.AppbarText = Colors.Shades.White;

        // Menú lateral
        dark.DrawerBackground = Colors.BlueGray.Darken3;
        dark.DrawerText = Colors.Shades.White;
        dark.DrawerIcon = Colors.BlueGray.Lighten3;

        // Color principal
        dark.Primary = Colors.BlueGray.Lighten1;
        dark.PrimaryContrastText = Colors.BlueGray.Darken4;

        // Color secundario
        dark.Secondary = Colors.Blue.Lighten2;
        dark.SecondaryContrastText = Colors.Shades.Black;

        // Fondo de páginas y superficies
        dark.Background = Colors.BlueGray.Darken4;
        dark.Surface = Colors.Gray.Darken3;

        // Textos
        dark.TextPrimary = Colors.Shades.White;
        dark.TextSecondary = Colors.BlueGray.Lighten3;

        // Bordes y separadores
        dark.Divider = Colors.BlueGray.Darken1;
        dark.LinesDefault = Colors.BlueGray.Darken1;

        // Colores de estado
        dark.Success = Colors.Green.Lighten2;
        dark.Warning = Colors.Orange.Lighten2;
        dark.Error = Colors.Red.Lighten2;
        dark.Info = Colors.Blue.Lighten2;

        return theme;
    }
}
