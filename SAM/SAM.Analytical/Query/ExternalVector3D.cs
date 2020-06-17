using SAM.Geometry.Spatial;
using System.Collections.Generic;

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
            if (face3D == null)
                return null;

            Shell shell = Query.Shell(adjacencyCluster, space);
            if (shell == null)
                return null;

            return shell.Normal(face3D.InternalPoint3D(), true, silverSpacing, tolerance);          
        }
    }
}