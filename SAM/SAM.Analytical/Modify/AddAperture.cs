using System;
using System.Collections.Generic;
using SAM.Geometry.Spatial;


namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static Aperture AddAperture(this Panel panel, ApertureConstruction apertureConstruction, Point3D point3D, double maxDistance = Geometry.Tolerance.MacroDistance)
        {
            if (panel == null || apertureConstruction == null || point3D == null)
                return null;

            Plane plane = panel.PlanarBoundary3D.Plane;
            if (plane == null)
                return null;

            Point3D point3D_Projected = plane.Project(point3D);
            if (point3D_Projected.Distance(point3D) > maxDistance)
                return null;

            return panel.AddAperture(apertureConstruction, point3D_Projected);
        }

        public static Aperture AddAperture(this Panel panel, string name, ApertureType apertureType, IClosedPlanar3D closedPlanar3D, double maxDistance = Geometry.Tolerance.MacroDistance)
        {
            if (panel == null || closedPlanar3D == null)
                return null;

            Plane plane_Panel = panel.PlanarBoundary3D.Plane;
            if (plane_Panel == null)
                return null;

            Plane plane_ClosedPlanar3D = closedPlanar3D.GetPlane();
            if (plane_ClosedPlanar3D == null)
                return null;

            Point3D origin_ClosedPlanar3D = plane_ClosedPlanar3D.Origin;

            Point3D point3D_Closest = Geometry.Spatial.Query.Closest(panel.GetFace3D(), origin_ClosedPlanar3D);
            if (point3D_Closest.Distance(origin_ClosedPlanar3D) > maxDistance)
                return null;

            ApertureConstruction apertureConstruction = Create.ApertureConstruction(plane_Panel, name, apertureType, closedPlanar3D, maxDistance);
            if (apertureConstruction == null)
                return null;

            Point3D origin_Projected = plane_Panel.Project(origin_ClosedPlanar3D);
            return AddAperture(panel, apertureConstruction, origin_Projected, maxDistance);
        }

        public static Aperture AddAperture(this IEnumerable<Panel> panels, string name, ApertureType apertureType, IClosedPlanar3D edges, double maxDistance = Geometry.Tolerance.MacroDistance)
        {
            foreach(Panel panel in panels)
            {
                Aperture aperture = panel.AddAperture(name, apertureType, edges, maxDistance);
                if(aperture != null)
                    return aperture;
            }

            return null;
        }
    }
}
