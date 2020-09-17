using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static TransparentMaterial TransparentMaterial(string name, string group, string displayName, string description, double thermalConductivity, double defaultThickness, double vapourDiffusionFactor, double externalSolarReflectance, double internalSolarReflectance, double externalLightReflectance, double internalLightReflectance, double externalEmissivity, double internalEmissivity, bool isBlind)
        {
            TransparentMaterial transparentMaterial = new TransparentMaterial(name, group, displayName, description, thermalConductivity, double.NaN, double.NaN);
            transparentMaterial.SetDefaultThickness(defaultThickness);
            transparentMaterial.SetVapourDiffusionFactor(vapourDiffusionFactor);
            transparentMaterial.SetExternalSolarReflectance(externalSolarReflectance);
            transparentMaterial.SetInternalSolarReflectance(internalSolarReflectance);
            transparentMaterial.SetExternalLightReflectance(externalLightReflectance);
            transparentMaterial.SetInternalLightReflectance(internalLightReflectance);
            transparentMaterial.SetExternalEmissivity(externalEmissivity);
            transparentMaterial.SetInternalEmissivity(internalEmissivity);
            transparentMaterial.SetIsBlind(isBlind);

            return transparentMaterial;
        }
    }
}