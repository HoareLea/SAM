namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double CalculatedSupplyAirflow(this AdjacencyCluster adjacencyCluster, Zone zone)
        {
            if (adjacencyCluster == null || zone == null)
                return double.NaN;

            return adjacencyCluster.Sum(zone, SpaceParameter.SupplyAirFlow);
        }
    }
}