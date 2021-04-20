using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Face2D FixEdges(this Face2D face2D, double tolerance = Core.Tolerance.Distance)
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

            if (polygon2Ds.Count > 1)
                polygon2Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

            IClosed2D externalEdge = polygon2Ds[0];

            List<IClosed2D> internalEdges = face2D.InternalEdge2Ds;
            if(internalEdges != null && internalEdges.Count != 0)
            {
                for(int i=0; i < internalEdges.Count; i++)
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

                    if (polygon2Ds.Count > 1)
                        polygon2Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

                    internalEdges[i] = polygon2Ds[0];
                }
            }

            return Face2D.Create(externalEdge, internalEdges, true);
        }
    }
}