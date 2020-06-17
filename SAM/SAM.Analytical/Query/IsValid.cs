using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool IsValid(this Panel panel, Aperture aperture)
        {
            if (panel == null || aperture == null)
                return false;

            PlanarBoundary3D planarBoundary3D_Panel = panel.PlanarBoundary3D;
            if (planarBoundary3D_Panel == null)
                return false;

            PlanarBoundary3D planarBoundary3D_Aperture = aperture.PlanarBoundary3D;
            if (planarBoundary3D_Aperture == null)
                return false;

            Plane plane_Panel = planarBoundary3D_Panel.Plane;
            if (plane_Panel == null)
                return false;

            Plane plane_Aperture = planarBoundary3D_Aperture.Plane;
            if (plane_Aperture == null)
                return false;

            if (!planarBoundary3D_Aperture.Coplanar(planarBoundary3D_Panel))
                return false;

            Face3D face3D_Panel = planarBoundary3D_Panel.GetFace3D();
            if (face3D_Panel == null)
                return false;

            Face3D face3D_Aperture = planarBoundary3D_Aperture.GetFace3D();
            if (face3D_Aperture == null)
                return false;

            if (face3D_Panel.Inside(face3D_Aperture.InternalPoint3D(), Core.Tolerance.Distance))
                return true;

            return false;
        }

        public static bool IsValid(this Panel panel, Aperture aperture, double maxDistance)
        {
            if (panel == null || aperture == null)
                return false;

            PlanarBoundary3D planarBoundary3D_Panel = panel.PlanarBoundary3D;
            if (planarBoundary3D_Panel == null)
                return false;

            IClosedPlanar3D closedPlanar3D = aperture.PlanarBoundary3D?.GetFace3D()?.GetExternalEdge();
            if (closedPlanar3D == null)
                return false;

            IClosedPlanar3D closedPlanar3D_Projected = planarBoundary3D_Panel.Plane.Project(closedPlanar3D);

            if (closedPlanar3D_Projected.GetPlane().Origin.Distance(closedPlanar3D.GetPlane().Origin) > maxDistance)
                return false;

            return true;
        }
    }
}