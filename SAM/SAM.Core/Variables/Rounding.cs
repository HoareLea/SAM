namespace SAM.Core
{
    /// <summary>
    /// Represents a static class that defines various levels of rounding precision for distances and angles.
    /// </summary>
    public static class Rounding
    {
        /// <summary>
        /// Rounding precision for micro distances (9 decimal places).
        /// </summary>
        public const int MicroDistance = 9;

        /// <summary>
        /// Rounding precision for distances (6 decimal places).
        /// </summary>
        public const int Distance = 6;

        /// <summary>
        /// Rounding precision for macro distances (3 decimal places).
        /// </summary>
        public const int MacroDistance = 3;

        /// <summary>
        /// Rounding precision for angles (6 decimal places).
        /// </summary>
        public const int Angle = 6;

        /// <summary>
        /// Represents no rounding (value: -1).
        /// </summary>
        public const int NoRounding = -1;
    }
}
