using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Weather
{
    /// <summary>
    /// Enum for Weather Data Parameters
    /// </summary>
    [AssociatedTypes(typeof(WeatherData)), Description("WeatherData Parameter")]
    public enum WeatherDataParameter
    {
        /// <summary>
        /// Ground Temperatures
        /// </summary>
        [ParameterProperties("Ground Temperatures", "Ground Temperatures"), SAMObjectParameterValue(typeof(Core.SAMCollection<GroundTemperature>))] GroundTemperatures,

        /// <summary>
        /// Country
        /// </summary>
        [ParameterProperties("Country", "Country"), ParameterValue(Core.ParameterType.String)] Country,

        /// <summary>
        /// State
        /// </summary>
        [ParameterProperties("State", "State"), ParameterValue(Core.ParameterType.String)] State,

        /// <summary>
        /// City
        /// </summary>
        [ParameterProperties("City", "City"), ParameterValue(Core.ParameterType.String)] City,

        /// <summary>
        /// Data Source
        /// </summary>
        [ParameterProperties("Data Source", "Data Source"), ParameterValue(Core.ParameterType.String)] DataSource,

        /// <summary>
        /// WMO Number
        /// </summary>
        [ParameterProperties("WMO Number", "WMO Number"), ParameterValue(Core.ParameterType.String)] WMONumber,

        /// <summary>
        /// UTC TimeZone
        /// </summary>
        [ParameterProperties("Time Zone", "UTC TimeZone"), ParameterValue(Core.ParameterType.String)] TimeZone,

        /// <summary>
        /// Comments 1
        /// </summary>
        [ParameterProperties("Comments 1", "Comments 1"), ParameterValue(Core.ParameterType.String)] Comments_1,

        /// <summary>
        /// Comments 2
        /// </summary>
        [ParameterProperties("Comments 2", "Comments 2"), ParameterValue(Core.ParameterType.String)] Comments_2,
    }
}
