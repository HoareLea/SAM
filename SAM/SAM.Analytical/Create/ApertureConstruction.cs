using System;

using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static ApertureConstruction ApertureConstruction(this Plane plane, string name, ApertureType apertureType, IClosedPlanar3D edge, double maxDistance = Geometry.Tolerance.MacroDistance)
        {
            if (plane == null || edge == null)
                return null;

            Plane plane_Edge = edge.GetPlane();
            if (plane_Edge == null)
                return null;

            Point3D origin_Edge = plane_Edge.Origin;

            Point3D origin_Projected = plane.Project(origin_Edge);
            if (origin_Projected.Distance(origin_Edge) > maxDistance)
                return null;

            IClosedPlanar3D edge_Projected = plane.Project(edge);
            Plane plane_Edge_Projected = edge_Projected.GetPlane();
            origin_Projected = plane_Edge_Projected.Origin;

            Vector3D vector3D = new Vector3D(plane.Origin.X - origin_Projected.X, plane.Origin.Y - origin_Projected.Y, plane.Origin.Z - origin_Projected.Z);
            edge_Projected = (IClosedPlanar3D)edge_Projected.GetMoved(vector3D);

            return new ApertureConstruction(name, plane.Convert(edge_Projected), apertureType);
        }
    }
}
