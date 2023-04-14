using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Zone> CopyZones(this AdjacencyCluster adjacencyCluster_Destination, AdjacencyCluster adjacencyCluster_Source, IEnumerable<Guid> zoneGuids)
        {
            if(adjacencyCluster_Destination == null || adjacencyCluster_Source == null || zoneGuids == null)
            {
                return null;
            }

            List<Zone> result = new List<Zone>();

            Dictionary<Space, Shell> dictionary_Destination = adjacencyCluster_Destination?.ShellDictionary();

            foreach (Guid zoneGuid in zoneGuids)
            {
                Zone zone_Source = adjacencyCluster_Source.GetObject<Zone>(zoneGuid);
                if(zone_Source == null)
                {
                    continue;
                }

                List<Space> spaces_Destination = new List<Space>();

                if(dictionary_Destination != null)
                {
                    List<Space> spaces_Source = adjacencyCluster_Source.GetRelatedObjects<Space>(zone_Source);
                    if (spaces_Source != null && spaces_Source.Count != 0)
                    {
                        foreach (Space space_Source in spaces_Source)
                        {
                            Point3D point3D = space_Source?.Location;
                            if (point3D == null)
                            {
                                continue;
                            }

                            foreach (KeyValuePair<Space, Shell> keyValuePair in dictionary_Destination)
                            {
                                if (keyValuePair.Value.InRange(point3D) || keyValuePair.Value.Inside(point3D))
                                {
                                    spaces_Destination.Add(keyValuePair.Key);
                                    break;
                                }
                            }


                        }
                    }
                }

                if(!zone_Source.TryGetValue(ZoneParameter.ZoneCategory, out string zoneCategory))
                {
                    zoneCategory = null;
                }

                Zone zone_Destination = adjacencyCluster_Destination.UpdateZone(zone_Source.Name, zoneCategory, spaces_Destination.ToArray());
                if(zone_Destination != null)
                {
                    result.Add(zone_Destination);
                }
            }

            return result;
        }
    }
}