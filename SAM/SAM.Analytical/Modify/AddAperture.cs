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

        public static Aperture AddAperture(this Panel panel, ApertureType apertureType, IClosedPlanar3D edge, double maxDistance = Geometry.Tolerance.MacroDistance)
        {
            if (panel == null || edge == null)
                return null;

            Plane plane_Panel = panel.PlanarBoundary3D.Plane;
            if (plane_Panel == null)
                return null;
                
            Plane plane_Edge = edge.GetPlane();
            if (plane_Edge == null)
                return null;

            Point3D origin_Edge = plane_Edge.Origin;

            Point3D origin_Projected = plane_Panel.Project(origin_Edge);
            if (origin_Projected.Distance(origin_Edge) > maxDistance)
                return null;

            IClosedPlanar3D edge_Projected = plane_Panel.Project(edge);
            Plane plane_Edge_Projected = edge_Projected.GetPlane();
            origin_Projected = plane_Edge_Projected.Origin;

            Vector3D vector3D = new Vector3D(plane_Panel.Origin.X - origin_Projected.X, plane_Panel.Origin.Y - origin_Projected.Y, plane_Panel.Origin.Z - origin_Projected.Z);
            edge_Projected = (IClosedPlanar3D)edge_Projected.GetMoved(vector3D);

            ApertureConstruction apertureConstruction = new ApertureConstruction(Guid.NewGuid().ToString(), plane_Panel.Convert(edge_Projected), apertureType);

            return AddAperture(panel, apertureConstruction, origin_Projected, maxDistance);
        }

        public static Aperture AddAperture(this IEnumerable<Panel> panels, ApertureType apertureType, IClosedPlanar3D edges, double maxDistance = Geometry.Tolerance.MacroDistance)
        {
            foreach(Panel panel in panels)
            {
                Aperture aperture = panel.AddAperture(apertureType, edges, maxDistance);
                if(aperture != null)
                    return aperture;
            }

            return null;
        }
    }
}
