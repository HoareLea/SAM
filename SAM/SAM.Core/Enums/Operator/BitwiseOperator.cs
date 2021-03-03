using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Core
{
    [Description("Bitwise Operator")]
    public enum BitwiseOperator
    {
        [Operator("&"), Description("And")] And,
        [Operator("|"), Description("Or")] Or,
        [Operator("^"), Description("Xor")] Xor,
        [Operator(">="), Description("Less Than")] LessThan,
        [Operator("~"), Description("Not")] Not,
        [Operator("<<"), Description("LeftShift")] LeftShift,
        [Operator(">>"), Description("RightShift")] RightShift
    }
}