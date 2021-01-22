using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Core.GuidCollection)), Description("Analytical Zone Parameter")]
    public enum AnalyticalZoneParameter
    {
        [ParameterProperties("Analytical Zone Type", "Analytical Zone Type"), ParameterValue(Core.ParameterType.String)] AnalyticalZoneType,
    }
}