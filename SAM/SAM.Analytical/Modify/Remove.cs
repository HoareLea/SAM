using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<System.Guid> Remove(this AdjacencyCluster adjacencyCluster, System.Type type, IEnumerable<System.Guid> guids)
        {
            if (adjacencyCluster == null || type == null || guids == null)
                return null;

            HashSet<System.Guid> guids_Temp = new HashSet<System.Guid>();
            foreach (System.Guid guid in guids)
                guids_Temp.Add(guid);

            List<System.Guid> result = new List<System.Guid>();

            if (type.IsAssignableFrom(typeof(Aperture)))
            {
                List<Panel> panels = adjacencyCluster.GetPanels();
                if (panels == null || panels.Count == 0)
                    return result;

                foreach(Panel panel in panels)
                {
                    HashSet<System.Guid> guids_Removed = new HashSet<System.Guid>();
                    foreach(System.Guid guid in guids_Temp)
                    {
                        if(panel.RemoveAperture(guid))
                            guids_Removed.Add(guid);
                    }

                    foreach (System.Guid guid in guids_Removed)
                    {
                        guids_Temp.Remove(guid);
                        result.Add(guid);
                    }
                }
            }
            else
            {
                foreach(System.Guid guid in guids_Temp)
                {
                    if(adjacencyCluster.RemoveObject(type, guid))
                        result.Add(guid);
                }
            }

            return result;
        }
    }
}