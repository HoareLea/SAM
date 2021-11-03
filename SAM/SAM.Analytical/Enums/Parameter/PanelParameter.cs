using System.ComponentModel;
using SAM.Core.Attributes;

namespace SAM.Analytical
{
    [AssociatedTypes(typeof(Panel)), Description("Panel Parameter")]
    public enum PanelParameter
    {
        [ParameterProperties("Transparent", "Transparent"), ParameterValue(Core.ParameterType.Boolean)] Transparent,
        [ParameterProperties("Color", "Color"), ParameterValue(Core.ParameterType.Color)] Color,

        [ParameterProperties("Bucket Size", "Bucket Size [m]"), DoubleParameterValue(0)] BucketSize,
            [ParameterProperties("Max Extend", "Max Extend [m]"), DoubleParameterValue(0)] MaxExtend,
        [ParameterProperties("Weight", "Weight"), DoubleParameterValue(0)] Weight
    }
}