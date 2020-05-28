using Rhino.Geometry;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Convert
    {
        public static List<Brep> ToRhino(this AdjacencyCluster adjacencyCluster, double tolerance = Core.Tolerance.Distance)
        {
            if (adjacencyCluster == null)
                return null;

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels == null)
                panels = new List<Panel>();

            List<Brep> result = new List<Brep>();

            List<Space> spaces = adjacencyCluster.GetSpaces();
            if (spaces != null && spaces.Count > 0)
            {
                foreach (Space space in spaces)
                {

                    List<Panel> panels_Related = adjacencyCluster.GetRelatedObjects<Panel>(space);
                    if (panels_Related == null || panels_Related.Count == 0)
                        continue;

                    panels.RemoveAll(x => panels_Related.Contains(x));
                    List<Brep> breps = new List<Brep>();
                    foreach (Panel panel in panels_Related)
                    {
                        Brep brep = panel.ToRhino();
                        if (brep == null)
                            continue;

                        breps.Add(brep);
                    }

                    if (breps == null || breps.Count == 0)
                        continue;

                    Brep[] breps_Join = Brep.JoinBreps(breps, tolerance);

                    if (breps_Join != null)
                        result.AddRange(breps_Join);
                }
            }

            foreach (Panel panel in panels)
            {
                Brep brep = panel.ToRhino();
                if (brep == null)
                    continue;

                result.Add(brep);
            }

            return result;
        }
    }
}