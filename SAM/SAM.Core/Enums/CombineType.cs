using System.ComponentModel;

namespace SAM.Core
{
    [Description("CombineType Enum")]
    public enum CombineType
    {
        [Description("Undefined")] Undefined,
        [Description("Sum")] Sum,
        [Description("Average")] Average,
        [Description("Min")] Min,
        [Description("Max")] Max,
    }
}