using SAM.Geometry;
using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool Overlap(this Panel panel_1, Panel panel_2, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (panel_1 == panel_2)
                return true;

            Face3D face3D_1 = panel_1?.GetFace3D();
            if (face3D_1 == null)
                return false;

            Face3D face3D_2 = panel_2?.GetFace3D();
            if (face3D_2 == null)
                return false;

            return Geometry.Spatial.Query.Overlap(face3D_1, face3D_2, tolerance_Angle, tolerance_Distance);
        }
    }
}