using NetTopologySuite.Geometries;
using NetTopologySuite.Noding.Snapround;
using NetTopologySuite.Operation.Polygonize;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static List<Polygon2D> Polygon2Ds(this IEnumerable<ISegmentable2D> segmentable2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable2Ds == null)
                return null;

            List<Segment2D> segment2Ds = new List<Segment2D>();
            foreach (ISegmentable2D segmentable2D in segmentable2Ds)
            {
                if (segmentable2D == null)
                    continue;

                List<Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();
                if (segment2Ds_Temp != null && segment2Ds_Temp.Count > 0)
                    segment2Ds.AddRange(segment2Ds_Temp);
            }

            segment2Ds.RemoveAll(x => x == null || x.GetLength() < tolerance);
            segment2Ds = Modify.Split(segment2Ds, tolerance);

            //PointGraph2D pointGraph2D = new PointGraph2D(segment2Ds);
            //return pointGraph2D.GetPolygon2Ds_External();

            Polygonizer polygonizer = new Polygonizer(false);
            GeometryNoder geometryNoder = new GeometryNoder(new PrecisionModel(1 / tolerance));
            polygonizer.Add(geometryNoder.Node(segment2Ds.ConvertAll(x => x.ToNetTopologySuite())).ToArray());

            IEnumerable<NetTopologySuite.Geometries.Geometry> polygons = polygonizer.GetPolygons();
            if (polygons == null)
                return null;

            return polygons.ToList().FindAll(x => x is Polygon).Cast<Polygon>().ToList().ConvertAll(x => x.ToSAM());
        }
    }
}
