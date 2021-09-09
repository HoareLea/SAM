using System.Collections.Generic;
using NetTopologySuite.Triangulate;
using NetTopologySuite.Geometries;
using System.Linq;

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

            if(point2Ds.Count() < 3)
            {
                return new List<Triangle2D>();
            }

            List<Coordinate> coordinates = new List<Coordinate>();
            foreach(Point2D point2D in point2Ds)
            {
                Coordinate coordinate = point2D.ToNTS(tolerance);
                if(coordinate == null)
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

            Polygon polygon = face2D.ToNTS(tolerance);

            DelaunayTriangulationBuilder delaunayTriangulationBuilder = new DelaunayTriangulationBuilder();
            delaunayTriangulationBuilder.SetSites(polygon);

            GeometryFactory geometryFactory = new GeometryFactory(new PrecisionModel(1 / tolerance));

            GeometryCollection geometryCollection =  delaunayTriangulationBuilder.GetTriangles(geometryFactory);
            if (geometryCollection == null)
                return null;

            List<Triangle2D> result = new List<Triangle2D>();
            foreach(NetTopologySuite.Geometries.Geometry geometry in geometryCollection.Geometries)
            {
                Polygon polygon_Temp = geometry as Polygon;
                if (polygon == null)
                    continue;

                if (!polygon.Contains(polygon_Temp.Centroid))
                    continue;

                Coordinate[] coordinates = polygon_Temp.Coordinates;
                if (coordinates == null || coordinates.Length != 4)
                    continue;

                result.Add(new Triangle2D(coordinates[0].ToSAM(tolerance), coordinates[1].ToSAM(), coordinates[2].ToSAM(tolerance)));
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
            if(face2D == null)
            {
                return null;
            }

            BoundingBox2D boundingBox2D = face2D.GetBoundingBox();
            if (boundingBox2D == null)
            {
                return null;
            }

            IClosed2D externalEdge = face2D.ExternalEdge2D;
            if(externalEdge == null)
            {
                return null;
            }

            ISegmentable2D segmentable2D = externalEdge as ISegmentable2D;
            if(segmentable2D == null)
            {
                throw new System.NotImplementedException();
            }

            List <Point2D> point2Ds_Triangulate = new List<Point2D>();
            List<Point2D> point2Ds_Temp = null;

            point2Ds_Temp = segmentable2D.GetPoints();
            if(point2Ds_Temp == null)
            {
                return null;
            }

            point2Ds_Triangulate.AddRange(point2Ds_Temp);

            List<IClosed2D> internalEdges = face2D.InternalEdge2Ds;
            if(internalEdges != null && internalEdges.Count != 0)
            {
                foreach(IClosed2D internalEdge in internalEdges)
                {
                    if(internalEdge == null)
                    {
                        continue;
                    }

                    segmentable2D = internalEdge as ISegmentable2D;
                    if (segmentable2D == null)
                    {
                        throw new System.NotImplementedException();
                    }

                    point2Ds_Temp = segmentable2D.GetPoints();
                    if (point2Ds_Temp == null)
                    {
                        continue;
                    }

                    point2Ds_Triangulate.AddRange(point2Ds_Temp);
                }
            }

            if(point2Ds != null && point2Ds.Count() != 0)
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
    }
}