using SAM.Core;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public static class PaletteDefinitions
{
    public static readonly IReadOnlyList<PaletteDefinition> All =
        new[]
        {
            SamSpacesSoft,
            SamSystemsBold,
            SamThermal,
            SamComfort,
            SamEnergy,
            SamSpectrumAnalytical,
            SamSpectrumClassic,
            SamMonochrome
        };

    // ------------------------------
    // 1. Spaces
    // ------------------------------
    public static readonly PaletteDefinition SamSpacesSoft = new()
    {
        Name = "SAM Spaces (Soft)",
        Description = "Soft but clearly separated categorical palette for room types and zoning categories.",
        PaletteType = PaletteType.Categorical,
        StableByName = true,
        Colors = HexList(
            "#4E79A7", // blue
            "#F28E2B", // orange
            "#59A14F", // green
            "#E15759", // red
            "#B07AA1", // purple
            "#76B7B2", // teal
            "#EDC948", // yellow
            "#9C755F", // brown
            "#FF9DA7", // pink
            "#BAB0AC", // grey
            "#86BCB6", // mint
            "#D37295"  // rose
        )
    };

    // ------------------------------
    // 2. Systems
    // ------------------------------
    public static readonly PaletteDefinition SamSystemsBold = new()
    {
        Name = "SAM Systems (Bold)",
        Description = "High-contrast categorical palette for HVAC systems and technical classifications.",
        PaletteType = PaletteType.Categorical,
        StableByName = true,
        Colors = HexList(
            "#1F77B4", // blue
            "#FF7F0E", // orange
            "#2CA02C", // green
            "#D62728", // red
            "#9467BD", // purple
            "#17BECF", // cyan
            "#BCBD22", // olive
            "#8C564B", // brown
            "#E377C2", // magenta
            "#7F7F7F"  // grey
        )
    };

    // ------------------------------
    // 3. Thermal Sequential
    // ------------------------------
    public static readonly PaletteDefinition SamThermal = new()
    {
        Name = "SAM Thermal (Cool → Warm)",
        Description = "Sequential gradient for temperature and thermal intensity.",
        PaletteType = PaletteType.Sequential,
        Colors = HexList(
            "#1D4E89", // deep blue
            "#2C7FB8", // blue
            "#41B6C4", // cyan
            "#FEE08B", // warm yellow
            "#F46D43", // orange
            "#B2182B"  // red
        )
    };

    // ------------------------------
    // 4. Comfort Diverging
    // ------------------------------
    public static readonly PaletteDefinition SamComfort = new()
    {
        Name = "SAM Comfort (Cool ↔ Neutral ↔ Warm)",
        Description = "Diverging palette centred on neutral conditions.",
        PaletteType = PaletteType.Diverging,
        Midpoint = 0.5,
        Colors = HexList(
            "#2166AC", // cool blue
            "#92C5DE", // light blue
            "#F7F7F7", // neutral
            "#F4A582", // light warm
            "#B2182B"  // warm red
        )
    };

    // ------------------------------
    // 5. Energy Sequential
    // ------------------------------
    public static readonly PaletteDefinition SamEnergy = new()
    {
        Name = "SAM Energy (Intensity)",
        Description = "Sequential palette for energy demand and loads.",
        PaletteType = PaletteType.Sequential,
        Colors = HexList(
            "#EDF8FB", // very low
            "#B2E2E2", // low
            "#66C2A4", // medium
            "#FDAE61", // high
            "#D73027"  // very high
        )
    };

    // ------------------------------
    // 6. Spectrum (Analytical)
    // ------------------------------
    public static readonly PaletteDefinition SamSpectrumAnalytical = new()
    {
        Name = "SAM Spectrum (Analytical)",
        Description = "Balanced spectral gradient for solar radiation, illuminance and intensity plots.",
        PaletteType = PaletteType.Sequential,
        Colors = HexList(
            "#243B98", // deep blue
            "#1F9AC6", // cyan-blue
            "#39B54A", // green
            "#F2D14C", // yellow
            "#F28E2B", // orange
            "#C73E1D"  // red
        )
    };

    // ------------------------------
    // 7. Spectrum (Classic)
    // ------------------------------
    public static readonly PaletteDefinition SamSpectrumClassic = new()
    {
        Name = "SAM Spectrum (Classic)",
        Description = "High-saturation spectral palette for strong visual contrast across wide value ranges.",
        PaletteType = PaletteType.Sequential,
        Colors = HexList(
            "#0000FF", // blue
            "#00FFFF", // cyan
            "#00FF00", // green
            "#FFFF00", // yellow
            "#FF7F00", // orange
            "#FF0000"  // red
        )
    };

    // ------------------------------
    // 8. Monochrome
    // ------------------------------
    public static readonly PaletteDefinition SamMonochrome = new()
    {
        Name = "SAM Monochrome (Print)",
        Description = "Greyscale palette for reports and print.",
        PaletteType = PaletteType.Sequential,
        Colors = HexList(
            "#F5F5F5",
            "#D9D9D9",
            "#BDBDBD",
            "#969696",
            "#636363",
            "#252525"
        )
    };

    // ------------------------------
    // Helper Methods
    // ------------------------------
    private static IReadOnlyList<Color> HexList(params string[] hex)
        => hex.Select(ParseHex).ToList();

    private static Color ParseHex(string hex)
    {
        hex = hex.Trim().TrimStart('#');
        byte r = System.Convert.ToByte(hex.Substring(0, 2), 16);
        byte g = System.Convert.ToByte(hex.Substring(2, 2), 16);
        byte b = System.Convert.ToByte(hex.Substring(4, 2), 16);
        return Color.FromArgb(255, r, g, b);
    }
}