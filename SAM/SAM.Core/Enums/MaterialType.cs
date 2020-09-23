using System.ComponentModel;

namespace SAM.Core
{
    [Description("Material Type")]
    public enum MaterialType
    {
        [Description("Undefined Material")] Undefined,
        [Description("Gas Material")] Gas,
        [Description("Opaque Material")] Opaque,
        [Description("Transparent Material")] Transparent,
    }
}