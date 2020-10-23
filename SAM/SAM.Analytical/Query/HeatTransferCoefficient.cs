using SAM.Core;
using System;

namespace SAM.Analytical
{
    public static partial class Query
    {

        /// <summary>
        /// Calculates Heat Transfer Coefficient (Thermal Conductance) [W/m2K] <see href="https://www.edsl.net/htmlhelp/Building_Simulator/ParametersforGasLayers.htm">source</see>
        /// </summary>
        /// <param name="fluidMaterial">SAM Fluid Material</param>
        /// <param name="temperatureDifference">Temperature difference between glass surfaces bounding the fluid space</param>
        /// <param name="width">Width of the space , default 0.0012m</param>
        /// <param name="meanTemperature">Mean Temperature</param>
        /// <param name="angle">Angle of heat flow direction in radians (measured in 2D from Upward direction (0, 1) Vector2D.SignedAngle(Vector2D)), angle less than 0 considered as downward direction</param>
        /// <returns>Heat Transfer Coefficient [W/m2K]</returns>
        public static double HeatTransferCoefficient(this FluidMaterial fluidMaterial, double temperatureDifference, double width, double meanTemperature, double angle)
        {
            double thermalConductivity = fluidMaterial.ThermalConductivity;
            if (double.IsNaN(thermalConductivity))
                return double.NaN;

            double nusseltNumber = NusseltNumber(fluidMaterial, temperatureDifference, width, meanTemperature, angle);
            if (double.IsNaN(nusseltNumber))
                return double.NaN;

            if (nusseltNumber < 1)
                nusseltNumber = 1;

            return nusseltNumber * (thermalConductivity / width);
        }

        [Obsolete]
        public static double HeatTransferCoefficient(this GasMaterial gasMaterial)
        {
            if (gasMaterial == null)
                return double.NaN;

            return gasMaterial.GetValue<double>(GasMaterialParameter.HeatTransferCoefficient);

            double result = double.NaN;
            if (!Core.Query.TryGetValue(gasMaterial, ParameterName_HeatTransferCoefficient(), out result))
                return double.NaN;

            return result;
        }
    }
}