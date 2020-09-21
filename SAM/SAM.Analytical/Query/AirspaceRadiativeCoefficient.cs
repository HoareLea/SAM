namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Airspace Radiative Coeffiicient according to BS EN ISO 6946:2017 [W/m2K]
        /// </summary>
        /// <param name="meanTemperature">The mean thermodynamic temperature of the surface and of its surroundings [K]</param>
        /// <param name="surfaceEmissivity_1">Hemispherical Emissivity of the surface 1</param>
        /// <param name="surfaceEmissivity_2">Hemispherical Emissivity of the surface 2</param>
        /// <returns>Airspace Radiative Coefficient [W/m2K]</returns>
        public static double AirspaceRadiativeCoefficient(double meanTemperature = 283.15, double surfaceEmissivity_1 = 0.9, double surfaceEmissivity_2 = 0.9)
        {
            return AirspaceIntersurfaceEmittance(surfaceEmissivity_1, surfaceEmissivity_2) * BlackBodySurfaceRadiativeCoefficient(meanTemperature);
        }
    }
}