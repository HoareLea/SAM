using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Dictionary<Panel, Vector3D> NormalDictionary(this AdjacencyCluster adjacencyCluster, Space space, out Shell shell, bool external = true, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            shell = null;

            if (adjacencyCluster == null || space == null)
                return null;

            List<Panel> panels = adjacencyCluster.GetPanels(space);
            if (panels == null)
                return null;

            List<Face3D> face3Ds = panels.ConvertAll(x => x.GetFace3D(false, tolerance));

            shell = new Shell(face3Ds);

            Dictionary<Panel, Vector3D> result = new Dictionary<Panel, Vector3D>();
            for(int i=0; i < face3Ds.Count(); i++)
                result[panels[i]] = shell.Normal(face3Ds[i].InternalPoint3D(), external, silverSpacing, tolerance);

            return result;
        }
    }
}