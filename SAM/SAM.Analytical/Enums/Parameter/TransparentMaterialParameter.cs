using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Core.TransparentMaterial)), Description("TransparentMaterial Parameter")]
    public enum TransparentMaterialParameter
    {
        [ParameterProperties("External Emissivity", "External Emissivity"), DoubleParameterValue(0)] ExternalEmissivity,
        [ParameterProperties("External Light Reflectance", "External Light Reflectance"), DoubleParameterValue(0)] ExternalLightReflectance,
        [ParameterProperties("External Solar Reflectance", "External Solar Reflectance"), DoubleParameterValue(0)] ExternalSolarReflectance,
        [ParameterProperties("Internal Emissivity", "Internal Emissivity"), DoubleParameterValue(0)] InternalEmissivity,
        [ParameterProperties("Internal Light Reflectance", "Internal Light Reflectance"), DoubleParameterValue(0)] InternalLightReflectance,
        [ParameterProperties("Internal Solar Reflectance", "Internal Solar Reflectance"), DoubleParameterValue(0)] InternalSolarReflectance,
        [ParameterProperties("Is Blind", "Is Blind"), ParameterValue(Core.ParameterType.Boolean)] IsBlind,
        [ParameterProperties("Light Transmittance", "Light Transmittance"), DoubleParameterValue(0)] LightTransmittance,
        [ParameterProperties("Solar Transmittance", "Solar Transmittance"), DoubleParameterValue(0)] SolarTransmittance,
    }
}