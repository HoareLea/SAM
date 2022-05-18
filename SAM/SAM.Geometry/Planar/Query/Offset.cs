//using ClipperLib;
using SAM.Core;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Operation.Buffer;
using NetTopologySuite.Geometries;
using NetTopologySuite.Noding.Snapround;
using NetTopologySuite.Operation.Polygonize;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Face2D> Offset(this Face2D face2D, double offset, bool includeExternalEdge = true, bool includeInternalEdges = true, double tolerance = Tolerance.MicroDistance)
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
                    List<Polygon2D> polygon2Ds = Offset(Create.Polygon2D(externalEdge), offset, tolerance);
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
                            List<Polygon2D> polygon2Ds = Offset(Create.Polygon2D(externalEdge), offset, tolerance);
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
        
        public static List<Polygon2D> Offset(this Polygon2D polygon2D, double offset, double tolerance = Tolerance.MicroDistance)
        {
            if (polygon2D == null)
                return null;

            //return Offset(polygon2D.Points, offset, JoinType.jtMiter, EndType.etClosedPolygon, tolerance)?.ConvertAll(x => new Polygon2D(x));

            return Offset(polygon2D, offset, JoinStyle.Mitre, EndCapStyle.Square, tolerance);
        }

        public static List<Polygon2D> Offset(this Polyline2D polyline2D, double offset, double tolerance = Tolerance.MicroDistance)
        {
            if (polyline2D == null)
                return null;

            //EndType endType = EndType.etOpenSquare;
            //if (polyline2D.IsClosed())
            //    endType = EndType.etClosedPolygon;

            //return Offset(polyline2D.Points, offset, JoinType.jtMiter, endType, tolerance).ConvertAll(x => new Polygon2D(x));

            return Offset(polyline2D, offset, JoinStyle.Mitre, EndCapStyle.Square, tolerance);
        }

        public static Triangle2D Offset(Triangle2D triangle2D, double offset, double tolerance = Tolerance.MicroDistance)
        {
            if (triangle2D == null)
            {
                return null;
            }

            List<Polygon2D> polygon2Ds = Offset(triangle2D, offset, JoinStyle.Mitre, EndCapStyle.Square, tolerance);
            if(polygon2Ds == null || polygon2Ds.Count == 0)
            {
                return null;
            }

            List<Point2D> point2Ds = polygon2Ds[0].GetPoints();
            if(point2Ds == null || point2Ds.Count  < 3)
            {
                return null;
            }

            return new Triangle2D(point2Ds[0], point2Ds[1], point2Ds[2]);

            //List<List<Point2D>> point2DsList =  Offset(triangle2D.GetPoints(), offset, JoinType.jtMiter, EndType.etClosedPolygon, tolerance);
            //if(point2DsList == null)
            //{
            //    return null;
            //}

            //point2DsList.RemoveAll(x => x == null || x.Count < 3);
            //if(point2DsList.Count == 0)
            //{
            //    return null;
            //}

            //if (point2DsList.Count > 1)
            //{
            //    point2DsList.Sort((x, y) => Area(y).CompareTo(Area(x)));
                
            //}

            //return new Triangle2D(point2DsList[0][0], point2DsList[0][1], point2DsList[0][2]);
        }

        public static BoundingBox2D Offset(BoundingBox2D boundingBox2D, double offset, double tolerance = Tolerance.MicroDistance)
        {
            if (boundingBox2D == null)
            {
                return null;
            }

            List<Polygon2D> polygon2Ds = Offset(boundingBox2D, offset, JoinStyle.Mitre, EndCapStyle.Square, tolerance);
            if (polygon2Ds == null || polygon2Ds.Count == 0)
            {
                return null;
            }

            List<Point2D> point2Ds = polygon2Ds[0].GetPoints();
            if (point2Ds == null || point2Ds.Count < 4)
            {
                return null;
            }

            return new BoundingBox2D(point2Ds);

            //List<List<Point2D>> point2DsList = Offset(boundingBox2D.GetPoints(), offset, JoinType.jtMiter, EndType.etClosedPolygon, tolerance);
            //if (point2DsList == null)
            //{
            //    return null;
            //}

            //point2DsList.RemoveAll(x => x == null || x.Count < 3);
            //if (point2DsList.Count == 0)
            //{
            //    return null;
            //}

            //if (point2DsList.Count > 1)
            //{
            //    point2DsList.Sort((x, y) => Area(y).CompareTo(Area(x)));

            //}

            //return new BoundingBox2D(point2DsList[0]);
        }

        public static Rectangle2D Offset(Rectangle2D rectangle2D, double offset, double tolerance = Tolerance.MicroDistance)
        {
            if (rectangle2D == null)
            {
                return null;
            }

            List<Polygon2D> polygon2Ds = Offset(rectangle2D, offset, JoinStyle.Mitre, EndCapStyle.Square, tolerance);
            if (polygon2Ds == null || polygon2Ds.Count == 0)
            {
                return null;
            }

            List<Point2D> point2Ds = polygon2Ds[0].GetPoints();
            if (point2Ds == null || point2Ds.Count < 4)
            {
                return null;
            }

            return Create.Rectangle2D(point2Ds);

            //List<List<Point2D>> point2DsList = Offset(rectangle2D.GetPoints(), offset, JoinType.jtMiter, EndType.etClosedPolygon, tolerance);
            //if (point2DsList == null)
            //{
            //    return null;
            //}

            //point2DsList.RemoveAll(x => x == null || x.Count < 3);
            //if (point2DsList.Count == 0)
            //{
            //    return null;
            //}

            //if (point2DsList.Count > 1)
            //{
            //    point2DsList.Sort((x, y) => Area(y).CompareTo(Area(x)));

            //}

            //return Create.Rectangle2D(point2DsList[0]);
        }

        public static Circle2D Offset(Circle2D circle2D, double offset)
        {
            return new Circle2D(circle2D.Center, System.Math.Abs(circle2D.Radious + offset));
        }

        public static Ellipse2D Offset(Ellipse2D ellipse2D, double offset)
        {
            return new Ellipse2D(ellipse2D.Center, System.Math.Abs(ellipse2D.Width + offset), System.Math.Abs(ellipse2D.Height + offset), ellipse2D.HeightDirection);
        }

        public static List<Polygon2D> Offset(this Polygon2D polygon2D, double offset, bool inside, double tolerance = Tolerance.Distance)
        {
            if (inside && offset > 0)
                offset = -offset;

            return Offset(polygon2D, offset, tolerance);
        }

        //public static List<Polygon2D> Offset(this ISegmentable2D segmentable2D, double offset, JoinType joinType, EndType endType, double tolerance = Tolerance.MicroDistance)
        //{
        //    if (segmentable2D == null)
        //        return null;

        //    List<IntPoint> intPoints = segmentable2D.ToClipper(tolerance);
        //    if (intPoints == null)
        //        return null;

        //    ClipperOffset clipperOffset = new ClipperOffset();
        //    clipperOffset.AddPath(intPoints, joinType, endType);
        //    List<List<IntPoint>> intPointList = new List<List<IntPoint>>();
        //    clipperOffset.Execute(ref intPointList, offset / tolerance);

        //    if (intPointList == null)
        //        return null;

        //    return intPointList.ConvertAll(x => new Polygon2D(x.ToSAM(tolerance)));
        //}

        //public static List<List<Point2D>> Offset(this IEnumerable<Point2D> point2Ds, double offset, JoinType joinType, EndType endType, double tolerance = Tolerance.MicroDistance)
        //{
        //    List<IntPoint> intPoints = point2Ds?.ToClipper(tolerance);
        //    if (intPoints == null)
        //        return null;

        //    ClipperOffset clipperOffset = new ClipperOffset();
        //    clipperOffset.AddPath(intPoints, joinType, endType);
        //    List<List<IntPoint>> intPointList = new List<List<IntPoint>>();
        //    clipperOffset.Execute(ref intPointList, offset / tolerance);

        //    if (intPointList == null)
        //        return null;

        //    return intPointList.ConvertAll(x => x.ToSAM(tolerance));
        //}

        public static List<Polygon2D> Offset(this ISegmentable2D segmentable2D, double offset, JoinStyle joinStyle, EndCapStyle endCapStyle, double tolerance = Tolerance.MicroDistance)
        {
            LineString lineString = segmentable2D.ToNTS(tolerance);
            if(lineString == null)
            {
                return null;
            }

            GeometryFactory geometryFactory = lineString.Factory;

            PrecisionModel precisionModel = new PrecisionModel(1 / tolerance);

            BufferParameters bufferParameters = new BufferParameters()
            {
                EndCapStyle = endCapStyle,
                JoinStyle = joinStyle
            };

            NetTopologySuite.Geometries.Geometry geometry = lineString;
            if(lineString is LinearRing)
            {
                geometry = new Polygon((LinearRing)lineString);
            }

            OffsetCurveSetBuilder offsetCurveSetBuilder = new OffsetCurveSetBuilder(geometry, offset, new OffsetCurveBuilder(precisionModel, bufferParameters));

            IEnumerable<NetTopologySuite.Noding.ISegmentString> segmentStrings = offsetCurveSetBuilder.GetCurves();

            List<LineString> lineStrings = new List<LineString>();
            foreach (NetTopologySuite.Noding.ISegmentString segmentString in segmentStrings)
            {
                NetTopologySuite.Noding.NodedSegmentString nodedSegmentString = segmentString as NetTopologySuite.Noding.NodedSegmentString;

                if (nodedSegmentString == null)
                {
                    continue;
                }

                for (int i = 0; i < nodedSegmentString.Count - 1; i++)
                {
                    lineStrings.Add(nodedSegmentString[i].ToGeometry(geometryFactory));
                }
            }

            lineStrings = new GeometryNoder(precisionModel).Node(lineStrings).ToList();
            if(lineString.IsClosed)
            {
                double offste_Abs = System.Math.Abs(offset);

                for (int i = lineStrings.Count - 1; i >= 0; i--)
                {
                    double distance = lineStrings[i].Distance(lineString);
                    if (distance < offste_Abs - tolerance)
                    {
                        lineStrings.RemoveAt(i);
                    }
                }
            }

            Polygonizer polygonizer = new Polygonizer(true);
            polygonizer.Add(lineStrings.ToArray());

            IEnumerable<NetTopologySuite.Geometries.Geometry> geometries = polygonizer.GetPolygons();
            if(geometries == null)
            {
                return null;
            }

            List<Polygon2D> result = new List<Polygon2D>();
            foreach(NetTopologySuite.Geometries.Geometry geometry_Temp in geometries)
            {
                ISAMGeometry2D sAMGeometry2D = geometry_Temp?.ToSAM(tolerance);
                if(sAMGeometry2D == null)
                {
                    continue;
                }

                if(sAMGeometry2D is Polygon2D)
                {
                    result.Add((Polygon2D)sAMGeometry2D);
                    continue;
                }

                if(sAMGeometry2D is Face2D)
                {
                    List<IClosed2D> edge2Ds = ((Face2D)sAMGeometry2D).Edge2Ds;
                    if(edge2Ds != null)
                    {
                        foreach(IClosed2D edge2D in edge2Ds)
                        { 
                            if(edge2D is ISegmentable2D)
                            {
                                result.Add(new Polygon2D(((ISegmentable2D)edge2D).GetPoints()));
                            }
                        }
                    }
                }
            }

            return result;

        }


        public static List<Polyline2D> Offset(this Polyline2D polyline2D, double offset, Orientation orientation, double tolerance = Tolerance.Distance)
        {
            return Offset(polyline2D, new double[] { offset }, orientation, tolerance);
        }

        public static List<Polygon2D> Offset(this Polygon2D polygon2D, IEnumerable<double> offsets, bool inside, bool simplify = true, bool orient = true, double tolerance = Tolerance.Distance)
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

        public static List<Polyline2D> Offset(this Polyline2D polyline2D, IEnumerable<double> offsets, Orientation orientation, double tolerance = Tolerance.Distance)
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

        private static List<Polygon2D> Offset_Inside(this Polygon2D polygon2D, List<double> offsets, bool simplify = true, bool orient = true, double tolerance = Tolerance.Distance)
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
                segment2D_Offset = segment2D_Offset.Join(polygon2D, tolerance);

                if (segment2D_Offset != null && segment2D_Offset.GetLength() >= tolerance)
                    segment2Ds_Offset.Add(segment2D_Offset);
            }

            segment2Ds_Offset = Split(segment2Ds_Offset, tolerance);

            segment2Ds_Offset.RemoveAll(x => !polygon2D.Inside(x.Mid()) && !polygon2D.On(x.Mid()));

            //segment2Ds_Offset.RemoveAll(x => polygon2D.On(x[0]) || polygon2D.On(x[1]));

            for (int i = 0; i < segment2Ds.Count; i++)
            {
                if (offsets[i] - Tolerance.MacroDistance < 0)
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
                    List<Point2D> point2Ds_Simplify = SimplifyByAngle(polygon2Ds[i].GetPoints(), true, Tolerance.Angle);
                    polygon2Ds[i] = new Polygon2D(point2Ds_Simplify);
                }

                if (orient)
                    polygon2Ds[i].SetOrientation(orientation);
            }

            return polygon2Ds;
        }

        private static List<Polygon2D> Offset_Outside(this Polygon2D polygon2D, List<double> offsets, bool simplify = true, bool orient = true, double tolerance = Tolerance.Distance)
        {
            //TODO: Implement Outside Offset
            throw new System.Exception();
        }
    }
}