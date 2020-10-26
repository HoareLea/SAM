using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static TransparentMaterial TransparentMaterial(string name, string group, string displayName, string description, double thermalConductivity, double defaultThickness, double vapourDiffusionFactor, double solarTransmittance, double lightTransmittance, double externalSolarReflectance, double internalSolarReflectance, double externalLightReflectance, double internalLightReflectance, double externalEmissivity, double internalEmissivity, bool isBlind)
        {
            TransparentMaterial transparentMaterial = new TransparentMaterial(name, group, displayName, description, thermalConductivity, double.NaN, double.NaN);

            transparentMaterial.SetValue(MaterialParameter.DefaultThickness, defaultThickness);
            transparentMaterial.SetValue(MaterialParameter.VapourDiffusionFactor, vapourDiffusionFactor);
            transparentMaterial.SetValue(TransparentMaterialParameter.SolarTransmittance, solarTransmittance);
            transparentMaterial.SetValue(TransparentMaterialParameter.LightTransmittance, lightTransmittance);
            transparentMaterial.SetValue(TransparentMaterialParameter.ExternalSolarReflectance, externalSolarReflectance);
            transparentMaterial.SetValue(TransparentMaterialParameter.InternalSolarReflectance, internalSolarReflectance);
            transparentMaterial.SetValue(TransparentMaterialParameter.ExternalLightReflectance, externalLightReflectance);
            transparentMaterial.SetValue(TransparentMaterialParameter.InternalLightReflectance, internalLightReflectance);
            transparentMaterial.SetValue(TransparentMaterialParameter.ExternalEmissivity, externalEmissivity);
            transparentMaterial.SetValue(TransparentMaterialParameter.InternalEmissivity, internalEmissivity);
            transparentMaterial.SetValue(TransparentMaterialParameter.IsBlind, isBlind);

            return transparentMaterial;
        }
    }
}