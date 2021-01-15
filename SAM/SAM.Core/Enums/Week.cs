using System.ComponentModel;

namespace SAM.Core
{
    [Description("The days of the week")]
    public enum Week
    {
        [Description("Undefined")] Undefined,
        [Description("Monday")] Monday,
        [Description("Tuesday")] Tuesday,
        [Description("Wednesday")] Wednesday,
        [Description("Thursday")] Thursday,
        [Description("Friday")] Friday,
        [Description("Saturday")] Saturday,
        [Description("Sunday")] Sunday,
    }
}