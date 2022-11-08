using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Aperture)), Description("Aperture Parameter")]
    public enum ApertureParameter
    {
        [ParameterProperties("Thermal Transmittance", "Thermal Transmittance (U-Value) [W/m^2*K]"), DoubleParameterValue()] ThermalTransmittance,
        [ParameterProperties("Light Transmittance", "Light Transmittance"), DoubleParameterValue(0, 1)] LightTransmittance,
        [ParameterProperties("Light Reflectance", "Light Reflectance"), DoubleParameterValue(0, 1)] LightReflectance,
        [ParameterProperties("Direct Solar Energy Transmittance", "Direct Solar Energy Transmittance"), DoubleParameterValue(0, 1)] DirectSolarEnergyTransmittance,
        [ParameterProperties("Direct Solar Energy Reflectance", "Direct Solar Energy Reflectance"), DoubleParameterValue(0, 1)] DirectSolarEnergyReflectance,
        [ParameterProperties("Direct Solar Energy Absorptance", "Direct Solar Energy Absorptance"), DoubleParameterValue(0, 1)] DirectSolarEnergyAbsorptance,
        [ParameterProperties("Total Solar Energy Transmittance", "Total Solar Energy Transmittance"), DoubleParameterValue(0, 1)] TotalSolarEnergyTransmittance,
        [ParameterProperties("Pilkington Shading Short Wavelength Coefficient", "Pilkington Shading Short Wavelength Coefficient"), DoubleParameterValue(0, 1)] PilkingtonShadingShortWavelengthCoefficient,
        [ParameterProperties("Pilkington Shading Long Wavelength Coefficient", "Pilkington Shading Long Wavelength Coefficient"), DoubleParameterValue(0, 1)] PilkingtonShadingLongWavelengthCoefficient,
        [ParameterProperties("Opening Properties", "Opening Properties"), SAMObjectParameterValue(typeof(IOpeningProperties))] OpeningProperties,
    }
}