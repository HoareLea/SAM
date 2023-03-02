using System.ComponentModel;

namespace SAM.Core
{
    [Description("Filter Logical Operator")]
    public enum FilterLogicalOperator
    {
        [Description("Undefined")] Undefined,
        [Description("And")] And,
        [Description("Or")] Or,
    }
}