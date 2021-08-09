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

            //Fix for the case when externalEdge selfintersect
            List<Polygon2D> polygon2Ds_ExternalEdge = Create.Polygon2Ds(segment2Ds, tolerance);
            if (polygon2Ds_ExternalEdge == null || polygon2Ds_ExternalEdge.Count == 0)
                return null;

            List<Polygon2D> polygon2Ds_InternalEdge = null;

            List<IClosed2D> internalEdges = face2D.InternalEdge2Ds;
            if (internalEdges != null && internalEdges.Count != 0)
            {
                polygon2Ds_InternalEdge = new List<Polygon2D>();
                for (int i = 0; i < internalEdges.Count; i++)
                {
                    segmentable2D = internalEdges[i] as ISegmentable2D;
                    if (segmentable2D == null)
                        continue;

                    segment2Ds = segmentable2D.GetSegments()?.Split(tolerance);
                    if (segment2Ds == null || segment2Ds.Count == 0)
                        continue;

                    List<Polygon2D> polygon2Ds_InternalEdge_Temp = Create.Polygon2Ds(segment2Ds, tolerance);
                    if (polygon2Ds_InternalEdge_Temp == null || polygon2Ds_InternalEdge_Temp.Count == 0)
                        continue;

                    polygon2Ds_InternalEdge_Temp.ForEach(x => polygon2Ds_InternalEdge.Add(x));
                }
            }

            List<Face2D> result = new List<Face2D>();
            foreach(Polygon2D polygon2D_ExternalEdge in polygon2Ds_ExternalEdge)
            {
                List<Polygon2D> polygon2Ds_ExternalEdge_Temp = new List<Polygon2D>() { polygon2D_ExternalEdge };

                List<Polygon2D> polygon2Ds_InternalEdge_Temp = polygon2Ds_InternalEdge?.FindAll(x => polygon2D_ExternalEdge.Inside(x.InternalPoint2D(tolerance)));
                if(polygon2Ds_InternalEdge_Temp != null && polygon2Ds_InternalEdge_Temp.Count != 0)
                {
                    //Fix for the case when externalEdge and internal edge intersect
                    List<Segment2D> segment2Ds_All = polygon2D_ExternalEdge.GetSegments();
                    polygon2Ds_InternalEdge_Temp.ForEach(x => segment2Ds_All.AddRange(x.GetSegments()));
                    segment2Ds_All = segment2Ds_All.Split(tolerance);
                    segment2Ds_All = segment2Ds_All.Snap(true, tolerance);

                    polygon2Ds_ExternalEdge_Temp = Create.Polygon2Ds(segment2Ds_All, tolerance);
                    if(polygon2Ds_ExternalEdge_Temp == null || polygon2Ds_ExternalEdge_Temp.Count == 0)
                    {
                        continue;
                    }

                    for(int i = polygon2Ds_ExternalEdge_Temp.Count - 1; i >= 0; i--)
                    {
                        Polygon2D polygon2D = polygon2Ds_InternalEdge_Temp.Find(x => polygon2Ds_ExternalEdge_Temp[i].Inside(x.InternalPoint2D(tolerance)));
                        if(polygon2D != null)
                        {
                            polygon2Ds_ExternalEdge_Temp.RemoveAt(i);
                        }
                    }
                }

                foreach(Polygon2D polygon2D_ExternalEdge_Temp in polygon2Ds_ExternalEdge_Temp)
                {
                    Face2D face2D_New = Face2D.Create(polygon2D_ExternalEdge_Temp, polygon2Ds_InternalEdge_Temp, EdgeOrientationMethod.Opposite, tolerance);
                    if (face2D_New != null)
                        result.Add(face2D_New);
                }
            }

            return result;
        }
    }
}