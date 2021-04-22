using System;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Guid> JoinExternal(this List<Panel> panels, double elevation, double maxDistance, double snapTolerance = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (panels == null)
                return null;

            Plane plane = Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation)) as Plane;

            List<Geometry.Planar.ISegmentable2D> segmentable2Ds = new List<Geometry.Planar.ISegmentable2D>();
            List<Tuple<Panel, BoundingBox3D, List<Geometry.Planar.ISegmentable2D>>> tuples = new List<Tuple<Panel, BoundingBox3D, List<Geometry.Planar.ISegmentable2D>>>();
            foreach(Panel panel in panels)
            {
                Face3D face3D = panel.GetFace3D(false, tolerance_Distance);
                if(face3D == null)
                {
                    continue;
                }

                BoundingBox3D boundingBox3D = face3D.GetBoundingBox(tolerance_Distance);
                if(boundingBox3D.Max.Z < elevation || boundingBox3D.Min.Z > elevation)
                {
                    continue;
                }

                PlanarIntersectionResult planarIntersectionResult = Geometry.Spatial.Create.PlanarIntersectionResult(plane, face3D, tolerance_Angle, tolerance_Distance);
                if(planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                {
                    continue;
                }

                List<Geometry.Planar.ISegmentable2D> segmentable2Ds_Temp = planarIntersectionResult.GetGeometry2Ds<Geometry.Planar.ISegmentable2D>();
                if(segmentable2Ds_Temp == null || segmentable2Ds_Temp.Count == 0)
                {
                    continue;
                }

                segmentable2Ds.AddRange(segmentable2Ds_Temp);
                tuples.Add(new Tuple<Panel, BoundingBox3D, List<Geometry.Planar.ISegmentable2D>>(panel, boundingBox3D, segmentable2Ds_Temp));
            }

            HashSet<Guid> result = new HashSet<Guid>();

            List<Geometry.Planar.Polygon2D> polygon2Ds = Geometry.Planar.Query.ExternalPolygon2Ds(segmentable2Ds, maxDistance, snapTolerance, tolerance_Distance);
            if (polygon2Ds == null || polygon2Ds.Count == 0)
                return result.ToList();

            List<Geometry.Planar.Segment2D> segment2Ds = new List<Geometry.Planar.Segment2D>();
            foreach (Geometry.Planar.Polygon2D polygon2D in polygon2Ds)
            {
                List<Geometry.Planar.Segment2D> segment2Ds_Temp = polygon2D?.GetSegments();
                if(segment2Ds_Temp == null || segment2Ds_Temp.Count == 0)
                {
                    continue;
                }

                segment2Ds.AddRange(segment2Ds_Temp);
            }


            foreach (Tuple<Panel, BoundingBox3D, List<Geometry.Planar.ISegmentable2D>> tuple in tuples)
            {
                List<Geometry.Planar.ISegmentable2D> segmentable2Ds_Temp = tuple?.Item3;
                if (segmentable2Ds_Temp == null || segmentable2Ds_Temp.Count == 0)
                {
                    continue;
                }

                foreach (Geometry.Planar.ISegmentable2D segmentable2D in segmentable2Ds_Temp)
                {
                    List<Geometry.Planar.Segment2D> segment2Ds_Temp = segmentable2D?.GetSegments();
                    if (segment2Ds_Temp == null || segment2Ds_Temp.Count == 0)
                    {
                        continue;
                    }

                    foreach(Geometry.Planar.Segment2D segment2D in segment2Ds_Temp)
                    {
                        Geometry.Planar.Polygon2D polygon2D = polygon2Ds.Find(x => x.On(segment2D.Mid(), tolerance_Distance));
                        if(polygon2D != null)
                        {
                            segment2Ds.Add(segment2D);
                        }
                    }
                }
            }

            segment2Ds = Geometry.Planar.Query.Split(segment2Ds, tolerance_Distance);

            foreach(Tuple<Panel, BoundingBox3D, List<Geometry.Planar.ISegmentable2D>> tuple in tuples)
            {
                if (tuple.Item3 == null)
                {
                    continue;
                }

                foreach(Geometry.Planar.ISegmentable2D segmentable2D in tuple.Item3)
                {
                    if (segment2Ds == null || segment2Ds.Count == 0)
                    {
                        break;
                    }

                    List<Geometry.Planar.Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();
                    if(segment2Ds_Temp == null || segment2Ds_Temp.Count == 0)
                    {
                        continue;
                    }

                    foreach(Geometry.Planar.Segment2D segment2D in segment2Ds_Temp)
                    {
                        if(segment2D == null || segment2Ds == null || segment2Ds.Count == 0)
                        {
                            break;
                        }

                        Geometry.Planar.Point2D point2D_Mid = segment2D.Mid();

                        segment2Ds.RemoveAll(x => x.On(point2D_Mid, snapTolerance));
                    }
                }
            }

            foreach(Geometry.Planar.Segment2D segment2D in segment2Ds)
            {
                Geometry.Planar.Point2D point2D_1 = segment2D[0];
                Geometry.Planar.Point2D point2D_2 = segment2D[1];

                Tuple<Panel, BoundingBox3D, Geometry.Planar.Segment2D> tuple_Temp = null;
                foreach (Tuple<Panel, BoundingBox3D, List<Geometry.Planar.ISegmentable2D>> tuple in tuples)
                {
                    foreach(Geometry.Planar.ISegmentable2D segmentable2D in tuple.Item3)
                    {
                        List<Geometry.Planar.Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();
                        Geometry.Planar.Point2D point2D_Temp = null;
                        Geometry.Planar.Segment2D segment2D_Temp = null;

                        segment2D_Temp = segment2Ds_Temp[0];
                        point2D_Temp = segment2D_Temp[0];

                        if (point2D_1.AlmostEquals(point2D_Temp, snapTolerance) || point2D_2.AlmostEquals(point2D_Temp, snapTolerance))
                        {
                            if(segment2D.Direction.SmallestAngle(segment2D_Temp.Direction) < tolerance_Angle || segment2D.Direction.GetNegated().SmallestAngle(segment2D_Temp.Direction) < tolerance_Angle)
                            {
                                tuple_Temp = new Tuple<Panel, BoundingBox3D, Geometry.Planar.Segment2D>(tuple.Item1, tuple.Item2, segment2D_Temp);
                                break;
                            }
                        }

                        segment2D_Temp = segment2Ds_Temp[segment2Ds_Temp.Count - 1];
                        point2D_Temp = segment2D_Temp[1];

                        if (point2D_1.AlmostEquals(point2D_Temp, snapTolerance) || point2D_2.AlmostEquals(point2D_Temp, snapTolerance))
                        {
                            if (segment2D.Direction.SmallestAngle(segment2D_Temp.Direction) < tolerance_Angle || segment2D.Direction.GetNegated().SmallestAngle(segment2D_Temp.Direction) < tolerance_Angle)
                            {
                                tuple_Temp = new Tuple<Panel, BoundingBox3D, Geometry.Planar.Segment2D>(tuple.Item1, tuple.Item2, segment2D_Temp);
                                break;
                            }
                        }
                    }

                    if(tuple_Temp != null)
                    {
                        break;
                    }
                }

                if(tuple_Temp == null)
                {
                    continue;
                }

                Panel panel_Old = tuple_Temp.Item1;

                Geometry.Planar.Query.ExtremePoints(new Geometry.Planar.Point2D[] { point2D_1, point2D_2, tuple_Temp.Item3[0], tuple_Temp.Item3[1] }, out point2D_1, out point2D_2);

                Plane plane_Bottom = Plane.WorldXY.GetMoved(new Vector3D(0, 0, tuple_Temp.Item2.Min.Z)) as Plane;

                Geometry.Planar.Segment2D segment2D_New = new Geometry.Planar.Segment2D(point2D_1, point2D_2);

                Face3D face3D = Geometry.Spatial.Create.Face3D(plane_Bottom.Convert(segment2D_New), tuple_Temp.Item2.Max.Z - tuple_Temp.Item2.Min.Z);

                Panel panel_New = new Panel(panel_Old.Guid, panel_Old, face3D);

                int index= tuples.FindIndex(x => x.Item1 == panel_Old);
                if(index != -1)
                {
                    tuples[index] = new Tuple<Panel, BoundingBox3D, List<Geometry.Planar.ISegmentable2D>>(panel_New, face3D.GetBoundingBox(tolerance_Distance), new List<Geometry.Planar.ISegmentable2D>() { segment2D_New });
                    result.Add(panel_New.Guid);
                }
            }

            panels.Clear();

            panels.AddRange(tuples.ConvertAll(x => x.Item1));

            return result.ToList();
        }
    }
}