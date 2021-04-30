using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static List<Polygon2D> Polygon2Ds(this IEnumerable<ISegmentable2D> segmentable2Ds, double tolerance = Core.Tolerance.MicroDistance)
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

            List<Polygon> polygons = segment2Ds.ToNTS_Polygons(tolerance);
            if (polygons == null)
                return null;

            List<Polygon2D> result = new List<Polygon2D>();
            foreach (Polygon polygon in polygons)
            {
                List<Polygon2D> polygon2Ds = polygon.ToSAM_Polygon2Ds();
                if (polygon2Ds == null)
                    continue;

                //result.AddRange(polygon2Ds);

                //Removing duplicated polygon2Ds
                foreach (Polygon2D polygon2D in polygon2Ds)
                    if (result.Find(x => x.Similar(polygon2D)) == null)
                        result.AddRange(polygon2Ds);
            }

            return result;
        }

        public static List<Polygon2D> Polygon2Ds(this IEnumerable<ISegmentable2D> segmentable2Ds, bool split, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (segmentable2Ds == null)
                return null;

            if (split)
                return Polygon2Ds(segmentable2Ds.Split(tolerance), tolerance);

            return Polygon2Ds(segmentable2Ds, tolerance);
        }

        public static List<Polygon2D> Polygon2Ds(this IEnumerable<ISegmentable2D> segmentable2Ds, double maxDistance, double snapTolerance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.MicroDistance)
        {
            List<Segment2D> segment2Ds = Segment2Ds(segmentable2Ds, maxDistance, snapTolerance, tolerance);
            if(segment2Ds == null || segment2Ds.Count == 0)
            {
                return null;
            }

            return Polygon2Ds(segment2Ds, snapTolerance);
        }
    }
}