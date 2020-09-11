namespace SAM.Analytical
{
    public static partial class Query
    {

        /// <summary>
        /// The product of the Grashof and Prandtl numbers. <see href="https://www.edsl.net/htmlhelp/Building_Simulator/ParametersforGasLayers.htm">source</see>
        /// </summary>
        /// <param name="defaultGasType">Default Gas Type</param>
        /// <param name="width">Width of the cavity [m]</param>
        /// <param name="temperatureDifference">Mean temperature difference across the cavity [K]</param>
        /// <returns>Grashof amd Prandtl product</returns>
        public static double GrPr(this DefaultGasType defaultGasType, double width, double temperatureDifference)
        {
            double a = double.NaN;

            switch(defaultGasType)
            {
                case DefaultGasType.Air:
                    a = 1.241e8;
                    break;
                case DefaultGasType.Argon:
                    a = 1.433e8;
                    break;
                case DefaultGasType.SulfurHexaFluoride:
                    a = 4.607e9;
                    break;
            }

            if (double.IsNaN(a))
                return double.NaN;

            return a * width * width * width * temperatureDifference;
        }
    }
}