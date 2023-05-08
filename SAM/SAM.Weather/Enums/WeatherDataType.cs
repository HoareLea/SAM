using System.ComponentModel;

namespace SAM.Weather
{
    /// <summary>
    /// Represents the types of weather data used in the SAM application.
    /// </summary>
    public enum WeatherDataType
    {
        /// <summary>
        /// Undefined
        /// </summary>
        [Description("Undefined")] Undefined,
        
        /// <summary>
        /// Global Solar Radiation [W/m2]
        /// </summary>
        [Description("Global Solar Radiation [W/m2]")] GlobalSolarRadiation,
        
        /// <summary>
        /// Diffuse Solar Radiation [W/m2]
        /// </summary>
        [Description("Diffuse Solar Radiation [W/m2]")] DiffuseSolarRadiation,

        /// <summary>
        /// Direct Solar Radiation [W/m2]
        /// </summary>
        [Description("Direct Solar Radiation [W/m2]")] DirectSolarRadiation,

        /// <summary>
        /// Cloud Cover [0-1]
        /// </summary>
        [Description("Cloud Cover [0-1]")] CloudCover,

        /// <summary>
        /// Dry Bulb Temperature [C]
        /// </summary>
        [Description("Dry Bulb Temperature [°C]")] DryBulbTemperature,

        /// <summary>
        /// Wet Bulb Temperature (Dew Point Temperature) [C]
        /// </summary>
        [Description("Wet Bulb Temperature [°C]")] WetBulbTemperature,

        /// <summary>
        /// Relative Humidity [%]
        /// </summary>
        [Description("Relative Humidity [%]")] RelativeHumidity,

        /// <summary>
        /// Wind Speed [m/s]
        /// </summary>
        [Description("Wind Speed [m/s]")] WindSpeed,

        /// <summary>
        /// Wind Direction [Degree]
        /// </summary>
        [Description("Wind Direction [°]")] WindDirection,

        /// <summary>
        /// Atmospheric Pressure [Pa]
        /// </summary>
        [Description("Atmospheric Pressure [Pa]")] AtmosphericPressure,
    }
}