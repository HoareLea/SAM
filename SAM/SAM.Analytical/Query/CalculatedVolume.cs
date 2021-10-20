namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double CalculatedVolume(this Space space, AdjacencyCluster adjacencyCluster = null)
        {
            if (space.TryGetValue(SpaceParameter.Volume, out double result) && !double.IsNaN(result))
                return result;

            if (adjacencyCluster == null)
                return double.NaN;

            return space.Volume(adjacencyCluster);
        }
    }
}