using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool Rectangular(this Panel panel, double tolerance = Core.Tolerance.Distance)
        {
            Face3D face3D = panel?.GetFace3D();
            if (face3D == null || !face3D.IsValid())
                return false;

            return Geometry.Spatial.Query.Rectangular(face3D, tolerance);
        }

        public static bool Rectangular(this Aperture aperture, double tolerance = Core.Tolerance.Distance)
        {
            Face3D face3D = aperture?.GetFace3D();
            if (face3D == null || !face3D.IsValid())
                return false;

            return Geometry.Spatial.Query.Rectangular(face3D, tolerance);
        }
    }
}