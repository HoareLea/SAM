using System;
using System.Collections.Generic;
using SAM.Geometry.Spatial;


namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static Aperture AddAperture(this Panel panel, ApertureType apertureType, Point3D point3D, double maxDistance = Geometry.Tolerance.MacroDistance)
        {
            if (panel == null || apertureType == null || point3D == null)
                return null;

            Plane plane = panel.PlanarBoundary3D.Plane;
            if (plane == null)
                return null;

            Point3D point3D_Projected = plane.Project(point3D);
            if (point3D_Projected.Distance(point3D) > maxDistance)
                return null;


            return panel.AddAperture(apertureType, point3D_Projected);
        }

        public static Aperture AddAperture(this Panel panel, IClosedPlanar3D edges, double maxDistance = Geometry.Tolerance.MacroDistance)
        {
            if (panel == null || edges == null)
                return null;

            Plane plane_Panel = panel.PlanarBoundary3D.Plane;
            if (plane_Panel == null)
                return null;

            Plane plane_Edges = edges.GetPlane();
            if (plane_Edges == null)
                return null;

            Point3D origin_Edges = plane_Edges.Origin;

            Point3D point3D_Projected = plane_Panel.Project(origin_Edges);
            if (point3D_Projected.Distance(origin_Edges) > maxDistance)
                return null;

            ApertureType apertureType = new ApertureType(Guid.NewGuid().ToString(), plane_Panel.Convert(edges));

            return AddAperture(panel, apertureType, point3D_Projected, maxDistance);
        }

        public static Aperture AddAperture(this IEnumerable<Panel> panels, IClosedPlanar3D edges, double maxDistance = Geometry.Tolerance.MacroDistance)
        {
            foreach(Panel panel in panels)
            {
                Aperture aperture = panel.AddAperture(edges, maxDistance);
                if(aperture != null)
                    return aperture;
            }

            return null;
        }
    }
}
