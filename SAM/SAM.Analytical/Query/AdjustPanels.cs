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

            Face2D face2D = plane.Convert(face3D);
            if(face2D == null)
            {
                return null;
            }

            List<Tuple<Panel, BoundingBox2D, Face2D>> tuples = new List<Tuple<Panel, BoundingBox2D, Face2D>>();
            List<Point2D> point2Ds = new List<Point2D>();
            foreach(Panel panel in panels)
            {
                Face2D face2D_Panel = plane.Convert(panel?.GetFace3D());
                if(face2D_Panel == null || !face2D_Panel.IsValid() || face2D_Panel.GetArea() < tolerance)
                {
                    continue;
                }

                ISegmentable2D segmentable2D = face2D_Panel.ExternalEdge2D as ISegmentable2D;
                if (segmentable2D == null)
                {
                    continue;
                }

                List<Point2D> point2Ds_Segmentable3D = segmentable2D.GetPoints();
                if (point2Ds_Segmentable3D == null)
                {
                    continue;
                }

                point2Ds.AddRange(point2Ds_Segmentable3D);

                BoundingBox2D boundingBox2D = face2D_Panel.GetBoundingBox();
                tuples.Add(new Tuple<Panel, BoundingBox2D, Face2D>(panel, boundingBox2D, face2D_Panel));
            }

            if(tuples == null || tuples.Count == 0)
            {
                return null;
            }

            List<Triangle2D> triangle2Ds = face2D.Triangulate(point2Ds, tolerance);
            if(triangle2Ds == null|| triangle2Ds.Count == 0)
            {
                return null;
            }

            List<Panel> panels_Temp = new List<Panel>();
            foreach(Triangle2D triangle2D in triangle2Ds)
            {
                BoundingBox2D boundingBox2D = triangle2D?.GetBoundingBox();
                if(boundingBox2D == null)
                {
                    continue;
                }

                Point2D point2D = triangle2D.GetCentroid();
                if(point2D == null)
                {
                    continue;
                }

                List<Tuple<Panel, BoundingBox2D, Face2D>> tuples_Temp = tuples.FindAll(x => x.Item2.InRange(boundingBox2D, tolerance));
                if(tuples_Temp == null || tuples_Temp.Count == 0)
                {
                    tuples_Temp = new List<Tuple<Panel, BoundingBox2D, Face2D>>(tuples);
                    tuples_Temp.Sort((x, y) => x.Item3.Distance(point2D, tolerance).CompareTo(y.Item3.Distance(point2D, tolerance)));
                    tuples_Temp = new List<Tuple<Panel, BoundingBox2D, Face2D>>() { tuples_Temp[0] };
                }
                
                if(tuples_Temp != null && tuples_Temp.Count > 1)
                {
                    List<Tuple<Panel, BoundingBox2D, Face2D>> tuples_Temp_Temp = tuples_Temp.FindAll(x => x.Item3.InRange(point2D, tolerance));
                    if(tuples_Temp_Temp == null || tuples_Temp_Temp.Count == 0)
                    {
                        tuples_Temp_Temp = tuples_Temp;
                    }

                    if(tuples_Temp_Temp.Count > 1)
                    {
                        Face2D face2D_Triangle2D = new Face2D(triangle2D);

                        List<Tuple<Tuple<Panel, BoundingBox2D, Face2D>, double>> tuples_Area = new List<Tuple<Tuple<Panel, BoundingBox2D, Face2D>, double>>();
                        foreach(Tuple<Panel, BoundingBox2D, Face2D> tuple in tuples_Temp_Temp)
                        {
                            List<Face2D> face2Ds_Intersection = face2D_Triangle2D.Intersection(tuple.Item3, tolerance);
                            if(face2Ds_Intersection == null || face2Ds_Intersection.Count == 0)
                            {
                                continue;
                            }

                            double area = face2Ds_Intersection.ConvertAll(x => x.GetArea()).Sum();
                            if(area < tolerance)
                            {
                                continue;
                            }

                            tuples_Area.Add(new Tuple<Tuple<Panel, BoundingBox2D, Face2D>, double>(tuple, area));
                        }

                        tuples_Area.Sort((x, y) => y.Item2.CompareTo(x.Item2));

                        tuples_Temp = new List<Tuple<Panel, BoundingBox2D, Face2D>>() { tuples_Area[0].Item1 };
                    }

                }

                if(tuples_Temp == null || tuples_Temp.Count == 0)
                {
                    continue;
                }

                Panel panel = tuples_Temp[0].Item1;

                Plane plane_Panel = panel.Plane;

                Triangle3D triangle3D = plane_Panel.Project(plane.Convert(triangle2D));
                if(triangle3D == null)
                {
                    continue;
                }

                panel = Create.Panel(Guid.NewGuid(), panel, new Face3D(triangle3D), null, true, tolerance, maxDistance);
                if(panel == null)
                {
                    continue;
                }

                panels_Temp.Add(panel);
            }

            List<Point3D> point3Ds = new List<Point3D>();
            foreach (Panel panel in panels_Temp)
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
                if (point3D == null)
                {
                    return;
                }

                Panel panel = Geometry.Object.Spatial.Query.Closest(panels, point3D);
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