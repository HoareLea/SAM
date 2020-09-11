﻿using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {

        /// <summary>
        /// Calculates Heat Transfer Coefficient (Thermal Conductance) [W/m2K] <see href="https://www.edsl.net/htmlhelp/Building_Simulator/ParametersforGasLayers.htm">source</see>
        /// </summary>
        /// <returns>Heat Transfer Coefficient [W/m2K]</returns>
        public static double HeatTransferCoefficient(this FluidMaterial fluidMaterial, double temperatureDifference, double width, double meanTemperature, double angle)
        {
            double thermalConductivity = fluidMaterial.ThermalConductivity;
            if (double.IsNaN(thermalConductivity))
                return double.NaN;

            double nusseltNumber = NusseltNumber(fluidMaterial, temperatureDifference, width, meanTemperature, angle);
            if (double.IsNaN(nusseltNumber))
                return double.NaN;

            return nusseltNumber * (thermalConductivity / width);
        }
    }
}