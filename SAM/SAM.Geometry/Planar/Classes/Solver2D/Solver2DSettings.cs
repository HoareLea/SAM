using SAM.Core;

namespace SAM.Geometry.Planar
{
    public class Solver2DSettings
    {
        public double moveDistance { get; set; } = 0.1;
        public double maxStepPolyline { get; set; } = 200;

        public double maxStepPoint { get; set; } = 10;
    }
}
