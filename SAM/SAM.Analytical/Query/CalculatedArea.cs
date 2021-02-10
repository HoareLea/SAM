namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double CalculatedArea(this Space space, AdjacencyCluster adjacencyCluster = null)
        {
            if (space.TryGetValue(SpaceParameter.Area, out double result) && !double.IsNaN(result))
                return result;

            return double.NaN;
        }
    }
}