namespace SAM.Analytical
{
    public static partial class Query
    {

        /// <summary>
        /// Nusselt number for Gas at specific conditions <see href="https://www.edsl.net/htmlhelp/Building_Simulator/ParametersforGasLayers.htm">source</see>
        /// </summary>
        /// <param name="defaultGasType">Default Gas Type</param>
        /// <param name="width">Width of the cavity [m]</param>
        /// <param name="temperatureDifference">Mean temperature difference across the cavity [K]</param>
        /// <param name="height">Height of the cavity in case of horizontal heat flow (vertically oriented cavity) [m]</param>
        /// <param name="heatFlowDirection">Heat Flow Direction</param>
        /// <returns>Nusselt number</returns>
        public static double Nu(this DefaultGasType defaultGasType, double width, double height, double temperatureDifference, HeatFlowDirection heatFlowDirection)
        {
            double grPr = GrPr(defaultGasType, width, temperatureDifference);
            if (double.IsNaN(grPr))
                return double.NaN;

            double c = double.NaN;
            double n = double.NaN;
            double m = double.NaN;

            switch(heatFlowDirection)
            {
                case HeatFlowDirection.Down:
                    c = 1;
                    n = 0;
                    m = 0;
                    break;
                case HeatFlowDirection.Horizontal:
                    m = -1 / 9;
                    if(grPr < 200000)
                    {
                        c = 0.197;
                        n = 0.25;
                    }
                    else
                    {
                        c = 0.073;
                        n = 1 / 3;
                    }
                    break;
                case HeatFlowDirection.Up:
                    m = 0;
                    if(grPr < 7000)
                    {
                        c = 0.059;
                        n = 2 / 5;
                    }
                    else if(7000 < grPr && grPr < 320000)
                    {
                        c = 0.212;
                        n = 0.25;
                    }
                    else
                    {
                        c = 0.061;
                        n = 1 / 3;
                    }
                    break;
            }

            if (double.IsNaN(c) || double.IsNaN(n) || double.IsNaN(m))
                return double.NaN;

            double result = c * System.Math.Pow(grPr, n) * System.Math.Pow(height * width, m);

            return result;
        }
    }
}