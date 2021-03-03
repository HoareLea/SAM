using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Core
{
    [Description("Increment And Decrement Operator")]
    public enum IncrementAndDecrementOperator
    {
        [Operator("++"), Description("Increment")] Increment,
        [Operator("--"), Description("Decrement")] Decrement,
    }
}