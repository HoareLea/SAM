using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {

        /// <summary>
        /// Grashof Number (Gr) according to EN 673:1997 [-]
        /// </summary>
        /// <param name="fluidMaterial">SAM Fluid Material</param>
        /// <param name="temperatureDifference">Temperature difference between glass surfaces bounding the fluid space</param>
        /// <param name="width">Width of the space</param>
        /// <param name="meanTemperature">Mean Temperature</param>
        /// <returns>Grashof Number (Gr) [-]</returns>
        public static double GrashofNumber(this FluidMaterial fluidMaterial, double temperatureDifference, double width, double meanTemperature)
        {
            if (fluidMaterial == null || double.IsNaN(temperatureDifference) || double.IsNaN(width) || double.IsNaN(meanTemperature))
                return double.NaN;

            double dynamicViscosity = fluidMaterial.DynamicViscosity;
            if (double.IsNaN(dynamicViscosity))
                return double.NaN;

            double thermalConductivity = fluidMaterial.ThermalConductivity;
            if (double.IsNaN(thermalConductivity))
                return double.NaN;

            double density = fluidMaterial.Density;
            if (double.IsNaN(density))
                return double.NaN;

            double result = (9.81 * System.Math.Pow(width, 3) * temperatureDifference * System.Math.Pow(density, 2)) / (meanTemperature * System.Math.Pow(dynamicViscosity, 2));

            return result;
        }
    }
}