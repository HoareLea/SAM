namespace SAM.Core
{
    public static partial class Query
    {
        /// <summary>
        /// Latent Heat of Vapourization (Evaporation heat of water) - The specific heat of water condensation [J/kg] for temparture range from -25C to 100C 
        /// </summary>
        /// <param name="dryBulbTemperature">Dry Bulb Temprature (from -25 to 100) [C]</param>
        /// <returns>Latent Heat of Vapourization [J/kg]</returns>
        public static double VapourizationLatentHeat(this double dryBulbTemperature)
        {
            if(double.IsNaN(dryBulbTemperature) || dryBulbTemperature < -25 || dryBulbTemperature > 100)
            {
                return double.NaN;
            }

            double result = double.NaN;

            if (dryBulbTemperature < 40)
            {
                result = 2500.8 - (2.36 * dryBulbTemperature) + (0.0016 * System.Math.Pow(dryBulbTemperature, 2)) - (0.00006 * System.Math.Pow(dryBulbTemperature, 3));
            }
            else
            {
                result = 2501 - (2.41 * dryBulbTemperature);
            }

            return result * 1000;
        }
    }
}