using System;
using System.Collections.Generic;
using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static ApertureConstruction ApertureConstruction(this Plane plane, string name, ApertureType apertureType, IClosedPlanar3D closedPlanar3D, double maxDistance = Geometry.Tolerance.MacroDistance)
        {
            if (plane == null || closedPlanar3D == null)
                return null;

            Plane plane_closedPlanar3D = closedPlanar3D.GetPlane();
            if (plane_closedPlanar3D == null)
                return null;

            Point3D origin_closedPlanar3D = plane_closedPlanar3D.Origin;

            Point3D origin_Projected = plane.Project(origin_closedPlanar3D);
            if (origin_Projected.Distance(origin_closedPlanar3D) > maxDistance)
                return null;

            if (closedPlanar3D is Face3D)
            {
                Face3D face3D = (Face3D)closedPlanar3D;

                IClosedPlanar3D externalEdge = GetClosedPlanar3D_Projected(plane, face3D.GetExternalEdge());

                List<IClosedPlanar3D> internalEdges = face3D.GetInternalEdges();
                if (internalEdges == null)
                    internalEdges = new List<IClosedPlanar3D>();

                Boundary2D boundary2D = new Boundary2D(Face3D.Create(plane, plane.Convert(externalEdge), internalEdges.ConvertAll(x => plane.Convert(x))));
                return new ApertureConstruction(name, boundary2D, apertureType);
            }
            else
            {
                IClosedPlanar3D closedPlanar3D_Projected = GetClosedPlanar3D_Projected(plane, closedPlanar3D);
                return new ApertureConstruction(name, plane.Convert(closedPlanar3D_Projected), apertureType);
            }
        }

        private static IClosedPlanar3D GetClosedPlanar3D_Projected(Plane plane, IClosedPlanar3D closedPlanar3D)
        {
            Plane plane_closedPlanar3D = closedPlanar3D.GetPlane();
            if (plane_closedPlanar3D == null)
                return null;

            Point3D origin_closedPlanar3D = plane_closedPlanar3D.Origin;

            Point3D origin_Projected = plane.Project(origin_closedPlanar3D);
            if (origin_Projected == null)
                return null;

            IClosedPlanar3D closedPlanar3D_Projected = plane.Project(closedPlanar3D);
            Plane plane_closedPlanar3D_Projected = closedPlanar3D_Projected.GetPlane();
            origin_Projected = plane_closedPlanar3D_Projected.Origin;

            Vector3D vector3D = new Vector3D(plane.Origin.X - origin_Projected.X, plane.Origin.Y - origin_Projected.Y, plane.Origin.Z - origin_Projected.Z);
            return (IClosedPlanar3D)closedPlanar3D_Projected.GetMoved(vector3D);
        }
    }
}
