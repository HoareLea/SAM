using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Face2D> FixEdges(this Face2D face2D, double tolerance = Core.Tolerance.Distance)
        {
            ISegmentable2D segmentable2D = face2D?.ExternalEdge2D as ISegmentable2D;
            if (segmentable2D == null)
                return null;

            List<Segment2D> segment2Ds = segmentable2D.GetSegments()?.Split(tolerance);
            if (segment2Ds == null || segment2Ds.Count == 0)
                return null;

            List<Polygon2D> polygon2Ds = Create.Polygon2Ds(segment2Ds, tolerance);
            if (polygon2Ds == null || polygon2Ds.Count == 0)
                return null;

            List<IClosed2D> internalEdges = face2D.InternalEdge2Ds;
            if (internalEdges != null && internalEdges.Count != 0)
            {
                List<IClosed2D> internalEdges_New = new List<IClosed2D>();
                for (int i = 0; i < internalEdges.Count; i++)
                {
                    segmentable2D = face2D?.ExternalEdge2D as ISegmentable2D;
                    if (segmentable2D == null)
                        continue;

                    segment2Ds = segmentable2D.GetSegments()?.Split(tolerance);
                    if (segment2Ds == null || segment2Ds.Count == 0)
                        continue;

                    polygon2Ds = Create.Polygon2Ds(segment2Ds, tolerance);
                    if (polygon2Ds == null || polygon2Ds.Count == 0)
                        continue;

                    polygon2Ds.ForEach(x => internalEdges_New.Add(x));
                }

                internalEdges = internalEdges_New;
            }

            List<Face2D> result = new List<Face2D>();
            foreach(Polygon2D externalEdge in polygon2Ds)
            {
                Face2D face2D_New = Face2D.Create(externalEdge, internalEdges, true);
                if (face2D_New != null)
                    result.Add(face2D_New);
            }

            return result;
        }
    }
}