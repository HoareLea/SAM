using System.ComponentModel;

namespace SAM.Weather
{
    [Description("Weather Data Type.")]
    public enum WeatherDataType
    {
        /// <summary>
        /// Undefined
        /// </summary>
        [Description("Undefined")] Undefined,
        
        /// <summary>
        /// Global Solar Radiation [W/m2]
        /// </summary>
        [Description("Global Solar Radiation")] GlobalSolarRadiation,
        
        /// <summary>
        /// Diffuse Solar Radiation [W/m2]
        /// </summary>
        [Description("Diffuse Solar Radiation")] DiffuseSolarRadiation,

        /// <summary>
        /// Cloud Cover [0-1]
        /// </summary>
        [Description("Cloud Cover")] CloudCover,

        /// <summary>
        /// Dry Bulb Temperature [C]
        /// </summary>
        [Description("Dry Bulb Temperature")] DryBulbTemperature,

        /// <summary>
        /// Relative Humidity [%]
        /// </summary>
        [Description("Relative Humidity")] RelativeHumidity,

        /// <summary>
        /// Wind Speed [m/s]
        /// </summary>
        [Description("Wind Speed")] WindSpeed,

        /// <summary>
        /// Wind Direction [Degree]
        /// </summary>
        [Description("Wind Direction")] WindDirection,

    }
}