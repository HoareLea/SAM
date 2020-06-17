using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Vector3D ExternalVector3D(this AdjacencyCluster adjacencyCluster, Space space, Panel panel, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (adjacencyCluster == null || panel == null || space == null)
                return null;

            List<Panel> panels = adjacencyCluster.GetRelatedObjects<Panel>(space);
            if (panels.Find(x => x.Guid == panel.Guid) == null)
                return null;

            Face3D face3D = panel.GetFace3D();
            Vector3D vector3D = face3D?.GetPlane()?.Normal;
            if (vector3D == null)
                return null;

            vector3D *= silverSpacing;

            Point3D point3D = face3D.GetInternalPoint3D();

            Point3D point3D_Move = point3D.GetMoved(vector3D) as Point3D;
            if (adjacencyCluster.Inside(space, point3D_Move, silverSpacing, tolerance))
                vector3D.Negate();

            return vector3D.Unit;           
        }
    }
}