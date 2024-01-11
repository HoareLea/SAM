using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Segment3D MaxIntersectionSegment3D(this Plane plane, IClosedPlanar3D closedPlanar3D)
        {
            if(plane == null || closedPlanar3D == null)
            {
                return null;
            }

            if(closedPlanar3D is Face3D)
            {
                return MaxIntersectionSegment3D(plane, ((Face3D)closedPlanar3D).GetExternalEdge3D());
            }

            if(!(closedPlanar3D is ISegmentable3D))
            {
                throw new System.NotImplementedException();
            }

            PlanarIntersectionResult planarIntersectionResult = Create.PlanarIntersectionResult(plane, closedPlanar3D);
            if(planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
            {
                return null;
            }

            List<ISegmentable3D> segmentable3Ds = planarIntersectionResult.GetGeometry3Ds<ISegmentable3D>();
            if(segmentable3Ds == null || segmentable3Ds.Count == 0)
            {
                return null;
            }

            List<Point3D> point3Ds = new List<Point3D>();
            foreach(ISegmentable3D segmentable3D in segmentable3Ds)
            {
                List<Point3D> point3Ds_Temp = segmentable3D?.GetPoints();
                if(point3Ds_Temp == null)
                {
                    continue;
                }

                point3Ds.AddRange(point3Ds_Temp);
            }

            if(point3Ds == null || point3Ds.Count < 2)
            {
                return null;
            }

            point3Ds.ExtremePoints(out Point3D point3D_1, out Point3D point3D_2);
            if(point3D_1 == null || point3D_2 == null || point3D_1 == point3D_2)
            {
                return null;
            }


            return new Segment3D(point3D_1, point3D_2);
        }
    }
}