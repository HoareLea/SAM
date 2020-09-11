using System.ComponentModel;

namespace SAM.Core
{
    [Description("Material Type")]
    public enum MaterialType
    {
        [Description("Undefined Material Type")]
        Undefined,
        [Description("Gas Material Type")]
        Gas,
        [Description("Opaque Material Type")]
        Opaque,
        [Description("Transparent Material Type")]
        Transparent,
    }
}