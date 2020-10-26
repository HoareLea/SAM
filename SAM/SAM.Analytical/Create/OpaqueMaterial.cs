using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static OpaqueMaterial OpaqueMaterial(string name, string group, string displayName, string description, double thermalConductivity, double specificHeatCapacity, double density, double defaultThickness, double vapourDiffusionFactor, double externalSolarReflectance, double internalSolarReflectance, double externalLightReflectance, double internalLightReflectance, double externalEmissivity, double internalEmissivity, bool ignoreThermalTransmittanceCalculations)
        {
            OpaqueMaterial opaqueMaterial = new OpaqueMaterial(name, group, displayName, description, thermalConductivity, specificHeatCapacity, density);

            opaqueMaterial.SetValue(MaterialParameter.DefaultThickness, defaultThickness);
            opaqueMaterial.SetValue(MaterialParameter.VapourDiffusionFactor, vapourDiffusionFactor);
            opaqueMaterial.SetValue(OpaqueMaterialParameter.ExternalSolarReflectance, externalSolarReflectance);
            opaqueMaterial.SetValue(OpaqueMaterialParameter.InternalSolarReflectance, internalSolarReflectance);
            opaqueMaterial.SetValue(OpaqueMaterialParameter.ExternalLightReflectance, externalLightReflectance);
            opaqueMaterial.SetValue(OpaqueMaterialParameter.InternalLightReflectance, internalLightReflectance);
            opaqueMaterial.SetValue(OpaqueMaterialParameter.ExternalEmissivity, externalEmissivity);
            opaqueMaterial.SetValue(OpaqueMaterialParameter.InternalEmissivity, internalEmissivity);
            opaqueMaterial.SetValue(OpaqueMaterialParameter.IgnoreThermalTransmittanceCalculations, ignoreThermalTransmittanceCalculations);

            return opaqueMaterial;
        }
    }
}