using Grasshopper.Kernel.Types;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Convert
    {
        public static List<GH_Brep> ToGrasshopper(this AdjacencyCluster adjacencyCluster, double tolerance = Core.Tolerance.Distance)
        {

            List<Rhino.Geometry.Brep> breps = adjacencyCluster?.ToRhino(tolerance);
            if (breps == null || breps.Count == 0)
                return null;

            return breps.ConvertAll(x => new GH_Brep(x));
        }
    }
}