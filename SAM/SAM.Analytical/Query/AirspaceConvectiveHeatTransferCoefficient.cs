namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Calculates the airspace conduction/convection coefficient in Opaque Construction according to BS EN ISO 6946:2017 [W/(m2K)]
        /// </summary>
        /// <param name="angle">Angle of heat flow direction in radians (measured in 2D from Upward direction (0, 1) Vector2D.SignedAngle(Vector2D)), angle less than 0 considered as downward direction</param>
        /// <param name="thickness">Air Gap Spacing [m]</param>
        /// <returns></returns>
        public static double AirspaceConvectiveHeatTransferCoefficient(double angle, double thickness)
        {
            if (double.IsNaN(thickness))
                return double.NaN;
            
            double result = 0.025 / thickness;

            if (angle == 0)
                result = System.Math.Max(1.95, result); //Upwards
            else if (angle == System.Math.PI / 2)
                result = System.Math.Max(1.25, result); //Horizontal
            else if (angle < 0)
                result = System.Math.Max(0.12 * System.Math.Pow(thickness, 0.44), result); //Downwards
            else
            {
                double airspaceConvectiveHeatTransferCoefficient_Horizontal = AirspaceConvectiveHeatTransferCoefficient(System.Math.PI / 2, thickness);
                double airspaceConvectiveHeatTransferCoefficient_Upwards = AirspaceConvectiveHeatTransferCoefficient(0, thickness);
                if (double.IsNaN(airspaceConvectiveHeatTransferCoefficient_Horizontal) || double.IsNaN(airspaceConvectiveHeatTransferCoefficient_Upwards))
                    return double.NaN;

                result = airspaceConvectiveHeatTransferCoefficient_Horizontal + ((airspaceConvectiveHeatTransferCoefficient_Horizontal - airspaceConvectiveHeatTransferCoefficient_Upwards) * ((angle - (System.Math.PI / 2)) / (System.Math.PI / 2)));
            }

            return result;

        }
    }
}