using System;
using System.Collections.Generic;
using System.Linq;
using SAM.Geometry.Spatial;

namespace SAM.Analytical.Rhino.Plugin
{
    public static partial class Modify
    {
        public static void SetMaxExtends(this List<Panel> panels, double offset = 0.1, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (panels == null)
            {
                return;
            }

            List<Tuple<int, double>> tuples = new List<Tuple<int, double>>();
            for(int i =0; i < panels.Count; i++)
            {
                Panel panel = panels[i];
                
                if (panel.PanelType == PanelType.Air)
                {
                    panels[i].SetValue(PanelParameter.Weight, 0);
                }

                Face3D face3D = panel.GetFace3D();

                BoundingBox3D boundingBox3D = face3D.GetBoundingBox();

                if (boundingBox3D.Height > offset)
                {
                    Plane plane = Geometry.Spatial.Create.Plane(boundingBox3D.Min.Z + offset);

                    PlanarIntersectionResult planarIntersectionResult = Geometry.Spatial.Create.PlanarIntersectionResult(plane, face3D, tolerance_Angle, tolerance_Distance);
                    if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                    {
                        continue;
                    }

                    List<ISegmentable3D> segmentable3Ds = planarIntersectionResult.GetGeometry3Ds<ISegmentable3D>();
                    if (segmentable3Ds == null)
                    {
                        continue;
                    }

                    List<Point3D> point3Ds = new List<Point3D>();
                    foreach (ISegmentable3D segmentable3D in segmentable3Ds)
                    {
                        List<Point3D> point3Ds_Temp = segmentable3D?.GetPoints();
                        if (point3Ds_Temp != null)
                        {
                            point3Ds.AddRange(point3Ds_Temp);
                        }
                    }

                    point3Ds.ExtremePoints(out Point3D point3D_1, out Point3D point3D_2);

                    tuples.Add(new Tuple<int, double>(i, point3D_1.Distance(point3D_2)));
                }
                else
                {
                    Plane plane = face3D.GetPlane();
                    if (plane == null)
                    {
                        continue;
                    }

                    Geometry.Planar.Rectangle2D rectangle2D = Geometry.Planar.Create.Rectangle2D((plane.Convert(face3D).ExternalEdge2D as Geometry.Planar.ISegmentable2D)?.GetPoints());
                    if(rectangle2D == null)
                    {
                        continue;
                    }

                    tuples.Add(new Tuple<int, double>(i, System.Math.Max(rectangle2D.Height, rectangle2D.Width)));
                }
            }

            double min = tuples.ConvertAll(x => x.Item2).Min();
            double max = tuples.ConvertAll(x => x.Item2).Max();

            foreach(Tuple<int, double> tuple in tuples)
            {
                double weight = Math.Query.Remap(tuple.Item2, min, max, 0.2, 1);
                panels[tuple.Item1].SetValue(PanelParameter.Weight, weight);
            }
        }
    }
}