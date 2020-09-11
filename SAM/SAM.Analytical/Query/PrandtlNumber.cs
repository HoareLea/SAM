using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {

        /// <summary>
        /// Prandtl Number (Pr) according to EN 673:1997 [-]
        /// </summary>
        /// <param name="fluidMaterial">SAM Fluid Material</param>
        /// <returns>Prandtl Number (Pr) [-]</returns>
        public static double PrandtlNumber(this FluidMaterial fluidMaterial)
        {
            if (fluidMaterial == null)
                return double.NaN;

            double dynamicViscosity = fluidMaterial.DynamicViscosity;
            if (double.IsNaN(dynamicViscosity))
                return double.NaN;

            double specificHeatCapacity = fluidMaterial.SpecificHeatCapacity;
            if (double.IsNaN(specificHeatCapacity))
                return double.NaN;

            double thermalConductivity = fluidMaterial.ThermalConductivity;
            if (double.IsNaN(thermalConductivity))
                return double.NaN;

            double result = (dynamicViscosity * specificHeatCapacity) / thermalConductivity;

            return result;
        }
    }
}