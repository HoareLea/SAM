using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Face2D> FixEdges(this Face2D face2D, double tolerance = Core.Tolerance.Distance)
        {
            List<Point2D> point2Ds = (face2D?.ExternalEdge2D as ISegmentable2D)?.GetPoints();
            if(point2Ds == null || point2Ds.Count < 3)
            {
                return null;
            }

            //Fix for the case when externalEdge selfintersect
            List<Polygon2D> polygon2Ds_ExternalEdge = new Polygon2D(point2Ds)?.FixEdges(tolerance);
            if (polygon2Ds_ExternalEdge == null || polygon2Ds_ExternalEdge.Count == 0)
            {
                return null;
            }

            List<Polygon2D> polygon2Ds_InternalEdge = null;

            List<IClosed2D> internalEdges = face2D.InternalEdge2Ds;
            if (internalEdges != null && internalEdges.Count != 0)
            {
                polygon2Ds_InternalEdge = new List<Polygon2D>();
                for (int i = 0; i < internalEdges.Count; i++)
                {
                    point2Ds = (internalEdges[i] as ISegmentable2D)?.GetPoints();
                    if (point2Ds == null || point2Ds.Count < 3)
                    {
                        continue;
                    }

                    List<Polygon2D> polygon2Ds_InternalEdge_Temp = new Polygon2D(point2Ds)?.FixEdges(tolerance);
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
                if(polygon2Ds_InternalEdge_Temp == null || polygon2Ds_InternalEdge_Temp.Count == 0)
                {
                    Face2D face2D_New = Face2D.Create(polygon2D_ExternalEdge, new IClosed2D[] { }, EdgeOrientationMethod.Opposite, tolerance);
                    if(face2D_New != null)
                    {
                        result.Add(face2D_New);
                    }
                    continue;
                }

                //Fix for the case when externalEdge and internal edge intersect
                List<Segment2D> segment2Ds_All = polygon2D_ExternalEdge.GetSegments();
                polygon2Ds_InternalEdge_Temp.ForEach(x => segment2Ds_All.AddRange(x.GetSegments()));
                segment2Ds_All = segment2Ds_All.Split(tolerance);
                segment2Ds_All = segment2Ds_All.Snap(true, tolerance);

                List<Face2D> face2Ds = Create.Face2Ds(segment2Ds_All, EdgeOrientationMethod.Undefined, tolerance);
                if (face2Ds == null || face2Ds.Count == 0)
                {
                    continue;
                }

                if (face2Ds.Count == 1)
                {
                    result.Add(face2Ds.FirstOrDefault());
                }
                else
                {
                    face2Ds.Sort((x, y) => y.ExternalEdge2D.GetBoundingBox().GetArea().CompareTo(x.ExternalEdge2D.GetBoundingBox().GetArea()));
                    IClosed2D externalEdge = face2Ds[0].ExternalEdge2D;
                    face2Ds.RemoveAt(0);
                    List<IClosed2D> internalEdges_Temp = new List<IClosed2D>();
                    face2Ds.ForEach(x => internalEdges_Temp.AddRange(x.Edge2Ds));

                    Face2D face2D_New = Face2D.Create(externalEdge, internalEdges_Temp, EdgeOrientationMethod.Opposite, tolerance);
                    if (face2D_New == null)
                    {
                        continue;
                    }

                    result.Add(face2D_New);
                }
            }

            return result;
        }

        public static List<Polygon2D> FixEdges(this Polygon2D polygon2D, double tolerance = Core.Tolerance.Distance)
        {
            if(polygon2D == null)
            {
                return null;
            }

            List<Point2D> point2Ds = polygon2D.GetPoints();
            if(point2Ds == null || point2Ds.Count < 3)
            {
                return null;
            }

            for (int i = point2Ds.Count - 1; i > 0; i--)
            {
                if(!point2Ds[i].AlmostEquals(point2Ds[i - 1], tolerance))
                {
                    continue;
                }

                point2Ds.RemoveAt(i);
            }

            if(point2Ds.Count < 3)
            {
                return null;
            }

            if(point2Ds[0].AlmostEquals(point2Ds[point2Ds.Count - 1], tolerance))
            {
                point2Ds.RemoveAt(point2Ds.Count - 1);
            }

            if (point2Ds.Count < 3)
            {
                return null;
            }

            List<Segment2D> segment2Ds = point2Ds.Segment2Ds(true);
            if(segment2Ds == null || segment2Ds.Count < 3)
            {
                return null;
            }

            List<Segment2D> segment2Ds_Temp = new List<Segment2D>();
            for(int i= segment2Ds.Count - 1; i >= 0; i--)
            {
                Segment2D segment2D = segment2Ds[i];

                segment2Ds.RemoveAt(i);

                if(segment2D.GetLength() < tolerance)
                {
                    continue;
                }

                if(segment2Ds.Find(x => segment2D.AlmostSimilar(x, tolerance)) != null)
                {
                    continue;
                }

                segment2Ds_Temp.Add(segment2D);
            }

            segment2Ds = Split(segment2Ds_Temp, tolerance);

            segment2Ds = Snap(segment2Ds, true, tolerance);
            if (segment2Ds == null || segment2Ds.Count < 3)
            {
                return null;
            }

            return Create.Polygon2Ds(segment2Ds, tolerance);
        }
    }
}