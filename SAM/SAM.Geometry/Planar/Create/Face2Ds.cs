using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static List<Face2D> Face2Ds<T>(this IEnumerable<T> closed2D, double tolerance = Core.Tolerance.MicroDistance) where T : IClosed2D
        {
            if (closed2D == null)
            {
                return null;
            }

            List<ISegmentable2D> segmentable2Ds = new List<ISegmentable2D>();
            foreach (T closed in closed2D)
            {
                if (closed == null)
                {
                    continue;
                }

                ISegmentable2D segmentable2D = closed as ISegmentable2D;
                if (segmentable2D == null)
                {
                    throw new System.NotImplementedException();
                }

                segmentable2Ds.Add(segmentable2D);
            }

            return Face2Ds(segmentable2Ds, EdgeOrientationMethod.Undefined, tolerance);
        }

        public static List<Face2D> Face2Ds<T>(this IEnumerable<T> segmentable2Ds, EdgeOrientationMethod edgeOrientationMethod = EdgeOrientationMethod.Undefined, double tolerance = Core.Tolerance.MicroDistance) where T: ISegmentable2D
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

            List<Face2D> result = new List<Face2D>();
            foreach (Polygon polygon in polygons)
            {
                Face2D face2D = polygon?.ToSAM(tolerance);
                if (face2D == null || !face2D.IsValid())
                {
                    continue;
                }

                if(edgeOrientationMethod != EdgeOrientationMethod.Undefined)
                {
                    face2D = Face2D(face2D.ExternalEdge2D, face2D.InternalEdge2Ds, edgeOrientationMethod);
                }

                result.Add(face2D);
            }

            return result;
        }
    }
}