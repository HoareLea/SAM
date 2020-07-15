using System.ComponentModel;

namespace SAM.Core
{
    [Description("Enumerator defining the way in which two strings are compared.")]
    public enum TextComparisonType
    {
        [Description("Check if the input string and reference string are the same.")]
        Equals,
        [Description("Check if the input string and reference string are different.")]
        NotEquals,
        [Description("Check if the input string contains reference string.")]
        Contains,
        [Description("Check if the input string does not contain reference string.")]
        NotContains,
        [Description("Check if the input string starts with reference string.")]
        StartsWith,
        [Description("Check if the input string ends with reference string.")]
        EndsWith,
    }
}