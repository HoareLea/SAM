using System.ComponentModel;
using SAM.Core;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(AnalyticalModelSimulationResult)), Description("AnalyticalModel Simulation Result Parameter")]
    public enum AnalyticalModelSimulationResultParameter
    {
        [ParameterProperties("Consumption Heating", "Consumption Heating [kWh]"), ParameterValue(ParameterType.Double)] ConsumptionHeating,
        [ParameterProperties("Peak Heating Load", "Peak Heating Load [kW]"), ParameterValue(ParameterType.Double)] PeakHeatingLoad,
        [ParameterProperties("Peak Heating Hour", "Peak Heating Hour [hr]"), ParameterValue(ParameterType.Integer)] PeakHeatingHour,

        [ParameterProperties("Consumption Cooling", "Consumption Cooling [kWh]"), ParameterValue(ParameterType.Double)] ConsumptionCooling,
        [ParameterProperties("Peak Cooling Load", "Peak Cooling Load [kW]"), ParameterValue(ParameterType.Double)] PeakCoolingLoad,
        [ParameterProperties("Peak Cooling Hour", "Peak Cooling Hour [hr]"), ParameterValue(ParameterType.Integer)] PeakCoolingHour,
    }
}