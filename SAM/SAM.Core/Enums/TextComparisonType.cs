using System.ComponentModel;

namespace SAM.Core
{
    [Description("Enumerator defining the way in which two strings are compared.")]
    public enum TextComparisonType
    {
        [Description("Text Equals")] Equals,
        [Description("Text Not Equals")] NotEquals,
        [Description("Text Contains")] Contains,
        [Description("Text Not Contains")] NotContains,
        [Description("Text Starts With")] StartsWith,
        [Description("Text Ends With")] EndsWith,
    }
}