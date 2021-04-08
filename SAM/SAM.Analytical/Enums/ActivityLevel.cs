using System.ComponentModel;

namespace SAM.Analytical
{
    /// <summary>
    /// Analytical ActivityLevel according to VDI 2078 2015.
    /// </summary>
    [Description("Analytical ActivityLevel according to VDI 2078 2015.")]
    public enum ActivityLevel
    {
        /// <summary>
        /// Undefined ActivityLevel
        /// </summary>
        [Description("Undefined")] Undefined,
        
        /// <summary>
        /// Sitting, relaxed 100W/person in total
        /// </summary>
        [Description("Activity Level I")] First,

        /// <summary>
        /// Sitting activity (office, school, lab) 125W/person in total
        /// </summary>
        [Description("Activity Level II")] Second,

        /// <summary>
        /// Standing, light activity (shop, lab, light industry) 170W/person in total
        /// </summary>
        [Description("Activity Level III")] Third,

        /// <summary>
        /// Standing, moderate activity (lab assistant, working with machinery) 210W/person in total
        /// </summary>
        [Description("Activity Level IV")] Fourth,
    }
}