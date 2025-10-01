using System.ComponentModel;

namespace SAM.Analytical
{
    [Description("Function Type.")]
    public enum FunctionType
    {
        [Description("Undefined")] Undefined,
        [Description("tcmvc")] tcmvc,
        [Description("tcmvn")] tcmvn,
        [Description("tmmvn")] tmmvn,
        [Description("tcbvc")] tcbvc,
        [Description("Other")] Other
    }
}