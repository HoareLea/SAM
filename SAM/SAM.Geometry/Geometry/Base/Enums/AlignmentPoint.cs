using System.ComponentModel;

namespace SAM.Geometry
{
    [Description("Alignment Point")]
    public enum AlignmentPoint
    {
        [Description("Undefined")] Undefined,
        [Description("Start")] Start,
        [Description("Mid")] Mid,
        [Description("End")] End,
    }
}