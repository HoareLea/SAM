using NetTopologySuite.Geometries;
using NetTopologySuite.Noding.Snapround;
using NetTopologySuite.Operation.Polygonize;
using SAM.Geometry.Planar;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static List<Polygon> ToNetTopologySuite_Polygons(this IEnumerable<Segment2D> segment2Ds, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (segment2Ds == null)
                return null;

            if (segment2Ds.Count() == 0)
                return new List<Polygon>();

            return ToNetTopologySuite_Polygons(segment2Ds.ToList().ConvertAll(x => x.ToNetTopologySuite(tolerance)), tolerance);
        }

        public static List<Polygon> ToNetTopologySuite_Polygons(this IEnumerable<NetTopologySuite.Geometries.Geometry> geometries, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (geometries == null)
                return null;

            if (geometries.Count() == 0)
                return new List<Polygon>();

            Polygonizer polygonizer = new Polygonizer(false);
            GeometryNoder geometryNoder = new GeometryNoder(new PrecisionModel(1 / tolerance));
            polygonizer.Add(geometryNoder.Node(geometries).ToArray());

            IEnumerable<NetTopologySuite.Geometries.Geometry> geometries_Result = polygonizer.GetPolygons();
            if (geometries_Result == null)
                return null;

            List<Polygon> result = new List<Polygon>();
            foreach (NetTopologySuite.Geometries.Geometry geometry in geometries_Result)
            {
                Polygon polygon = geometry as Polygon;
                if (polygon == null)
                    continue;

                result.Add(polygon);
            }

            return result;
        }
    }
}
