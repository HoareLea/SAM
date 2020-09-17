using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static OpaqueMaterial OpaqueMaterial(string name, string group, string displayName, string description, double thermalConductivity, double specificHeatCapacity, double density, double defaultThickness, double vapourDiffusionFactor, double externalSolarReflectance, double internalSolarReflectance, double externalLightReflectance, double internalLightReflectance, double externalEmissivity, double internalEmissivity, bool ignoreThermalTransmittanceCalculations)
        {
            OpaqueMaterial opaqueMaterial = new OpaqueMaterial(name, group, displayName, description, thermalConductivity, specificHeatCapacity, density);
            opaqueMaterial.SetDefaultThickness(defaultThickness);
            opaqueMaterial.SetVapourDiffusionFactor(vapourDiffusionFactor);
            opaqueMaterial.SetExternalSolarReflectance(externalSolarReflectance);
            opaqueMaterial.SetInternalSolarReflectance(internalSolarReflectance);
            opaqueMaterial.SetExternalLightReflectance(externalLightReflectance);
            opaqueMaterial.SetInternalLightReflectance(internalLightReflectance);
            opaqueMaterial.SetExternalEmissivity(externalEmissivity);
            opaqueMaterial.SetInternalEmissivity(internalEmissivity);
            opaqueMaterial.SetIgnoreThermalTransmittanceCalculations(ignoreThermalTransmittanceCalculations);

            return opaqueMaterial;
        }
    }
}