namespace SAM.Geometry.Planar
{
    public class Solver2DSettings
    {
        /// <summary>
        /// Starting distance between object to move and object around which it moves.
        /// </summary>
        public double StartingDistance { get; set; } = 0;

        /// <summary>
        /// Distance of every shift of moveable object.
        /// </summary>
        public double ShiftDistance { get; set; } = 1;

        /// <summary>
        /// Number of object shift attempts.
        /// </summary>
        public double IterationCount { get; set; } = 10;

        /// <summary>
        /// Area where object may be shifted. Object's center must be inside limit area.
        /// </summary>
        public IClosed2D LimitArea { get; set; } = null;
    }
}
