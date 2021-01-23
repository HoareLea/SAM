using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Zone)), Description("Analytical Zone Parameter")]
    public enum ZoneParameter
    {
        [ParameterProperties("Zone Category", "Zone Category"), ParameterValue(Core.ParameterType.String)] ZoneCategory,
    }
}