using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Dictionary<IPanel, Vector3D> NormalDictionary(this AdjacencyCluster adjacencyCluster, ISpace space, out Shell shell, bool external = true, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            shell = null;

            if (adjacencyCluster == null || space == null)
            {
                return null;
            }

            List<IPanel> panels = adjacencyCluster.GetRelatedObjects<IPanel>(space);
            if (panels == null)
            {
                return null;
            }

            List<Face3D> face3Ds = panels.ConvertAll(x => x.Face3D);

            shell = new Shell(face3Ds);

            Dictionary<IPanel, Vector3D> result = new Dictionary<IPanel, Vector3D>();
            for(int i=0; i < face3Ds.Count(); i++)
            {
                result[panels[i]] = shell.Normal(face3Ds[i].InternalPoint3D(), external, silverSpacing, tolerance);
            }

            return result;
        }
    }
}