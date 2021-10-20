using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static GasMaterial GasMaterial(string name, string group, string displayName, string description, double defaultThickness, double vapourDiffusionFactor, double heatTransferCoefficient)
        {
            GasMaterial gasMaterial = new GasMaterial(name, group, displayName, description, double.NaN, double.NaN, double.NaN, double.NaN);
            gasMaterial.SetValue(MaterialParameter.DefaultThickness, defaultThickness);
            gasMaterial.SetValue(MaterialParameter.VapourDiffusionFactor, vapourDiffusionFactor);
            gasMaterial.SetValue(GasMaterialParameter.HeatTransferCoefficient, heatTransferCoefficient);

            return gasMaterial;
        }

        public static GasMaterial GasMaterial(string name, string group, string displayName, string description, double defaultThickness, double vapourDiffusionFactor, double heatTransferCoefficient, DefaultGasType defaultGasType)
        {
            GasMaterial gasMaterial = GasMaterial(name, group, displayName, description, defaultThickness, vapourDiffusionFactor, heatTransferCoefficient);
            gasMaterial.SetValue(GasMaterialParameter.DefaultGasType, defaultGasType.Description());

            return gasMaterial;
        }

        public static GasMaterial GasMaterial(this GasMaterial gasMaterial, string name, string displayName, string description, double defaultThickness, double heatTransferCoefficient)
        {
            if (gasMaterial == null)
                return null;
            
            GasMaterial result = new GasMaterial(name, System.Guid.NewGuid(), gasMaterial, displayName, description);
            result.SetValue(MaterialParameter.DefaultThickness, defaultThickness);
            result.SetValue(GasMaterialParameter.HeatTransferCoefficient, heatTransferCoefficient);

            if(gasMaterial.TryGetValue(GasMaterialParameter.DefaultGasType, out string defaultGasType) && !string.IsNullOrWhiteSpace(defaultGasType))
            {
                gasMaterial.SetValue(GasMaterialParameter.DefaultGasType, defaultGasType);
            }

            return result;
        }
    }
}