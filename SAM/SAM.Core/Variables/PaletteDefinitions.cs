// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SAM.Core
{
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
            Description = "Soft categorical palette for room types and zoning categories.",
            PaletteType = PaletteType.Categorical,
            StableByName = true,
            Colors = HexList(
                "#7FC8C2", "#F6E58D", "#B8A9D9", "#F48C8C",
                "#6FA8DC", "#F7B267", "#A3D977", "#F2C6DE",
                "#CFCFCF", "#A67DB8", "#BCE5C8", "#FFE08A")
        };

        // ------------------------------
        // 2. Systems
        // ------------------------------
        public static readonly PaletteDefinition SamSystemsBold = new()
        {
            Name = "SAM Systems (Bold)",
            Description = "High-contrast palette for HVAC systems and technical classifications.",
            PaletteType = PaletteType.Categorical,
            StableByName = true,
            Colors = HexList(
                "#3E6FB0", "#E27D26", "#D64550", "#4FB3A4",
                "#4F9D69", "#E2C044", "#8E6FAF", "#F58EA8",
                "#8A6E4B", "#9A9A9A")
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
                "#1E5FA8", "#2BB3D6", "#F4D35E", "#EE964B", "#D7263D")
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
                "#2A6FBE", "#E6F2FF", "#FFFFFF", "#FFE6E6", "#C1121F")
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
                "#EAF4F4", "#8BC6EC", "#4C9F70", "#F4A261", "#E76F51")
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
                "#1B4F9C", // deep blue
                "#0096C7", // cyan
                "#2DC653", // green
                "#F4D35E", // yellow
                "#F77F00", // orange
                "#D62828"  // red
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
                "#111111", "#333333", "#555555", "#777777", "#999999", "#BBBBBB")
        };

        // ------------------------------
        // Helper Methods
        // ------------------------------
        private static IReadOnlyList<Color> HexList(params string[] hex)
            => hex.Select(ParseHex).ToList();

        private static Color ParseHex(string hex)
        {
            hex = hex.TrimStart('#');
            byte r = System.Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = System.Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = System.Convert.ToByte(hex.Substring(4, 2), 16);
            return Color.FromArgb(255, r, g, b);
        }
    }
}
