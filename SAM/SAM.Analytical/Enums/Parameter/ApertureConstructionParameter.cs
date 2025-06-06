using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(ApertureConstruction)), Description("ApertureConstruction Parameter")]
    public enum ApertureConstructionParameter
    {
        [ParameterProperties("Default Panel Type", "Default Panel Type"), ParameterValue(Core.ParameterType.String)] DefaultPanelType,
        [ParameterProperties("Description", "Description"), ParameterValue(Core.ParameterType.String)] Description,
        [ParameterProperties("Transparent", "Transparent"), ParameterValue(Core.ParameterType.Boolean)] Transparent,
        [ParameterProperties("Color", "Color"), ParameterValue(Core.ParameterType.Color)] Color,
        [ParameterProperties("Default Frame Width", "Default Frame Width"), ParameterValue(Core.ParameterType.Double)] DefaultFrameWidth,
        [ParameterProperties("Is Internal Shadow", "Is Internal Shadow"), ParameterValue(Core.ParameterType.Boolean)] IsInternalShadow,
        
        [ParameterProperties("Light Transmittance", "Light Transmittance (LT Value) [-]"), DoubleParameterValue(0, 1)] LightTransmittance,
        [ParameterProperties("Total Solar Energy Transmittance", "Total Solar Energy Transmittance (g Value) [-]"), DoubleParameterValue(0, 1)] TotalSolarEnergyTransmittance,
        [ParameterProperties("Thermal Transmittance", "Thermal Transmittance (U Value) [-]"), DoubleParameterValue()] ThermalTransmittance,

        [ParameterProperties("Pane Additional Heat Transfer", "Pane Additional Heat Transfer [%]"), DoubleParameterValue(-50, 150)] PaneAdditionalHeatTransfer,
        [ParameterProperties("Frame Additional Heat Transfer", "Frame Additional Heat Transfer [%]"), DoubleParameterValue(-50, 150)] FrameAdditionalHeatTransfer,
    }
}