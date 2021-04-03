using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static Panel Extend(this Panel panel, Plane plane, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (panel == null || plane == null)
                return null;

            Face3D face3D = panel.GetFace3D()?.Extend(plane, tolerance_Angle, tolerance_Distance);
            if (face3D == null)
                return null;

            return new Panel(panel.Guid, panel, face3D);
        }
    }
}