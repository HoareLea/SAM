using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static AdjacencyCluster UpdateNormals(this AdjacencyCluster adjacencyCluster, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance= Core.Tolerance.Distance)
        {
            if (adjacencyCluster == null)
                return null;

            AdjacencyCluster result = new AdjacencyCluster(adjacencyCluster);

            List<Space> spaces = result.GetSpaces();
            if (spaces == null || spaces.Count == 0)
                return result;


            HashSet<System.Guid> guids = new HashSet<System.Guid>();
            foreach(Space space in spaces)
            {
                Shell shell = null;
                Dictionary<Panel, Vector3D> dictionary = adjacencyCluster.NormalDictionary(space, out shell, true, silverSpacing, tolerance);
                if (dictionary == null)
                    continue;

                foreach(KeyValuePair<Panel, Vector3D> keyValuePair in dictionary)
                {
                    Panel panel = keyValuePair.Key;
                    if (panel == null)
                        continue;

                    if (guids.Contains(panel.Guid))
                        continue;

                    guids.Add(panel.Guid);

                    Vector3D normal_External = keyValuePair.Value;
                    if (normal_External == null)
                        continue;

                    Vector3D normal_Panel = panel.Plane?.Normal;
                    if (normal_Panel == null)
                        continue;

                    if (normal_External.SameHalf(normal_Panel))
                        continue;

                    panel = new Panel(panel);
                    panel.FlipNormal();

                    adjacencyCluster.AddObject(panel);
                }
            }

            return result;
        }
    }
}