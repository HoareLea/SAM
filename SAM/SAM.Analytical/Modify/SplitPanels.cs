using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Panel> SplitPanels(this AdjacencyCluster adjacencyCluster, IEnumerable<ISAMGeometry3D> geometry3Ds, IEnumerable<Guid> panelGuids = null, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (adjacencyCluster == null || geometry3Ds == null)
            {
                return null;
            }

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels == null)
            {
                return null;
            }

            if (panelGuids != null)
            {
                panels.RemoveAll(x => !panelGuids.Contains(x.Guid));
            }

            List<Panel> result = new List<Panel>();

            if (geometry3Ds.Count() == 0 || panels.Count == 0)
            {
                return result;
            }

            foreach (Panel panel in panels)
            {
                Face3D face3D = panel?.Face3D;
                if (face3D == null)
                {
                    continue;
                }

                List<Face3D> face3Ds = Geometry.Spatial.Query.Split(face3D, geometry3Ds, tolerance_Angle, tolerance_Distance);
                if (face3Ds == null || face3Ds.Count < 2)
                {
                    continue;
                }

                for(int i=0; i < face3Ds.Count; i++)
                {
                    Guid guid = panel.Guid;
                    if(i > 0)
                    {
                        guid = Guid.NewGuid();
                    }

                    Panel panel_New = Create.Panel(guid, panel, new PlanarBoundary3D(face3Ds[i]));
                    result.Add(panel_New);

                    adjacencyCluster.AddObject(panel_New);

                    if(i > 0)
                    {
                        List<object> objects = adjacencyCluster.GetRelatedObjects(panel.Guid);
                        if(objects != null)
                        {
                            foreach(object @object in objects)
                            {
                                adjacencyCluster.AddRelation(panel_New, @object);
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}