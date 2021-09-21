using System;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Face2D> SelfIntersectionFace2Ds(this Face2D face2D, double maxLength, double tolerance = Core.Tolerance.MicroDistance)
        {
            List<IClosed2D> edge2Ds = face2D?.Edge2Ds;
            if(edge2Ds == null || edge2Ds.Count == 0)
            {
                return null;
            }

            List<Segment2D> segment2Ds = new List<Segment2D>();
            foreach(IClosed2D edge2D in edge2Ds)
            {
                if(edge2D == null)
                {
                    continue;
                }

                ISegmentable2D segmentable2D = edge2D as ISegmentable2D;
                if(segmentable2D == null)
                {
                    throw new NotImplementedException();
                }

                List<Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();
                if(segment2Ds_Temp == null)
                {
                    continue;
                }

                segment2Ds.AddRange(segment2Ds_Temp);
            }


            segment2Ds = SelfIntersectionSegment2Ds(segment2Ds, maxLength, tolerance);
            if (segment2Ds == null || segment2Ds.Count < 2)
            {
                return null;
            }

            segment2Ds = segment2Ds.Split(tolerance);
            segment2Ds = segment2Ds.Snap(true, tolerance);

            for(int i = segment2Ds.Count - 1; i >= 0; i--)
            {
                Point2D point2D = segment2Ds[i]?.Mid();
                if(point2D == null)
                {
                    segment2Ds.RemoveAt(i);
                    continue;
                }

                if(face2D.On(point2D, tolerance))
                {
                    continue;
                }

                if(face2D.Inside(point2D, tolerance))
                {
                    continue;
                }

                segment2Ds.RemoveAt(i);
            }

            List<Polygon2D> polygon2Ds = Create.Polygon2Ds(segment2Ds, tolerance);
            if(polygon2Ds == null)
            {
                return null;
            }

            List<IClosed2D> internalEdge2Ds = face2D.InternalEdge2Ds;
            if(internalEdge2Ds == null || internalEdge2Ds.Count == 0 || polygon2Ds.Count < 2)
            {
                return polygon2Ds.ConvertAll(x => new Face2D(x));
            }

            polygon2Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

            List<Face2D> result = new List<Face2D>();

            List<Tuple<Polygon2D, Point2D>> tuples_External = polygon2Ds.ConvertAll(x => new Tuple<Polygon2D, Point2D>(x, x.InternalPoint2D(tolerance)));
            List<Tuple<Polygon2D, Point2D>> tuples_Internal = tuples_External.FindAll(x => !face2D.Inside(x.Item2, tolerance));
            tuples_External.RemoveAll(x => tuples_Internal.Contains(x));
            while(tuples_External.Count > 0)
            {
                Tuple<Polygon2D, Point2D> tuple_External = tuples_External[0];
                tuples_External.RemoveAt(0);

                Face2D face2D_Temp = Face2D.Create(tuple_External.Item1, tuples_Internal.FindAll(x => tuple_External.Item1.Inside(x.Item2, tolerance)).ConvertAll(x => x.Item1), EdgeOrientationMethod.Opposite, tolerance);
                if(face2D_Temp == null)
                {
                    continue;
                }

                result.Add(face2D_Temp);
            }

            return result;
        }
    }
}