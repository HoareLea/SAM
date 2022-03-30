using Rhino;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Modify
    {
        public static void BakeGeometry_ByBoundaryType(this RhinoDoc rhinoDoc, global::Grasshopper.Kernel.Data.IGH_Structure gH_Structure, bool cutApertures = false, double tolerance = Core.Tolerance.Distance)
        {
            if (rhinoDoc == null)
                return;

            List<AdjacencyCluster> adjacencyClusters = new List<AdjacencyCluster>();
            foreach (var variable in gH_Structure.AllData(true))
            {
                if (variable is GooAdjacencyCluster)
                {
                    AdjacencyCluster adjacencyCluster = ((GooAdjacencyCluster)variable).Value;
                    if (adjacencyCluster != null)
                        adjacencyClusters.Add(adjacencyCluster);
                }
                else if (variable is GooAnalyticalModel)
                {
                    AdjacencyCluster adjacencyCluster = ((GooAnalyticalModel)variable).Value?.AdjacencyCluster;
                    if (adjacencyCluster != null)
                        adjacencyClusters.Add(adjacencyCluster);
                }
            }

            foreach(AdjacencyCluster adjacencyCluster_Temp in adjacencyClusters)
            {
                Rhino.Modify.BakeGeometry_ByBoundaryType(rhinoDoc, adjacencyCluster_Temp, cutApertures, tolerance);
            }
        }
  }
}