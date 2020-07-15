using System.ComponentModel;

namespace SAM.Core
{
    [Description("Enumerator defining the way in which two numbers are compared.")]
    public enum NumberComparisonType
    {
        [Description("Check if the input number and reference number are equal.")]
        Equals,
        [Description("Check if the input number and reference number are not equal.")]
        NotEquals,
        [Description("Check if the input number is greater than reference number.")]
        Greater,
        [Description("Check if the input number is smaller than reference number.")]
        Less,
        [Description("Check if the input number is smaller than or equal to reference number.")]
        LessOrEquals,
        [Description("Check if the input number is greater than or equal to reference number.")]
        GreaterOrEquals
    }
}