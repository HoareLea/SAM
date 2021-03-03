using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Core
{
    [Description("Assignment Operator")]
    public enum AssignmentOperator
    {
        [Operator("="), Description("Assign")] Assign,
        [Operator("+="), Description("Add And Assign")] AddAndAssign,
        [Operator("-="), Description("Substract And Assign")] SubstractAndAssign,
        [Operator("*="), Description("Multiply And Assign")] MultiplyAndAssign,
        [Operator("/="), Description("Divide And Assign")] DivideAndAssign,
        [Operator("%="), Description("Modulus And Assign")] ModulusAndAssign,
        [Operator("<<="), Description("Left Shift And Assign")] LeftShiftAndAssign,
        [Operator(">>="), Description("Right Shift And Assign")] RightShiftAndAssign,
        [Operator("&="), Description("Bitwise And And Assign")] BitwiseAndAndAssign,
        [Operator("|="), Description("Bitwise Or And Assign")] BitwiseOrAndAssign,
        [Operator("^="), Description("Bitwise Xor And Assign")] BitwiseXorAndAssign,
    }
}