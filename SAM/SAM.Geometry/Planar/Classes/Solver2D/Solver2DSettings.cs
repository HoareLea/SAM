namespace SAM.Geometry.Planar
{
    public class Solver2DSettings
    {
        /// <summary>
        /// Distance of subsequent rectangle shifts in polyline variant
        /// </summary>
        public double MoveDistancePolyLine { get; set; } = 0.1;
        /// <summary>
        /// Number of attempts to subsequent rectangle shifts in polyline variant
        /// </summary>
        public double MaxStepPolyline { get; set; } = 200;
        /// <summary>
        /// Number of attempts to subsequent rectangle shifts in point variant
        /// </summary>
        public double MaxStepPoint { get; set; } = 10;
        /// <summary>
        /// Default distance from point to the center of IClosed2D
        /// </summary>
        public double DefaultDistancePoint = 0.5;

    }
}
