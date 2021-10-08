using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> Panels(this AdjacencyCluster adjacencyCluster, Construction construction)
        {
            if (adjacencyCluster == null || construction == null)
                return null;

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels == null)
                return null;

            Guid guid = construction.Guid;

            List<Panel> result = new List<Panel>();
            foreach (Panel panel in panels)
                if (panel.TypeGuid.Equals(guid))
                    result.Add(panel);

            return result;
        }

        public static List<Panel> Panels(this Point3D point3D, IEnumerable<Panel> panels, bool onEdgeOnly = false, double tolerance = Core.Tolerance.Distance)
        {
            if(point3D == null || panels == null)
            {
                return null;
            }

            List<Panel> result = new List<Panel>();
            foreach (Panel panel in panels)
            {
                Face3D face3D = panel?.GetFace3D();
                if(face3D == null)
                {
                    continue;
                }

                bool add = false;

                if(onEdgeOnly)
                {
                    add = face3D.OnEdge(point3D, tolerance);
                }
                else
                {
                    add = face3D.On(point3D, tolerance) || face3D.OnEdge(point3D, tolerance);
                }

                if(add)
                {
                    result.Add(panel);
                }
            }

            return result;

        }
    }
}