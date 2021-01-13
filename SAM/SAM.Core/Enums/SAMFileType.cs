using System.ComponentModel;

namespace SAM.Core
{
    [Description("Base SAM File Types.")]
    public enum SAMFileType
    {
        [Description("Undefined")] Undefined,
        [Description("Json")] Json,
        [Description("SAM")] SAM,
    }
}