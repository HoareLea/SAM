using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Panel)), Description("Panel Parameter")]
    public enum PanelParameter
    {
        [ParameterProperties("Transparent", "Transparent"), ParameterValue(Core.ParameterType.Boolean)] Transparent,
        [ParameterProperties("Color", "Color"), ParameterValue(Core.ParameterType.Color)] Color,
        [ParameterProperties("UValue", "Thermal Transmittance (U-Value) [W/m^2*K]"), DoubleParameterValue()] ThermalTransmittance,
        [ParameterProperties("Light Transmittance", "Light Transmittance [0-1]"), DoubleParameterValue(0, 1)] LightTransmittance,
        [ParameterProperties("Light Reflectance", "Light Reflectance [0-1]"), DoubleParameterValue(0, 1)] LightReflectance,
        [ParameterProperties("Direct Solar Energy Transmittance", "Direct Solar Energy Transmittance [0-1]"), DoubleParameterValue(0, 1)] DirectSolarEnergyTransmittance,
        [ParameterProperties("Direct Solar Energy Reflectance", "Direct Solar Energy Reflectance [0-1]"), DoubleParameterValue(0, 1)] DirectSolarEnergyReflectance,
        [ParameterProperties("Direct Solar Energy Absorptance", "Direct Solar Energy Absorptance [0-1]"), DoubleParameterValue(0, 1)] DirectSolarEnergyAbsorptance,
        [ParameterProperties("Total Solar Energy Transmittance", "Total Solar Energy Transmittance [0-1]"), DoubleParameterValue(0, 1)] TotalSolarEnergyTransmittance,
        [ParameterProperties("Pilkington Shading Short Wavelength Coefficient", "Pilkington Shading Short Wavelength Coefficient [0-1]"), DoubleParameterValue(0, 1)] PilkingtonShadingShortWavelengthCoefficient,
        [ParameterProperties("Pilkington Shading Long Wavelength Coefficient", "Pilkington Shading Long Wavelength Coefficient [0-1]"), DoubleParameterValue(0, 1)] PilkingtonShadingLongWavelengthCoefficient,
        [ParameterProperties("Adiabatic", "Adiabatic"), ParameterValue(Core.ParameterType.Boolean)] Adiabatic,
        [ParameterProperties("FeatureShade", "FeatureShade"), SAMObjectParameterValue(typeof(FeatureShade))] FeatureShade,
    }
}