namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double CalculatedExhaustAirflow(this AdjacencyCluster adjacencyCluster, Zone zone)
        {
            if (adjacencyCluster == null || zone == null)
                return double.NaN;

            return adjacencyCluster.Sum(zone, SpaceParameter.ExhaustAirFlow);
        }
    }
}