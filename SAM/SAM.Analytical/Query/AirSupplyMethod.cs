using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static AirSupplyMethod AirSupplyMethod(this AdjacencyCluster adjacencyCluster, string airHandlingUnitName)
        {
            List<VentilationSystem> ventilationSystems = adjacencyCluster?.VentilationSystems(airHandlingUnitName, out List<VentilationSystem> ventilationSystems_Supply, out List<VentilationSystem> ventilationSystems_Exhaust);

            return AirSupplyMethod(ventilationSystems, out VentilationSystemType ventilationSystemType);
        }

        public static AirSupplyMethod AirSupplyMethod(this AdjacencyCluster adjacencyCluster, Space space, out VentilationSystemType ventilationSystemType)
        {
            List<VentilationSystem> ventilationSystems = adjacencyCluster.GetRelatedObjects<VentilationSystem>(space);

            return AirSupplyMethod(ventilationSystems, out ventilationSystemType);
        }

        private static AirSupplyMethod AirSupplyMethod(this IEnumerable<VentilationSystem> ventilationSystems, out VentilationSystemType ventilationSystemType)
        {

            ventilationSystemType = ventilationSystems?.ToList().Find(x => x.Type != null)?.Type as VentilationSystemType;
            if (ventilationSystemType == null)
            {
                return Analytical.AirSupplyMethod.Undefined;
            }

            if (!ventilationSystemType.TryGetValue(VentilationSystemTypeParameter.AirSupplyMethod, out string airSupplyMethodString) || string.IsNullOrWhiteSpace(airSupplyMethodString))
            {
                return Analytical.AirSupplyMethod.Undefined;
            }

            return Core.Query.Enum<AirSupplyMethod>(airSupplyMethodString);
        }
    }
}