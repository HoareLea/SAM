using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Weather
{
    [AssociatedTypes(typeof(WeatherData)), Description("WeatherData Parameter")]
    public enum WeatherDataParameter
    {
        [ParameterProperties("Ground Temperatures", "Ground Temperatures"), SAMObjectParameterValue(typeof(Core.SAMCollection<GroundTemperature>))] GroundTemperatures,
        [ParameterProperties("Country", "Country"), ParameterValue(Core.ParameterType.String)] Country,
        [ParameterProperties("State", "State"), ParameterValue(Core.ParameterType.String)] State,
        [ParameterProperties("City", "City"), ParameterValue(Core.ParameterType.String)] City,
        [ParameterProperties("Data Source", "Data Source"), ParameterValue(Core.ParameterType.String)] DataSource,
        [ParameterProperties("WMO Number", "WMO Number"), ParameterValue(Core.ParameterType.String)] WMONumber,
        [ParameterProperties("Time Zone", "TimeZone [h] minimum -12, maximum +14"), ParameterValue(Core.ParameterType.Integer)] TimeZone,
        [ParameterProperties("Comments 1", "Comments 1"), ParameterValue(Core.ParameterType.String)] Comments_1,
        [ParameterProperties("Comments 2", "Comments 2"), ParameterValue(Core.ParameterType.String)] Comments_2,
    }
}