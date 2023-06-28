namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Calculates Infiltration Latent Gain
        /// </summary>
        /// <param name="space">Space</param>
        /// <param name="humidityRatio_Outside">Outside Humidity Ratio [kg_waterVapor/kg_dryAir]</param>
        /// <param name="humidityRatio_Inside">Inside Humidity Ratio [kg_waterVapor/kg_dryAir]</param>
        /// <param name="dryBulbTemperature_Inside">Inside Air Dry Bulb Temperature [C]</param>
        /// <param name="density_Outside">Outside Air Density [kg/m3]</param>
        /// <returns>Calculated Infiltration Latent Gain [W]</returns>
        public static double CalculatedInfiltrationLatentGain(this Space space, double humidityRatio_Outside, double humidityRatio_Inside, double dryBulbTemperature_Inside, double density_Outside)
        {
            if(space == null || double.IsNaN(humidityRatio_Inside) || double.IsNaN(humidityRatio_Outside) || double.IsNaN(dryBulbTemperature_Inside) || double.IsNaN(density_Outside))
            {
                return double.NaN;
            }
            
            double vapourizationLatentHeat = VapourizationLatentHeat(dryBulbTemperature_Inside);
            if (double.IsNaN(vapourizationLatentHeat))
            {
                return double.NaN;
            }

            double calculatedInfiltrationMassAirFlow = CalculatedInfiltrationAirFlow(space) * density_Outside; //[kg/s]
            if (double.IsNaN(calculatedInfiltrationMassAirFlow))
            {
                return double.NaN;
            }

            return calculatedInfiltrationMassAirFlow * (humidityRatio_Outside - humidityRatio_Inside) * (1.86 * dryBulbTemperature_Inside + vapourizationLatentHeat) * 1000;
        }
    }
}