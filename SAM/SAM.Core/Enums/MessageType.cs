using System.ComponentModel;

namespace SAM.Core
{
    [Description("Message Type")]
    public enum MessageType
    {
        [Description("Undefined")] Undefined,
        [Description("Information")] Information,
        [Description("Warning")] Warning,
        [Description("Error")] Error
    }
}