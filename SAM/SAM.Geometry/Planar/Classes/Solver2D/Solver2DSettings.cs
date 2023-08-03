using SAM.Core;

namespace SAM.Geometry.Planar
{
    public class Solver2DSettings
    {
        /// <summary>
        /// Distance of subsequent rectangle shifts
        /// </summary>
        public double MoveDistance { get; set; } = 0.1;
        /// <summary>
        /// Number of subsequent rectangle shifts in polyline variant
        /// </summary>
        public double MaxStepPolyline { get; set; } = 200;
        /// <summary>
        /// Number of subsequent rectangle shifts in point variant
        /// </summary>
        public double MaxStepPoint { get; set; } = 10;
    }
}
