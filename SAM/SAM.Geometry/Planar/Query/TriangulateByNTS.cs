using System.Collections.Generic;
using NetTopologySuite.Triangulate;
using NetTopologySuite.Geometries;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Triangle2D> TriangulateByNTS(this Polygon2D polygon2D, double tolerance = Core.Tolerance.MicroDistance)
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
    }
}