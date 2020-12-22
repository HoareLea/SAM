using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Weather
{
    [AssociatedTypes(typeof(WeatherData)), Description("WeatherData Parameter")]
    public enum WeatherDataParameter
    {
        [ParameterProperties("Ground Temperatures", "Ground Temperatures"), SAMObjectParameterValue(typeof(Core.SAMCollection<GroundTemperature>))] GroundTemperatures,
    }
}