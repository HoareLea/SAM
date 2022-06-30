using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Space> Spaces(this AdjacencyCluster adjacencyCluster, string ventilationUnitName, out List<Space> spaces_Supply, out List<Space> spaces_Exhaust)
        {
            spaces_Supply = null;
            spaces_Exhaust = null;

            if(adjacencyCluster == null || string.IsNullOrWhiteSpace(ventilationUnitName))
            {
                return null;
            }

            List<VentilationSystem> ventilationSystems = adjacencyCluster.VentilationSystems(ventilationUnitName, out List<VentilationSystem> ventilationSystems_Supply, out List<VentilationSystem> ventilationSystems_Exhaust);
            if(ventilationSystems == null || ventilationSystems.Count == 0)
            {
                return null;
            }

            List<Space> result = new List<Space>();

            spaces_Supply = new List<Space>();
            foreach (VentilationSystem ventilationSystem in ventilationSystems_Supply)
            {
                List<Space> spaces_VentilationSystem = adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem);
                if(spaces_VentilationSystem != null)
                {
                    foreach(Space space in spaces_VentilationSystem)
                    {
                        if(spaces_Supply.Find(x => x.Guid == space.Guid) == null)
                        {
                            spaces_Supply.Add(space);
                            if (result.Find(x => x.Guid == space.Guid) == null)
                            {
                                result.Add(space);
                            }
                        }


                    }
                }
            }

            spaces_Exhaust = new List<Space>();
            foreach (VentilationSystem ventilationSystem in ventilationSystems_Exhaust)
            {
                List<Space> spaces_VentilationSystem = adjacencyCluster.GetRelatedObjects<Space>(ventilationSystem);
                if (spaces_VentilationSystem != null)
                {
                    foreach (Space space in spaces_VentilationSystem)
                    {
                        if (spaces_Exhaust.Find(x => x.Guid == space.Guid) == null)
                        {
                            spaces_Exhaust.Add(space);
                            if (result.Find(x => x.Guid == space.Guid) == null)
                            {
                                result.Add(space);
                            }
                        }
                    }
                }
            }

            return result;
        }

        public static List<Space> Spaces(this AdjacencyCluster adjacencyCluster, string ventilationUnitName)
        {
            return Spaces(adjacencyCluster, ventilationUnitName, out List<Space> spaces_Supply, out List<Space> spaces_Exhaust);
        }
    }
}