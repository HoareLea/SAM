using SAM.Core;
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

            if (typeof(Aperture).IsAssignableFrom(type))
            {
                List<Panel> panels = adjacencyCluster.GetPanels();
                if (panels == null || panels.Count == 0)
                    return result;

                foreach (Panel panel in panels)
                {
                    if (!panel.HasApertures)
                        continue;

                    List<System.Guid> guids_Apertures = new List<System.Guid>();
                    foreach (System.Guid guid in guids_Temp)
                        if (panel.HasAperture(guid))
                            guids_Apertures.Add(guid);

                    if (guids_Apertures.Count == 0)
                        continue;

                    HashSet<System.Guid> guids_Removed = new HashSet<System.Guid>();

                    Panel panel_New = new Panel(panel);
                    foreach (System.Guid guid in guids_Apertures)
                        if (panel_New.RemoveAperture(guid))
                            guids_Removed.Add(guid);

                    adjacencyCluster.AddObject(panel_New);

                    foreach (System.Guid guid in guids_Removed)
                    {
                        guids_Temp.Remove(guid);
                        result.Add(guid);
                    }
                }
            }
            else
            {
                foreach (System.Guid guid in guids_Temp)
                {
                    if (adjacencyCluster.RemoveObject(type, guid))
                        result.Add(guid);
                }
            }

            return result;
        }

        public static List<System.Guid> Remove(this AdjacencyCluster adjacencyCluster, IEnumerable<SAMObject> objects)
        {
            if (adjacencyCluster == null || objects == null)
                return null;

            Dictionary<System.Type, List<SAMObject>> dictionary = Core.Query.TypeDictionary(objects);
            if (dictionary == null)
                return null;

            List<System.Guid> result = new List<System.Guid>();
            foreach (KeyValuePair<System.Type, List<SAMObject>> keyValuePair in dictionary)
            {
                List<System.Guid> guids = Remove(adjacencyCluster, keyValuePair.Key, keyValuePair.Value.ConvertAll(x => x.Guid));
                if (guids != null && guids.Count != 0)
                    result.AddRange(guids);
            }

            return result;
        }

        public static List<System.Guid> Remove<T>(this AdjacencyCluster adjacencyCluster, IEnumerable<T> objects) where T : SAMObject
        {
            if (adjacencyCluster == null || objects == null)
                return null;

            return Remove(adjacencyCluster, objects.Cast<SAMObject>());
        }
    }
}