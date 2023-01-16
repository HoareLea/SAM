using System.ComponentModel;

namespace SAM.Analytical
{
    /// <summary>
    /// TM59 Space Application
    /// </summary>
    public enum TM59SpaceApplication
    {
        [Description("Undefined")] Undefined,
        [Description("Sleeping")] Sleeping,
        [Description("Living")] Living,
        [Description("Cooking")] Cooking
    }
}
