using System.Collections.Generic;
namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static VentilationSystem AddVentilationSystem(this AdjacencyCluster adjacencyCluster, VentilationSystemType ventilationSystemType, IEnumerable<Space> spaces = null, string supplyUnitName = null, string exhaustUnitName = null)
        {
            if (adjacencyCluster == null || ventilationSystemType == null)
                return null;

            VentilationSystem ventilationSystem = AddMechanicalSystem(adjacencyCluster, ventilationSystemType, spaces) as VentilationSystem;
            if (ventilationSystem == null)
                return null;

            if (supplyUnitName != null)
                ventilationSystem.SetValue(VentilationSystemParameter.SupplyUnitName, supplyUnitName);

            if (exhaustUnitName != null)
                ventilationSystem.SetValue(VentilationSystemParameter.ExhaustUnitName, exhaustUnitName);

            adjacencyCluster.AddObject(ventilationSystem);

            return ventilationSystem;
        }
    }
}