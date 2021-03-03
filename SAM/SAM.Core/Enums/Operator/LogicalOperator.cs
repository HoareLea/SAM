using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Core
{
    [Description("Logical Operator")]
    public enum LogicalOperator
    {
        [Operator("&&"), Description("And")] And,
        [Operator("||"), Description("Or")] Or,
        [Operator("!"), Description("Not")] Not,
    }
}