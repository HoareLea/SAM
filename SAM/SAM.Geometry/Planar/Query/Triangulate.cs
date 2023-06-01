using System.Collections.Generic;
using NetTopologySuite.Triangulate;
using NetTopologySuite.Geometries;
using System.Linq;
using System;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Triangle2D> Triangulate(this IEnumerable<Point2D> point2Ds, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (point2Ds == null)
            {
                return null;
            }

            if (point2Ds.Count() < 3)
            {
                return new List<Triangle2D>();
            }

            List<Coordinate> coordinates = new List<Coordinate>();
            foreach (Point2D point2D in point2Ds)
            {
                Coordinate coordinate = point2D.ToNTS(tolerance);
                if (coordinate == null)
                {
                    continue;
                }

                coordinates.Add(coordinate);
            }

            DelaunayTriangulationBuilder delaunayTriangulationBuilder = new DelaunayTriangulationBuilder();
            delaunayTriangulationBuilder.SetSites(coordinates);

            GeometryFactory geometryFactory = new GeometryFactory(new PrecisionModel(1 / tolerance));

            GeometryCollection geometryCollection = delaunayTriangulationBuilder.GetTriangles(geometryFactory);
            if (geometryCollection == null)
                return null;

            List<Triangle2D> result = new List<Triangle2D>();
            foreach (NetTopologySuite.Geometries.Geometry geometry in geometryCollection.Geometries)
            {
                Polygon polygon_Temp = geometry as Polygon;
                if (polygon_Temp == null)
                {
                    continue;
                }

                Coordinate[] coordinates_Temp = polygon_Temp.Coordinates;
                if (coordinates_Temp == null || coordinates_Temp.Length != 4)
                {
                    continue;
                }

                result.Add(new Triangle2D(coordinates_Temp[0].ToSAM(tolerance), coordinates_Temp[1].ToSAM(), coordinates_Temp[2].ToSAM(tolerance)));
            }

            return result;
        }

        public static List<Triangle2D> Triangulate(this Polygon2D polygon2D, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (polygon2D == null)
                return null;

            Polygon polygon = polygon2D.ToNTS_Polygon(tolerance);

            DelaunayTriangulationBuilder delaunayTriangulationBuilder = new DelaunayTriangulationBuilder();
            delaunayTriangulationBuilder.SetSites(polygon);

            GeometryFactory geometryFactory = new GeometryFactory(new PrecisionModel(1 / tolerance));

            GeometryCollection geometryCollection = delaunayTriangulationBuilder.GetTriangles(geometryFactory);
            if (geometryCollection == null)
                return null;

            List<Triangle2D> result = new List<Triangle2D>();
            foreach (NetTopologySuite.Geometries.Geometry geometry in geometryCollection.Geometries)
            {
                Polygon polygon_Temp = geometry as Polygon;
                if (polygon_Temp == null)
                    continue;

                Coordinate[] coordinates = polygon_Temp.Coordinates;
                if (coordinates == null || coordinates.Length != 4)
                    continue;

                result.Add(new Triangle2D(coordinates[0].ToSAM(tolerance), coordinates[1].ToSAM(), coordinates[2].ToSAM(tolerance)));
            }

            return result;
        }

        public static List<Triangle2D> Triangulate(this Face2D face2D, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (face2D == null)
                return null;

            List<Face2D> face2Ds = face2D.FixEdges(tolerance);
            if(face2Ds == null || face2Ds.Count == 0)
            {
                face2Ds = new List<Face2D>() { face2D };
            }

            List<Triangle2D> result = new List<Triangle2D>();

            foreach (Face2D face2D_Temp in face2Ds)
            {
                Polygon polygon = face2D_Temp.ToNTS(tolerance);
                if(polygon == null || polygon.IsEmpty)
                {
                    continue;
                }

                List<Polygon> polygons_Triangulate = null;
                try
                {
                    polygons_Triangulate = Triangulate(polygon, tolerance);
                }
                catch
                {
                    polygons_Triangulate = null;
                }

                if(polygons_Triangulate == null || polygons_Triangulate.Count == 0)
                {
                    if(!face2D_Temp.HasInternalEdge2Ds)
                    {
                        continue;
                    }

                    List<Face2D> face2Ds_Split = face2D_Temp.SplitByInternalEdges(tolerance);
                    if (face2Ds_Split == null || face2Ds_Split.Count == 0)
                    {
                        continue;
                    }

                    foreach(Face2D face2D_Split in face2Ds_Split)
                    {
                        List<Triangle2D> triangle2Ds = Triangulate(face2D_Split, tolerance);
                        if(triangle2Ds == null || triangle2Ds.Count == 0)
                        {
                            continue;
                        }

                        result.AddRange(triangle2Ds);
                    }

                    continue;
                }

                foreach (Polygon polygon_Triangulate in polygons_Triangulate)
                {
                    Coordinate[] coordinates = polygon_Triangulate?.Coordinates;
                    if (coordinates == null || coordinates.Length != 4)
                    {
                        continue;
                    }

                    result.Add(new Triangle2D(coordinates[0].ToSAM(tolerance), coordinates[1].ToSAM(), coordinates[2].ToSAM(tolerance)));
                }
            }

            return result;
        }

        public static List<Triangle2D> Triangulate(this Polyline2D polyline2D, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (polyline2D == null)
                return null;

            if (!polyline2D.IsClosed(tolerance))
                return null;

            return new Polygon2D(polyline2D.Points).Triangulate(tolerance);
        }

        public static List<Triangle2D> Triangulate(this Rectangle2D rectangle2D)
        {
            List<Segment2D> segment2Ds = rectangle2D?.GetSegments();
            if (segment2Ds == null || segment2Ds.Count != 4)
                return null;

            List<Triangle2D> result = new List<Triangle2D>();
            result.Add(new Triangle2D(segment2Ds[0][0], segment2Ds[0][1], segment2Ds[1][1]));
            result.Add(new Triangle2D(segment2Ds[2][0], segment2Ds[2][1], segment2Ds[3][1]));
            return result;
        }

        public static List<Triangle2D> Triangulate(this BoundingBox2D boundingBox2D)
        {
            List<Segment2D> segment2Ds = boundingBox2D?.GetSegments();
            if (segment2Ds == null || segment2Ds.Count != 4)
                return null;

            List<Triangle2D> result = new List<Triangle2D>();
            result.Add(new Triangle2D(segment2Ds[0][0], segment2Ds[0][1], segment2Ds[1][1]));
            result.Add(new Triangle2D(segment2Ds[2][0], segment2Ds[2][1], segment2Ds[3][1]));
            return result;
        }

        public static List<Triangle2D> Triangulate(this Triangle2D triangle2D)
        {
            if (triangle2D == null)
                return null;

            return new List<Triangle2D>() { new Triangle2D(triangle2D) };
        }

        public static List<Triangle2D> Triangulate(this Circle2D circle2D, int density)
        {
            if(circle2D == null)
            {
                return null;
            }

            double factor = System.Math.PI / density;

            List<Point2D> point2Ds = new List<Point2D>();
            for (int i = 0; i <= density; i++)
            {
                double value = i * factor;

                Point2D point2D = circle2D.GetPoint2D(value);
                if(point2D == null)
                {
                    continue;
                }

                point2Ds.Add(point2D);
            }

            if(point2Ds == null || point2Ds.Count < 2)
            {
                return null;
            }

            List<Triangle2D> result = new List<Triangle2D>();
            for (int i=0; i < point2Ds.Count - 1; i++)
            {
                result.Add(new Triangle2D(circle2D.Center, point2Ds[i], point2Ds[i + 1]));
            }

            return result;
        }

        public static List<Triangle2D> Triangulate<T>(this T geometry2D, IEnumerable<Point2D> point2Ds, double tolerance = Core.Tolerance.MicroDistance) where T : ISegmentable2D, IClosed2D
        {
            if (geometry2D == null)
            {
                return null;
            }

            BoundingBox2D boundingBox2D = geometry2D.GetBoundingBox();
            if (boundingBox2D == null)
            {
                return null;
            }

            List<Point2D> point2Ds_Triangulate = new List<Point2D>();
            if (point2Ds != null)
            {
                foreach (Point2D point2D in point2Ds)
                {
                    if (!boundingBox2D.InRange(point2D, tolerance))
                    {
                        continue;
                    }

                    if (!geometry2D.InRange(point2D, tolerance))
                    {
                        continue;
                    }

                    point2Ds_Triangulate.Add(point2D);
                }
            }


            List<Point2D> point2Ds_Geometry2D = geometry2D.GetPoints();
            if (point2Ds_Geometry2D != null)
            {
                point2Ds_Triangulate.AddRange(point2Ds_Geometry2D);
            }

            List<Triangle2D> result = Triangulate(point2Ds_Triangulate, tolerance);
            for (int i = result.Count - 1; i >= 0; i--)
            {
                Triangle2D triangle2D = result[i];
                if (triangle2D == null)
                {
                    result.RemoveAt(i);
                    continue;
                }

                Point2D point2D = triangle2D.GetCentroid();
                if (!boundingBox2D.InRange(point2D, tolerance))
                {
                    result.RemoveAt(i);
                    continue;
                }

                if (!geometry2D.InRange(point2D, tolerance))
                {
                    result.RemoveAt(i);
                    continue;
                }
            }

            return result;
        }

        public static List<Triangle2D> Triangulate(this Face2D face2D, IEnumerable<Point2D> point2Ds, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (face2D == null)
            {
                return null;
            }

            BoundingBox2D boundingBox2D = face2D.GetBoundingBox();
            if (boundingBox2D == null)
            {
                return null;
            }

            IClosed2D externalEdge = face2D.ExternalEdge2D;
            if (externalEdge == null)
            {
                return null;
            }

            ISegmentable2D segmentable2D = externalEdge as ISegmentable2D;
            if (segmentable2D == null)
            {
                throw new NotImplementedException();
            }

            List<Point2D> point2Ds_Triangulate = new List<Point2D>();
            List<Point2D> point2Ds_Temp = null;

            point2Ds_Temp = segmentable2D.GetPoints();
            if (point2Ds_Temp == null)
            {
                return null;
            }

            point2Ds_Triangulate.AddRange(point2Ds_Temp);

            List<Tuple<BoundingBox2D, IClosed2D, Face2D>> tuples_InternalEdge = null;

            List<IClosed2D> internalEdges = face2D.InternalEdge2Ds;
            if (internalEdges != null && internalEdges.Count != 0)
            {
                tuples_InternalEdge = new List<Tuple<BoundingBox2D, IClosed2D, Face2D>>();

                foreach (IClosed2D internalEdge in internalEdges)
                {
                    if (internalEdge == null)
                    {
                        continue;
                    }

                    segmentable2D = internalEdge as ISegmentable2D;
                    if (segmentable2D == null)
                    {
                        throw new NotImplementedException();
                    }

                    point2Ds_Temp = segmentable2D.GetPoints();
                    if (point2Ds_Temp == null)
                    {
                        continue;
                    }

                    point2Ds_Triangulate.AddRange(point2Ds_Temp);

                    tuples_InternalEdge.Add(new Tuple<BoundingBox2D, IClosed2D, Face2D>(internalEdge.GetBoundingBox(), internalEdge, new Face2D(internalEdge)));
                }
            }

            if (point2Ds != null && point2Ds.Count() != 0)
            {
                foreach (Point2D point2D in point2Ds)
                {
                    if (!boundingBox2D.InRange(point2D, tolerance))
                    {
                        continue;
                    }

                    if (!face2D.InRange(point2D, tolerance))
                    {
                        continue;
                    }

                    point2Ds_Triangulate.Add(point2D);
                }
            }

            List<Triangle2D> result = Triangulate(point2Ds_Triangulate, tolerance);
            for (int i = result.Count - 1; i >= 0; i--)
            {
                Triangle2D triangle2D = result[i];
                if (triangle2D == null)
                {
                    result.RemoveAt(i);
                    continue;
                }

                if (tuples_InternalEdge != null)
                {
                    BoundingBox2D boundingBox2D_Triangle2D = triangle2D.GetBoundingBox();
                    Face2D face2D_Triangle = new Face2D(triangle2D);

                    List<Face2D> face2Ds_InternalEdge = tuples_InternalEdge.FindAll(x => x.Item1.InRange(boundingBox2D_Triangle2D, tolerance)).ConvertAll(x => x.Item3);
                    if (face2Ds_InternalEdge != null && face2Ds_InternalEdge.Count != 0)
                    {
                        List<Face2D> face2Ds_Difference = face2D_Triangle.Difference(face2Ds_InternalEdge, tolerance);
                        if (face2Ds_Difference != null)
                        {
                            foreach (Face2D face2D_Difference in face2Ds_Difference)
                            {
                                List<Triangle2D> triangle2Ds = face2D_Difference?.Triangulate(tolerance);
                                if (triangle2Ds != null && triangle2Ds.Count != 0)
                                {
                                    result.AddRange(triangle2Ds);
                                }
                            }
                        }
                        result.RemoveAt(i);
                        continue;
                    }
                }
            }

            for (int i = result.Count - 1; i >= 0; i--)
            {
                Triangle2D triangle2D = result[i];
                if (triangle2D == null)
                {
                    result.RemoveAt(i);
                    continue;
                }

                Point2D point2D = triangle2D.GetCentroid();
                if (!boundingBox2D.InRange(point2D, tolerance))
                {
                    result.RemoveAt(i);
                    continue;
                }

                if (!face2D.Inside(point2D, tolerance))
                {
                    result.RemoveAt(i);
                    continue;
                }
            }

            return result;
        }

        private static List<Polygon> Triangulate(this Polygon polygon, double tolerance = Core.Tolerance.MicroDistance)
        {
            if(polygon == null)
            {
                return null;
            }

            GeometryFactory geometryFactory = new GeometryFactory(new PrecisionModel(1 / tolerance));

            GeometryCollection geometryCollection = null;

            if (polygon.Holes != null && polygon.Holes.Length != 0)
            {
                NetTopologySuite.Geometries.Geometry geometry = WorkGeometry(polygon);
                if (geometry == null)
                {
                    geometry = polygon;
                }

                if (geometry == null)
                {
                    return null;
                }

                ConformingDelaunayTriangulationBuilder conformingDelaunayTriangulationBuilder = new ConformingDelaunayTriangulationBuilder();

                conformingDelaunayTriangulationBuilder.SetSites(geometry);
                conformingDelaunayTriangulationBuilder.Constraints = geometry;

                geometryCollection = conformingDelaunayTriangulationBuilder.GetTriangles(geometryFactory);
                if (geometryCollection == null)
                {
                    return null;
                }

                geometry = FilterRelevant(polygon, geometryCollection);

                geometryCollection = geometry is GeometryCollection ? (GeometryCollection)geometry : new GeometryCollection(new NetTopologySuite.Geometries.Geometry[] { geometry }, geometryFactory);
            }
            else
            {
                Coordinate[] coordinates = polygon.Coordinates;
                if(coordinates == null || coordinates.Length < 3)
                {
                    return null;
                }

                if(coordinates.Length == 3)
                {
                    return new List<Polygon>() { polygon };
                }

                DelaunayTriangulationBuilder delaunayTriangulationBuilder = new DelaunayTriangulationBuilder();
                delaunayTriangulationBuilder.SetSites(polygon);

                geometryCollection = delaunayTriangulationBuilder.GetTriangles(geometryFactory);
            }

            if(geometryCollection == null)
            {
                return null;
            }

            List<Polygon> polygons = new List<Polygon>();
            foreach (NetTopologySuite.Geometries.Geometry geometry_Temp in geometryCollection.Geometries)
            {
                Polygon polygon_Temp = geometry_Temp as Polygon;
                if (polygon == null)
                {
                    continue;
                }

                polygons.Add(polygon_Temp);
            }

            List<Polygon> result = new List<Polygon>();
            foreach (Polygon polygon_Temp in polygons)
            {
                NetTopologySuite.Geometries.Geometry geometry_Intersection = polygon.Intersection(polygon_Temp);

                List<Polygon> polygons_Intersection = new List<Polygon>();
                if(geometry_Intersection is Polygon)
                {
                    polygons_Intersection.Add((Polygon)geometry_Intersection);
                }
                else if(geometry_Intersection is GeometryCollection)
                {
                    foreach(NetTopologySuite.Geometries.Geometry geometry_Temp in (GeometryCollection)geometry_Intersection)
                    {
                        if(geometry_Temp is Polygon)
                        {
                            polygons_Intersection.Add((Polygon)geometry_Temp);
                        }
                    }
                }

                foreach(Polygon polygon_Intersection in polygons_Intersection)
                {
                    if (Core.Query.AlmostEqual(polygon_Temp.Area, polygon_Intersection.Area, tolerance))
                    {
                        result.Add(polygon_Intersection);
                        continue;
                    }

                    List<Polygon> polygons_Temp_Temp = Triangulate(polygon_Intersection, tolerance);
                    if (polygons_Temp_Temp == null || polygons_Temp_Temp.Count == 0)
                    {
                        continue;
                    }

                    result.AddRange(polygons_Temp_Temp);
                }
            }

            return result;
        }

    }
}