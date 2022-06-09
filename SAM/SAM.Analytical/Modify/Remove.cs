using SAM.Core;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Guid> Remove(this AdjacencyCluster adjacencyCluster, Type type, IEnumerable<Guid> guids)
        {
            if (adjacencyCluster == null || type == null || guids == null)
                return null;

            HashSet<Guid> guids_Temp = new HashSet<Guid>();
            foreach (Guid guid in guids)
                guids_Temp.Add(guid);

            List<Guid> result = new List<Guid>();

            if (typeof(Aperture).IsAssignableFrom(type))
            {
                List<Panel> panels = adjacencyCluster.GetPanels();
                if (panels == null || panels.Count == 0)
                    return result;

                foreach (Panel panel in panels)
                {
                    if (!panel.HasApertures)
                        continue;

                    List<Guid> guids_Apertures = new List<Guid>();
                    foreach (Guid guid in guids_Temp)
                        if (panel.HasAperture(guid))
                            guids_Apertures.Add(guid);

                    if (guids_Apertures.Count == 0)
                        continue;

                    HashSet<Guid> guids_Removed = new HashSet<Guid>();

                    Panel panel_New = new Panel(panel);
                    foreach (Guid guid in guids_Apertures)
                        if (panel_New.RemoveAperture(guid))
                            guids_Removed.Add(guid);

                    adjacencyCluster.AddObject(panel_New);

                    foreach (Guid guid in guids_Removed)
                    {
                        guids_Temp.Remove(guid);
                        result.Add(guid);
                    }
                }
            }
            else
            {
                foreach (Guid guid in guids_Temp)
                {
                    if (adjacencyCluster.RemoveObject(type, guid))
                        result.Add(guid);
                }
            }

            return result;
        }

        public static List<Guid> Remove(this AdjacencyCluster adjacencyCluster, IEnumerable<SAMObject> objects)
        {
            if (adjacencyCluster == null || objects == null)
                return null;

            Dictionary<Type, List<SAMObject>> dictionary = Core.Query.TypeDictionary(objects);
            if (dictionary == null)
                return null;

            List<Guid> result = new List<Guid>();
            foreach (KeyValuePair<Type, List<SAMObject>> keyValuePair in dictionary)
            {
                if(keyValuePair.Key.IsAssignableFrom(typeof(Panel)))
                {
                    foreach(Panel panel in keyValuePair.Value)
                    {
                        List<Guid> guids_Panel = Remove(adjacencyCluster, panel);
                        if(guids_Panel != null)
                        {
                            result.AddRange(guids_Panel);
                        }
                    }
                }

                List<Guid> guids = Remove(adjacencyCluster, keyValuePair.Key, keyValuePair.Value.ConvertAll(x => x.Guid));
                if (guids != null && guids.Count != 0)
                    result.AddRange(guids);
            }

            return result;
        }

        public static List<Guid> Remove<T>(this AdjacencyCluster adjacencyCluster, IEnumerable<T> objects) where T : SAMObject
        {
            if (adjacencyCluster == null || objects == null)
                return null;

            return Remove(adjacencyCluster, objects.Cast<SAMObject>());
        }

        public static List<Guid> Remove(this AdjacencyCluster adjacencyCluster, Panel panel)
        {
            if(adjacencyCluster == null || panel == null)
            {
                return null;
            }

            Panel panel_AdjacencyCluster = adjacencyCluster.GetObject<Panel>(panel.Guid);
            if(panel_AdjacencyCluster == null)
            {
                return null;
            }

            List<Space> spaces = adjacencyCluster.GetRelatedObjects<Space>(panel_AdjacencyCluster);
            if(spaces != null && spaces.Count() != 0)
            {
                List<Tuple<Space, double>> tuples = new List<Tuple<Space, double>>();
                Dictionary<Guid, Panel> dictionary = new Dictionary<Guid, Panel>();
                foreach (Space space_Temp in spaces)
                {
                    Shell shell = adjacencyCluster.Shell(space_Temp);
                    if(shell == null)
                    {
                        continue;
                    }

                    double volume = shell.Volume();
                    if(double.IsNaN(volume))
                    {
                        continue;
                    }

                    tuples.Add(new Tuple<Space, double>(space_Temp, volume));

                    List<Panel> panels =  adjacencyCluster.GetRelatedObjects<Panel>(space_Temp);
                    foreach(Panel panel_Temp in panels)
                    {
                        if(panel_Temp == null)
                        {
                            continue;
                        }

                        if(panel_Temp.Guid == panel.Guid)
                        {
                            continue;
                        }

                        dictionary[panel_Temp.Guid] = panel_Temp;
                    }
                }

                Space space = tuples[0].Item1;
                tuples.Sort((x, y) => y.Item2.CompareTo(x.Item2));
                tuples.RemoveAt(0);

                foreach(Space space_Temp in tuples.ConvertAll(x => x.Item1))
                {
                    adjacencyCluster.RemoveObject<Space>(space_Temp.Guid);
                }

                foreach(Panel panel_Temp in dictionary.Values)
                {
                    adjacencyCluster.AddRelation(space, panel_Temp);
                }

            }

            return new List<Guid>() { panel.Guid };
        }
    }
}