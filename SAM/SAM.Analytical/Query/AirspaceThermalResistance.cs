namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Thermal Resistance of airspace according to BS EN ISO 6946:2017 [m2K/W]
        /// </summary>
        /// <param name="angle">Angle of heat flow direction in radians (measured in 2D from Upward direction (0, 1) Vector2D.SignedAngle(Vector2D)), angle less than 0 considered as downward direction</param>
        /// <param name="thickness">Air Gap Spacing [m]</param>
        /// <param name="meanTemperature">The mean thermodynamic temperature of the surface and of its surroundings [K]</param>
        /// <param name="surfaceEmissivity_1">Hemispherical Emissivity of the surface 1</param>
        /// <param name="surfaceEmissivity_2">Hemispherical Emissivity of the surface 2</param>
        /// <returns>Airspace Thermal Resistance [m2K/W]</returns>
        public static double AirspaceThermalResistance(double angle, double thickness, double meanTemperature = 283.15, double surfaceEmissivity_1 = 0.9, double surfaceEmissivity_2 = 0.9)
        {
            return 1 / (AirspaceConvectiveHeatTransferCoefficient(angle, thickness) + AirspaceRadiativeCoefficient(meanTemperature, surfaceEmissivity_1, surfaceEmissivity_2));
        }
    }
}