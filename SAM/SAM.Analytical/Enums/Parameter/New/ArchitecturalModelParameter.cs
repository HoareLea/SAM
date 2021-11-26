using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(BuildingModel)), Description("BuildingModel Parameter")]
    public enum BuildingModelParameter
    {
        [ParameterProperties("North Angle", "North Angle"), ParameterValue(Core.ParameterType.Double)] NorthAngle,
        [ParameterProperties("Cooling Sizing Factor", "Cooling Sizing Factor"), DoubleParameterValue(0)] CoolingSizingFactor,
        [ParameterProperties("Heating Sizing Factor", "Heating Sizing Factor"), DoubleParameterValue(0)] HeatingSizingFactor,
    }
}