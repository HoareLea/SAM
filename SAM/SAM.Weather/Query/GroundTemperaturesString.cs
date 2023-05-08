using System.Collections.Generic;

namespace SAM.Weather
{
    /// <summary>
    /// This class provides methods for creating and executing queries.
    /// </summary>
    public static partial class Query
    {
        /// <summary>
        /// Generates a string representation of the GroundTemperatures in the WeatherData object.
        /// </summary>
        /// <returns>
        /// A string representation of the GroundTemperatures in the WeatherData object.
        /// </returns>
        public static string GroundTemperaturesString(this WeatherData weatherData)
        {
            if (weatherData == null)
                return null;

            Core.SAMCollection<GroundTemperature> groundTemperatures;
            weatherData.TryGetValue(WeatherDataParameter.GroundTemperatures, out groundTemperatures);

            List<string> values_GroundTemperatures = new List<string>();

            if (groundTemperatures != null)
            {
                values_GroundTemperatures.Add(groundTemperatures.Count.ToString());
                foreach (GroundTemperature groundTemperature in groundTemperatures)
                {
                    List<string> value_Temp = new List<string>();
                    value_Temp.Add(double.IsNaN(groundTemperature.Depth) ? string.Empty : groundTemperature.Depth.ToString());
                    value_Temp.Add(double.IsNaN(groundTemperature.Conductivity) ? string.Empty : groundTemperature.Conductivity.ToString());
                    value_Temp.Add(double.IsNaN(groundTemperature.Density) ? string.Empty : groundTemperature.Density.ToString());
                    value_Temp.Add(double.IsNaN(groundTemperature.SpecificHeat) ? string.Empty : groundTemperature.SpecificHeat.ToString());
                    foreach (double temperature in groundTemperature.Temperatures)
                        value_Temp.Add(double.IsNaN(temperature) ? string.Empty : temperature.ToString());

                    values_GroundTemperatures.Add(string.Join(",", value_Temp));
                }
            }
            else
            {
                values_GroundTemperatures.Add(0.ToString());
            }

            string[] values = new string[] {
                "GROUND TEMPERATURES",
                string.Join(",", values_GroundTemperatures)
            };

            return string.Join(",", values);
        }
    }
}