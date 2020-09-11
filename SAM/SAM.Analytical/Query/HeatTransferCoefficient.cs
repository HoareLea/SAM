using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {

        /// <summary>
        /// Calculates Heat Transfer Coefficient (Thermal Conductance) [W/m2K] <see href="https://www.edsl.net/htmlhelp/Building_Simulator/ParametersforGasLayers.htm">source</see>
        /// </summary>
        /// <param name="defaultGasType">Default Gas Type</param>
        /// <param name="width">Width of the cavity [m]</param>
        /// <param name="temperatureDifference">Mean temperature difference across the cavity [K]</param>
        /// <param name="height">Height of the cavity in case of horizontal heat flow (vertically oriented cavity) [m]</param>
        /// <param name="heatFlowDirection">Heat Flow Direction</param>
        /// <returns>Heat Transfer Coefficient [W/m2K]</returns>
        public static double HeatTransferCoefficient(this DefaultGasType defaultGasType, double width, double height, double temperatureDifference, HeatFlowDirection heatFlowDirection)
        {
            GasMaterial gasMaterial = GasMaterial(defaultGasType);
            if (gasMaterial == null)
                return double.NaN;

            double thermalConductivity = gasMaterial.ThermalConductivity;
            if (double.IsNaN(thermalConductivity))
                return double.NaN;

            double nu = Nu(defaultGasType, width, height, temperatureDifference, heatFlowDirection);
            if (nu < 1)
                nu = 1;

            double result = nu * thermalConductivity / width;

            return result;
        }
    }
}