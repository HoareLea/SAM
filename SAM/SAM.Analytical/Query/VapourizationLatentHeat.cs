namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Latent Heat of Vapourization - The specific heat of water condensation [J/g] for temparture range from -15C to 100C 
        /// </summary>
        /// <param name="dryBulbTemperature">Dry Bulb Temprature (from -15 to 100) [C]</param>
        /// <returns>Latent Heat of Vapourization [J/g]</returns>
        public static double VapourizationLatentHeat(this double dryBulbTemperature)
        {
            if(double.IsNaN(dryBulbTemperature) || dryBulbTemperature < -25 || dryBulbTemperature > 100)
            {
                return double.NaN;
            }

            if(dryBulbTemperature < 40)
            {
                return 2500.8 - (2.36 * dryBulbTemperature) + (0.0016 * System.Math.Pow(dryBulbTemperature, 2)) - (0.00006 * System.Math.Pow(dryBulbTemperature, 3));
            }

            return 2501 - (2.41 * dryBulbTemperature);
        }
    }
}