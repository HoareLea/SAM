using Grasshopper.Kernel.Types;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Convert
    {
        public static List<GH_Brep> ToGrasshopper(this AdjacencyCluster adjacencyCluster, bool cutApertures = false, bool includeFrame = false, double tolerance = Core.Tolerance.Distance)
        {

            List<global::Rhino.Geometry.Brep> breps = Rhino.Convert.ToRhino(adjacencyCluster, cutApertures, includeFrame, tolerance);
            if (breps == null || breps.Count == 0)
                return null;

            return breps.ConvertAll(x => new GH_Brep(x));
        }
    }
}