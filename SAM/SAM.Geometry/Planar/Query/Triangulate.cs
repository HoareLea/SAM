using System.Collections.Generic;
using NetTopologySuite.Triangulate;
using NetTopologySuite.Geometries;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
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
                if (polygon == null)
                    continue;

                Coordinate[] coordinates = polygon_Temp.Coordinates;
                if (coordinates == null || coordinates.Length != 4)
                    continue;

                result.Add(new Triangle2D(coordinates[0].ToSAM(tolerance), coordinates[1].ToSAM(), coordinates[2].ToSAM(tolerance)));
            }

            return result;
        }

        public static List<Triangle2D> Traingulate(this Face2D face2D, double tolerance = Core.Tolerance.MicroDistance)
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

        public static List<Triangle2D> Triangulate(this Polyline2D polyline2D, double tolerance = Core.Tolerance.Distance)
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
    }
}