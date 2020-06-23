using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Dictionary<Space, Geometry.Spatial.Shell> ShellDictionary(this AdjacencyCluster adjacencyCluster)
        {
            if (adjacencyCluster == null)
                return null;

            List<Space> spaces = adjacencyCluster.GetSpaces();
            if (spaces == null)
                return null;


            Dictionary<Space, Geometry.Spatial.Shell> result = new Dictionary<Space, Geometry.Spatial.Shell>();
            Dictionary<System.Guid, Geometry.Spatial.Face3D> dictionary_Shell = new Dictionary<System.Guid, Geometry.Spatial.Face3D>();
            foreach (Space space in spaces)
            {
                List<Panel> panels = adjacencyCluster.GetRelatedObjects<Panel>(space);
                if (panels == null)
                    continue;

                List<Geometry.Spatial.Face3D> face3Ds = new List<Geometry.Spatial.Face3D>();
                foreach(Panel panel in panels)
                {
                    Geometry.Spatial.Face3D face3D = null;
                    if(!dictionary_Shell.TryGetValue(panel.Guid, out face3D))
                    {
                        face3D = panel.GetFace3D();
                        dictionary_Shell[panel.Guid] = face3D;
                    }

                    if (face3D != null)
                        face3Ds.Add(face3D);
                }

                result[space] = new Geometry.Spatial.Shell(face3Ds);
            }

            return result;
        }
    }
}