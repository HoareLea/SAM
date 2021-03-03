using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Core
{
    [Description("Arithmetic Operator")]
    public enum ArithmeticOperator
    {
        [Operator("+"), Description("Addition")] Addition,
        [Operator("-"), Description("Subtraction")] Subtraction,
        [Operator("*"), Description("Multiplication")] Multiplication,
        [Operator("/"), Description("Division")] Division,
        [Operator("%"), Description("Modulus")] Modulus
    }
}