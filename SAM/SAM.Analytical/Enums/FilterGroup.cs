using System.ComponentModel;

namespace SAM.Analytical
{
    /// <summary>
    /// Analytical Aperture Type
    /// </summary>
    [Description("ISO 16890 Filter Group")]
    public enum FilterGroup
    {
        /// <summary>
        /// Undefined Filter Group
        /// </summary>
        [Description("Undefined")] Undefined,

        /// <summary>
        /// Other Filter Group
        /// </summary>
        [Description("Other")] Other,

        /// <summary>
        /// ISO ePM1 Filter Group
        /// </summary>
        [Description("ISO ePM1")] ePM1,

        /// <summary>
        /// ISO ePM2,5 Filter Group
        /// </summary>
        [Description("ISO ePM2,5")] ePM25,

        /// <summary>
        /// ISO ePM10 Filter Group
        /// </summary>
        [Description("ISO ePM10")] ePM10,

        /// <summary>
        /// ISO Coarse Filter Group
        /// </summary>
        [Description("ISO Coarse")] Coarse,
    }
}