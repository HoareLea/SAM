using System.ComponentModel;

namespace SAM.Analytical
{
    /// <summary>
    /// SeasonType
    /// </summary>
    [Description("Season Type")]
    public enum SeasonType
    {
        /// <summary>
        /// Undefined
        /// </summary>
        [Description("Undefined")] Undefined,
        
        /// <summary>
        /// Cooling
        /// </summary>
        [Description("Cooling")] Cooling,

        /// <summary>
        /// Heating
        /// </summary>
        [Description("Heating")] Heating,

        /// <summary>
        /// Free Cooling
        /// </summary>
        [Description("Free Cooling")] FreeCooling,

    }
}