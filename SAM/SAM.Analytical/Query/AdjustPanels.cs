using System;
using System.Collections.Generic;
using SAM.Geometry.Spatial;
using SAM.Geometry.Planar;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> AdjustPanels(this Face3D face3D, IEnumerable<Panel> panels, double maxDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (face3D == null || panels == null)
            {
                return null;
            }

            Plane plane = face3D.GetPlane();
            if (plane == null)
            {
                return null;
            }

            List<Point3D> point3Ds = new List<Point3D>();
            foreach (Panel panel in panels)
            {
                Face3D face3D_Panel = panel?.GetFace3D();
                if (face3D_Panel == null)
                {
                    continue;
                }

                ISegmentable3D segmentable3D = face3D_Panel.GetExternalEdge3D() as ISegmentable3D;
                if (segmentable3D == null)
                {
                    continue;
                }

                List<Point3D> point3Ds_Segmentable3D = segmentable3D.GetPoints();
                if (point3Ds_Segmentable3D != null)
                {
                    point3Ds.AddRange(point3Ds_Segmentable3D);
                }
            }

            List<Point3D> point3Ds_Snapped = new List<Point3D>();
            while (point3Ds.Count > 0)
            {
                Point3D point3D = point3Ds[0];
                List<Point3D> point3Ds_Snapped_Temp = point3Ds.FindAll(x => x.Distance(point3D) < maxDistance);

                point3Ds.RemoveAll(x => point3Ds_Snapped_Temp.Contains(x));
                if (point3Ds_Snapped_Temp.Count == 1)
                {
                    point3Ds_Snapped.Add(point3Ds_Snapped_Temp[0]);
                    continue;
                }
                else
                {
                    point3Ds_Snapped.Add(point3Ds_Snapped_Temp.Average());
                }
            }

            List<Triangle3D> triangle3Ds = face3D.Triangulate(point3Ds_Snapped, tolerance);
            if (triangle3Ds == null)
            {
                return new List<Panel>();
            }

            List<Panel> result = Enumerable.Repeat<Panel>(null, triangle3Ds.Count).ToList();

            Parallel.For(0, triangle3Ds.Count, (int i) =>
            {
                Point3D point3D = triangle3Ds[i]?.GetCentroid();
                if(point3D == null)
                {
                    return;
                }

                Panel panel = Closest(panels, point3D, tolerance);
                if (panel == null)
                {
                    return;
                }

                panel = Create.Panel(Guid.NewGuid(), panel, new Face3D(triangle3Ds[i]), null, true, tolerance, maxDistance);
                if (panel == null)
                {
                    return;
                }

                result[i] = panel;
            });

            result.RemoveAll(x => x == null);

            return result;
        }
    }
}