using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(HostPartitionType)), Description("HostPartitionType Parameter")]
    public enum HostPartitionTypeParameter
    {
        [ParameterProperties("Partition Analytical Type", "Partition Analytical Type"), ParameterValue(Core.ParameterType.String)] PartitionAnalyticalType,
        [ParameterProperties("Description", "Description"), ParameterValue(Core.ParameterType.String)] Description,
        [ParameterProperties("Color", "Color"), ParameterValue(Core.ParameterType.Color)] Color,
    }
}