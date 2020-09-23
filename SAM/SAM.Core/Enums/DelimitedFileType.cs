using System.ComponentModel;

namespace SAM.Core
{
    [Description("Delimited File Type")]
    public enum DelimitedFileType
    {
        [Description("Undefined")] Undefined,
        [Description("Csv")] Csv,
        [Description("Tab Delimited")] TabDelimited
    }
}