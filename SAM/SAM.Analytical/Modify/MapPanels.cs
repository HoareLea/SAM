using SAM.Core;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Panel> MapPanels(this AdjacencyCluster adjacencyCluster, IEnumerable<Panel> panels, double minArea = Tolerance.MacroDistance, double maxDistance = Tolerance.MacroDistance)
        {
            if (adjacencyCluster == null || panels == null)
                return null;

            List<Panel> panels_AdjacencyCluster = adjacencyCluster.GetPanels();
            if (panels_AdjacencyCluster == null)
                return null;

            List<Panel> result = new List<Panel>();

            if (panels.Count() == 0 || panels_AdjacencyCluster.Count == 0)
                return result;

            List<Tuple<Point3D, Panel>> tuples = new List<Tuple<Point3D, Panel>>();
            foreach(Panel panel_AdjacencyCluster in panels_AdjacencyCluster)
            {
                Point3D point3D = panel_AdjacencyCluster?.GetInternalPoint3D();
                if (point3D == null)
                    continue;

                tuples.Add(new Tuple<Point3D, Panel>(point3D, panel_AdjacencyCluster));
            }

            foreach(Panel panel in panels)
            {
                Face3D face3D = panel?.GetFace3D();
                if (face3D == null)
                    continue;

                List<Panel> panels_Panel = new List<Panel>();
                foreach(Tuple<Point3D, Panel> tuple in tuples)
                {
                    double distance = face3D.Distance(tuple.Item1);
                    if (distance <= maxDistance)
                        panels_Panel.Add(tuple.Item2);
                }

                if (panels_Panel == null || panels_Panel.Count == 0)
                    continue;

                for(int i =0; i < panels_Panel.Count; i++)
                {
                    Panel panel_Panel = panels_Panel[i];

                    panel_Panel = new Panel(panel_Panel.Guid, panel, panel_Panel.GetFace3D(), null, true, minArea, maxDistance);
                    adjacencyCluster.AddObject(panel_Panel);
                    result.Add(panel_Panel);
                }

            }

            return result;
        }
    }
}