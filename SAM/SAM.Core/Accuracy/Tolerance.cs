namespace SAM.Core
{
    /// <summary>
    /// This class provides methods for calculating tolerance values.
    /// </summary>
    public static class Tolerance
    {
        /// <summary>
        /// Micro dictance tolerance.
        /// </summary>
        public const double MicroDistance = 1e-9;

        /// <summary>
        /// Dictance tolerance.
        /// </summary>
        public const double Distance = 1e-6;

        /// <summary>
        /// Macro dictance tolerance.
        /// </summary>
        public const double MacroDistance = 1e-3;

        /// <summary>
        /// Angle tolerance. Equivalent of 2 degrees
        /// </summary>
        public const double Angle = 0.0349066;
    }
}