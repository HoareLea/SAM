using ClipperLib;
using SAM.Core;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Face2D> Offset(this Face2D face2D, double offset, bool includeExternalEdge = true, bool includeInternalEdges = true, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (face2D == null)
                return null;

            IClosed2D externalEdge = face2D.ExternalEdge2D;
            if (externalEdge == null)
                return null;

            List<IClosed2D> internalEdges = face2D.InternalEdge2Ds;

            List<IClosed2D> externalEdges_Offset = new List<IClosed2D>() { externalEdge };
            List<IClosed2D> internalEdges_Offset = internalEdges;

            if (includeExternalEdge)
            {
                if(externalEdge is Polygon2D)
                {
                    List<Polygon2D> polygon2Ds = Offset((Polygon2D)externalEdge, offset, tolerance);
                    if (polygon2Ds != null)
                        externalEdges_Offset = new List<IClosed2D>(polygon2Ds);
                }
            }

            if (externalEdges_Offset == null || externalEdges_Offset.Count == 0)
                return null;

            if (includeInternalEdges)
            {
                if(internalEdges != null)
                {
                    internalEdges_Offset = new List<IClosed2D>();
                    foreach (IClosed2D closed2D in internalEdges)
                    {
                        if(closed2D is Polygon2D)
                        {
                            List<Polygon2D> polygon2Ds = Offset((Polygon2D)externalEdge, offset, tolerance);
                            if (polygon2Ds != null)
                                internalEdges_Offset.AddRange(polygon2Ds);
                        }
                    }
                }
            }

            List<IClosed2D> edges = new List<IClosed2D>(externalEdges_Offset);
            if (internalEdges_Offset != null && internalEdges_Offset.Count != 0)
                edges.AddRange(internalEdges_Offset);

            return Create.Face2Ds(edges);
        }
        
        public static List<Polygon2D> Offset(this Polygon2D polygon2D, double offset, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (polygon2D == null)
                return null;

            return Offset(new Polyline2D(polygon2D.GetPoints(), true), offset, JoinType.jtMiter, EndType.etClosedPolygon, tolerance);
        }

        public static List<Polygon2D> Offset(this Polyline2D polyline2D, double offset, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (polyline2D == null)
                return null;

            EndType endType = EndType.etOpenSquare;
            if (polyline2D.IsClosed())
                endType = EndType.etClosedPolygon;

            return Offset(polyline2D, offset, JoinType.jtMiter, endType, tolerance);
        }

        //public static List<Polygon2D> Offset(this Polyline2D polyline2D, double offset, JoinType joinType, EndType endType, double tolerance = Core.Tolerance.MicroDistance)
        //{
        //    if (polyline2D == null)
        //        return null;

        // List<IntPoint> intPoints = ((ISegmentable2D)polyline2D).ToClipper(tolerance); if
        // (intPoints == null) return null;

        // ClipperOffset clipperOffset = new ClipperOffset(); clipperOffset.AddPath(intPoints,
        // joinType, endType); List<List<IntPoint>> intPointList = new List<List<IntPoint>>();
        // clipperOffset.Execute(ref intPointList, offset / tolerance);

        // if (intPointList == null) return null;

        //    return intPointList.ConvertAll(x => new Polygon2D(x.ToSAM(tolerance)));
        //}

        public static List<Polygon2D> Offset(this ISegmentable2D segmentable2D, double offset, JoinType joinType, EndType endType, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (segmentable2D == null)
                return null;

            List<IntPoint> intPoints = segmentable2D.ToClipper(tolerance);
            if (intPoints == null)
                return null;

            ClipperOffset clipperOffset = new ClipperOffset();
            clipperOffset.AddPath(intPoints, joinType, endType);
            List<List<IntPoint>> intPointList = new List<List<IntPoint>>();
            clipperOffset.Execute(ref intPointList, offset / tolerance);

            if (intPointList == null)
                return null;

            return intPointList.ConvertAll(x => new Polygon2D(x.ToSAM(tolerance)));
        }

        public static List<Polygon2D> Offset(this Polygon2D polygon2D, double offset, bool inside, double tolerance = Core.Tolerance.Distance)
        {
            if (inside && offset > 0)
                offset = -offset;

            return Offset(polygon2D, offset, JoinType.jtMiter, EndType.etClosedPolygon, tolerance);
        }

        public static List<Polyline2D> Offset(this Polyline2D polyline2D, double offset, Orientation orientation, double tolerance = Core.Tolerance.Distance)
        {
            return Offset(polyline2D, new double[] { offset }, orientation, tolerance);
        }

        public static List<Polygon2D> Offset(this Polygon2D polygon2D, IEnumerable<double> offsets, bool inside, bool simplify = true, bool orient = true, double tolerance = Core.Tolerance.Distance)
        {
            if (polygon2D == null || offsets == null)
                return null;

            if (offsets.Count() < 1)
                return null;

            if (offsets.Count() == 1)
                return Offset(polygon2D, offsets.First(), inside, tolerance);

            int count = polygon2D.Count;

            List<double> offsets_Temp = new List<double>(offsets);
            double offset_Temp = offsets.Last();
            while (offsets_Temp.Count < count)
                offsets_Temp.Add(offset_Temp);

            if (offsets_Temp.TrueForAll(x => System.Math.Abs(offsets_Temp.First() - x) <= tolerance))
                return Offset(polygon2D, offsets.First(), inside, tolerance);

            if (inside)
                return Offset_Inside(polygon2D, offsets_Temp, simplify, orient, tolerance);
            else
                return Offset_Outside(polygon2D, offsets_Temp, simplify, orient, tolerance);
        }

        public static List<Polyline2D> Offset(this Polyline2D polyline2D, IEnumerable<double> offsets, Orientation orientation, double tolerance = Core.Tolerance.Distance)
        {
            if (polyline2D == null || polyline2D.CountPoints() <= 1 || offsets == null || orientation == Geometry.Orientation.Collinear || orientation == Geometry.Orientation.Undefined)
                return null;

            int count = offsets.Count();
            if (count == 0)
                return null;

            List<Segment2D> segment2Ds_Polyline2D = polyline2D.GetSegments();
            if (segment2Ds_Polyline2D.Count == 0)
                return null;

            List<double> offsets_Temp = new List<double>(offsets);
            double offset_Temp = offsets.Last();
            while (offsets_Temp.Count < count)
                offsets_Temp.Add(offset_Temp);

            List<Segment2D> segment2Ds = new List<Segment2D>();
            for (int i = 0; i < segment2Ds_Polyline2D.Count; i++)
            {
                Segment2D segment2D = segment2Ds_Polyline2D[i];

                Vector2D vector2D = segment2D.Direction.GetPerpendicular(orientation).Unit * offsets_Temp[i];
                segment2Ds.Add(segment2D.GetMoved(vector2D));
            }

            Modify.JoinByIntersections(segment2Ds, false, tolerance);

            List<Polyline2D> result = new List<Polyline2D>() { new Polyline2D(segment2Ds) };

            return result;
        }

        private static List<Polygon2D> Offset_Inside(this Polygon2D polygon2D, List<double> offsets, bool simplify = true, bool orient = true, double tolerance = Core.Tolerance.Distance)
        {
            Orientation orientation = polygon2D.GetOrientation();

            List<Segment2D> segment2Ds = polygon2D?.GetSegments();
            if (segment2Ds == null || segment2Ds.Count == 0)
                return null;

            List<Segment2D> segment2Ds_Offset = new List<Segment2D>();
            for (int i = 0; i < segment2Ds.Count; i++)
            {
                Segment2D segment2D = segment2Ds[i];

                Segment2D segment2D_Offset = segment2D.Offset(offsets[i], orientation);
                segment2D_Offset = segment2D_Offset.ExtendOrTrim(polygon2D, tolerance);

                if (segment2D_Offset != null && segment2D_Offset.GetLength() >= tolerance)
                    segment2Ds_Offset.Add(segment2D_Offset);
            }

            segment2Ds_Offset = Split(segment2Ds_Offset, tolerance);

            segment2Ds_Offset.RemoveAll(x => !polygon2D.Inside(x.Mid()) && !polygon2D.On(x.Mid()));

            //segment2Ds_Offset.RemoveAll(x => polygon2D.On(x[0]) || polygon2D.On(x[1]));

            for (int i = 0; i < segment2Ds.Count; i++)
            {
                if (offsets[i] - Core.Tolerance.MacroDistance < 0)
                    continue;

                Segment2D segment2D = segment2Ds[i];
                //segment2Ds_Offset.RemoveAll(x => segment2D.Distance(x) < offsets[i] - Core.Tolerance.MacroDistance);

                segment2Ds_Offset.RemoveAll(x => segment2D.On(x[0]) || segment2D.On(x[1]));
            }

            List<Polygon2D> polygon2Ds_Temp = Create.Polygon2Ds(segment2Ds_Offset, tolerance);//new PointGraph2D(segment2Ds_Offset, false, tolerance).GetPolygon2Ds();
            if (polygon2Ds_Temp == null || polygon2Ds_Temp.Count == 0)
                return null;

            List<Polygon2D> polygon2Ds = new List<Polygon2D>();
            for (int i = 0; i < polygon2Ds_Temp.Count; i++)
            {
                Polygon2D polygon2D_Temp = polygon2Ds_Temp[i];

                //2020.08.02 START
                List<Point2D> point2Ds = polygon2D_Temp.Points;
                if (point2Ds == null || point2Ds.Count < 3)
                    continue;

                bool remove = false;
                foreach (Point2D point2D in point2Ds)
                {
                    List<double> distances = segment2Ds.ConvertAll(x => x.Distance(point2D));
                    double distance_Min = distances.Min();
                    List<int> indexes = distances.IndexesOf(distance_Min);

                    double offset = offsets[indexes.First()];
                    if(indexes.Count > 1)
                        offset = indexes.ConvertAll(x => offsets[x]).Min();

                    if (distance_Min < offset - tolerance)
                    {
                        remove = true;
                        break;
                    }
                }

                if (remove)
                    continue;
                //2020.08.02 END

                List<Segment2D> segment2Ds_Temp = IntersectionSegment2Ds(polygon2D, polygon2D_Temp, false, tolerance);
                if (segment2Ds_Temp == null || segment2Ds_Temp.Count == 0)
                {
                    polygon2Ds.Add(polygon2D_Temp);
                    continue;
                }

                remove = false;
                foreach (Segment2D Segment2Ds in segment2Ds_Temp)
                {
                    Point2D point2D = Segment2Ds.Mid();
                    int index = segment2Ds.FindIndex(x => x.On(point2D));
                    if (index == -1 || offsets[index] != 0)
                    {
                        remove = true;
                        break;
                    }
                }

                if (remove)
                    continue;

                polygon2Ds.Add(polygon2D_Temp);
            }

            polygon2Ds = ExternalPolygon2Ds(polygon2Ds, tolerance);//new PointGraph2D(polygon2Ds, false, tolerance).GetPolygon2Ds_External();
            if (polygon2Ds == null || polygon2Ds.Count == 0)
                return null;

            for (int i = 0; i < polygon2Ds.Count; i++)
            {
                if (simplify)
                {
                    List<Point2D> point2Ds_Simplify = polygon2Ds[i].GetPoints();
                    SimplifyBySAM_Angle(point2Ds_Simplify, true, Core.Tolerance.Angle);
                    polygon2Ds[i] = new Polygon2D(point2Ds_Simplify);
                }

                if (orient)
                    polygon2Ds[i].SetOrientation(orientation);
            }

            return polygon2Ds;
        }

        private static List<Polygon2D> Offset_Outside(this Polygon2D polygon2D, List<double> offsets, bool simplify = true, bool orient = true, double tolerance = Core.Tolerance.Distance)
        {
            //TODO: Implement Outside Offset
            throw new System.Exception();
        }
    }
}