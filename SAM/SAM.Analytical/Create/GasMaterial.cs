using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static GasMaterial GasMaterial(string name, string group, string displayName, string description, double defaultThickness, double vapourDiffusionFactor, double heatTransferCoefficient)
        {
            GasMaterial gasMaterial = new GasMaterial(name, group, displayName, description, double.NaN, double.NaN, double.NaN, double.NaN);
            gasMaterial.SetDefaultThickness(defaultThickness);
            gasMaterial.SetVapourDiffusionFactor(vapourDiffusionFactor);
            gasMaterial.SetHeatTransferCoefficient(heatTransferCoefficient);

            return gasMaterial;
        }
    }
}