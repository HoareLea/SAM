using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<VentilationSystem> VentilationSystems(this AdjacencyCluster adjacencyCluster, string unitName, out List<VentilationSystem> ventilationSystems_Supply, out List<VentilationSystem> ventilationSystems_Exhaust)
        {
            ventilationSystems_Supply = null;
            ventilationSystems_Exhaust = null;

            if (adjacencyCluster == null || string.IsNullOrWhiteSpace(unitName))
            {
                return null;
            }

            List<VentilationSystem> ventilationSystems = adjacencyCluster.GetMechanicalSystems<VentilationSystem>();
            if (ventilationSystems == null || ventilationSystems.Count == 0)
            {
                return null;
            }

            List<VentilationSystem> result = new List<VentilationSystem>();

            ventilationSystems_Supply = new List<VentilationSystem>();
            ventilationSystems_Exhaust = new List<VentilationSystem>();
            foreach (VentilationSystem ventilationSystem in ventilationSystems)
            {
                if (ventilationSystem == null)
                {
                    continue;
                }

                string unitName_Temp = null;

                bool added = false;

                if (ventilationSystem.TryGetValue(VentilationSystemParameter.SupplyUnitName, out unitName_Temp) && !string.IsNullOrWhiteSpace(unitName_Temp))
                {
                    if (unitName == unitName_Temp)
                    {
                        ventilationSystems_Supply.Add(ventilationSystem);
                        added = true;
                    }
                }

                if (ventilationSystem.TryGetValue(VentilationSystemParameter.ExhaustUnitName, out unitName_Temp) && !string.IsNullOrWhiteSpace(unitName_Temp))
                {
                    if (unitName == unitName_Temp)
                    {
                        ventilationSystems_Exhaust.Add(ventilationSystem);
                        added = true;
                    }
                }

                if(added)
                {
                    result.Add(ventilationSystem);
                }

            }

            return result;
        }
    }
}