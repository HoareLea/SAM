namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Calculates Infiltration Sensible Gain
        /// </summary>
        /// <param name="space">Space</param>
        /// <param name="dryBulbTemperature_Outside">Outside Air Dry Bulb Temperature [C]</param>
        /// <param name="dryBulbTemperature_Inside">Inside Air Dry Bulb Temperature [C]</param>
        /// <param name="density_Outside">Outside Air Density [kg/m3]</param>
        /// <returns>Calculated Infiltration Sensible Gain [W]</returns>
        public static double CalculatedInfiltrationSensibleGain(this Space space, double dryBulbTemperature_Outside, double dryBulbTemperature_Inside, double density_Outside)
        {
            if(space == null|| double.IsNaN(dryBulbTemperature_Inside) || double.IsNaN(density_Outside))
            {
                return double.NaN;
            }
            
            double vapourizationLatentHeat = Core.Query.VapourizationLatentHeat(dryBulbTemperature_Inside);
            if (double.IsNaN(vapourizationLatentHeat))
            {
                return double.NaN;
            }

            double calculatedInfiltrationMassAirFlow = CalculatedInfiltrationAirFlow(space) * density_Outside; //[kg/s]
            if (double.IsNaN(calculatedInfiltrationMassAirFlow))
            {
                return double.NaN;
            }

            return calculatedInfiltrationMassAirFlow * 1.005 * (dryBulbTemperature_Inside - dryBulbTemperature_Outside);
        }
    }
}