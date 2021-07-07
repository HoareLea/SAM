namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Interfurface Emittance for Airspace according to BS EN ISO 6946:2017 [-]
        /// </summary>
        /// <param name="surfaceEmissivity_1">Hemispherical Emissivity of the surface 1</param>
        /// <param name="surfaceEmissivity_2">Hemispherical Emissivity of the surface 2</param>
        /// <returns>Air Intersurface Emittance</returns>
        public static double AirspaceIntersurfaceEmittance(double surfaceEmissivity_1 = 0.9, double surfaceEmissivity_2 = 0.9)
        {
            return 1 / ((1 / surfaceEmissivity_1) + (1 / surfaceEmissivity_2) - 1);
        }
    }
}