using System.Collections.Generic;
using SAM.Geometry.Spatial;
using SAM.Geometry.Planar;
using System;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> SnapByElevation(this IEnumerable<Panel> panels, double elevation, double maxTolerance = Core.Tolerance.MacroDistance, double minTolerance = Core.Tolerance.MicroDistance)
        {
            if(panels == null || double.IsNaN(elevation))
            {
                return null;
            }

            Plane plane = Plane.WorldXY.GetMoved(Vector3D.WorldZ * elevation) as Plane;

            Dictionary<Panel, List<ISegmentable2D>> dictionary = panels.SectionDictionary<ISegmentable2D>(plane, maxTolerance);
            if(dictionary == null)
            {
                return null;
            }

            List<Tuple<Panel, List<Segment2D>>> tuples = new List<Tuple<Panel, List<Segment2D>>>();
            List<Tuple<Point2D, Segment2D, bool>> tuples_Point2D = new List<Tuple<Point2D, Segment2D, bool>>();
            foreach(KeyValuePair<Panel, List<ISegmentable2D>> keyValuePair in dictionary)
            {
                if(keyValuePair.Key == null || keyValuePair.Value == null || keyValuePair.Value.Count == 0)
                {
                    continue;
                }

                List<Segment2D> segment2Ds = new List<Segment2D>();
                foreach(ISegmentable2D segmentable2D in keyValuePair.Value)
                {
                    List<Segment2D> segment2Ds_Temp = segmentable2D?.GetSegments();
                    if(segment2Ds_Temp == null || segment2Ds_Temp.Count == 0)
                    {
                        continue;
                    }

                    segment2Ds.AddRange(segment2Ds_Temp);
                }

                foreach (Segment2D segment2D in segment2Ds)
                {
                    tuples_Point2D.Add(new Tuple<Point2D, Segment2D, bool>(segment2D[0], segment2D, true));
                    tuples_Point2D.Add(new Tuple<Point2D, Segment2D, bool>(segment2D[1], segment2D, false));
                }

                tuples.Add(new Tuple<Panel, List<Segment2D>>(keyValuePair.Key, segment2Ds));
            }

            List<Tuple<Point2D, Segment2D, bool>> tuples_Point2D_Snap = new List<Tuple<Point2D, Segment2D, bool>>();
            while (tuples_Point2D.Count > 0)
            {
                List<Tuple<Point2D, Segment2D, bool>> tuples_Point2D_Temp = tuples_Point2D.FindAll(x => x.Item1.Distance(tuples_Point2D[0].Item1) <= maxTolerance);
                tuples_Point2D_Temp.ForEach(x => tuples_Point2D.Remove(x));
                if(tuples_Point2D_Temp.Count < 2)
                {
                    continue;
                }

                bool equals = true;
                for(int i= 1; i < tuples_Point2D_Temp.Count; i++)
                {
                    if(!tuples_Point2D_Temp[0].Item1.AlmostEquals(tuples_Point2D_Temp[i].Item1, minTolerance))
                    {
                        equals = false;
                        break;
                    }
                }

                if(equals)
                {
                    continue;
                }

                List<Point2D> point2Ds = tuples_Point2D_Temp.ConvertAll(x => x.Item1);
                point2Ds.Add(point2Ds.Average());

                List<Tuple<Point2D, double>> tuples_Weight = new List<Tuple<Point2D, double>>();
                foreach(Point2D point2D in point2Ds)
                {
                    List<Tuple<Segment2D, double>> tuples_Segment2D_Weight = new List<Tuple<Segment2D, double>>();

                    foreach(Tuple<Point2D, Segment2D, bool> tuple in tuples_Point2D_Temp)
                    {
                        Segment2D segment2D = null;
                        if(tuple.Item3)
                        {
                            segment2D = new Segment2D(point2D, tuple.Item2[1]);
                        }
                        else
                        {
                            segment2D = new Segment2D(tuple.Item2[0], point2D);
                        }

                        tuples_Segment2D_Weight.Add(new Tuple<Segment2D, double> (segment2D, segment2D.GetLength() * segment2D.Direction.SmallestAngle(tuple.Item2.Direction)));
                    }

                    tuples_Weight.Add(new Tuple<Point2D, double>(point2D, tuples_Segment2D_Weight.ConvertAll(x => x.Item2).Sum()));
                }

                tuples_Weight.Sort((x, y) => x.Item2.CompareTo(y.Item2));

                Point2D point2D_Snap = tuples_Weight[0].Item1;
                
                foreach (Tuple<Point2D, Segment2D, bool> tuple in tuples_Point2D_Temp)
                {
                    tuples_Point2D_Snap.Add(new Tuple<Point2D, Segment2D, bool>(point2D_Snap, tuple.Item2, tuple.Item3));
                }
            }

            List<Panel> result = new List<Panel>();
            foreach(Tuple<Panel, List<Segment2D>> tuple in tuples)
            {
                List<Tuple<Point2D, Segment2D, bool>> tuples_Point2D_Snap_Panel = new List<Tuple<Point2D, Segment2D, bool>>();
                foreach (Segment2D segment2D in tuple.Item2)
                {
                    tuples_Point2D_Snap_Panel.Add(tuples_Point2D_Snap.Find(x => x.Item2 == segment2D));
                }

                if (tuples_Point2D_Snap_Panel.TrueForAll(x => x == null))
                {
                    result.Add(tuple.Item1);
                    continue;
                }

                for(int i=0; i < tuple.Item2.Count; i++)
                {
                    Segment2D segment2D = tuple.Item2[i];
                    if (tuples_Point2D_Snap_Panel[i] != null)
                    {
                        if (tuples_Point2D_Snap_Panel[i].Item3)
                        {
                            segment2D = new Segment2D(tuples_Point2D_Snap_Panel[i].Item1, segment2D[1]);
                        }
                        else
                        {
                            segment2D = new Segment2D(segment2D[0], tuples_Point2D_Snap_Panel[i].Item1);
                        }
                    }

                    Guid guid = tuple.Item1.Guid;
                    if(result.Find(x => x.Guid == guid) != null)
                    {
                        guid = Guid.NewGuid();
                    }

                    BoundingBox3D boundingBox3D = tuple.Item1.GetBoundingBox();
                    Vector3D vector3D = Vector3D.WorldZ * boundingBox3D.Height;

                    Segment3D segment3D = plane.Convert(segment2D);

                    Panel panel = Create.Panel(guid, tuple.Item1, new PlanarBoundary3D(new Polygon3D(new Point3D[] { segment3D[0], segment3D[1], segment3D[1].GetMoved(vector3D) as Point3D, segment3D[0].GetMoved(vector3D) as Point3D })));
                    result.Add(panel);
                }

            }

            return result;
        }
    }
}