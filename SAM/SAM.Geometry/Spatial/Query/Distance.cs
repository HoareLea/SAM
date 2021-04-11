using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static double Distance(this IClosedPlanar3D closedPlanar3D_1, IClosedPlanar3D closedPlanar3D_2, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (closedPlanar3D_1 == null || closedPlanar3D_2 == null)
            {
                return double.NaN;
            }

            IClosedPlanar3D closedPlanar3D_1_Temp = closedPlanar3D_1;
            if(closedPlanar3D_1_Temp is Face3D)
            {
                closedPlanar3D_1_Temp = ((Face3D)closedPlanar3D_1_Temp).GetExternalEdge3D();
            }

            IClosedPlanar3D closedPlanar3D_2_Temp = closedPlanar3D_2;
            if (closedPlanar3D_2_Temp is Face3D)
            {
                closedPlanar3D_2_Temp = ((Face3D)closedPlanar3D_2_Temp).GetExternalEdge3D();
            }

            PlanarIntersectionResult planarIntersectionResult = Create.PlanarIntersectionResult(new Face3D(closedPlanar3D_1_Temp), new Face3D(closedPlanar3D_2_Temp), tolerance_Angle, tolerance_Distance);
            if(planarIntersectionResult != null && planarIntersectionResult.Intersecting)
            {
                return 0;
            }

            ISegmentable3D segmentable3D_1 = closedPlanar3D_1_Temp as ISegmentable3D;
            if(segmentable3D_1 == null)
            {
                throw new System.NotImplementedException();
            }

            ISegmentable3D segmentable3D_2 = closedPlanar3D_2_Temp as ISegmentable3D;
            if (segmentable3D_2 == null)
            {
                throw new System.NotImplementedException();
            }

            return Distance(segmentable3D_1, segmentable3D_2, tolerance_Distance);
        }

        public static double Distance(this ISegmentable3D segmentable3D_1, ISegmentable3D segmentable3D_2, double tolerance = Core.Tolerance.Distance)
        {
            List<Segment3D> segment3Ds_1 = segmentable3D_1.GetSegments();
            if (segment3Ds_1 == null || segment3Ds_1.Count == 0)
            {
                return double.NaN;
            }

            List<Segment3D> segment3Ds_2 = segmentable3D_2.GetSegments();
            if (segment3Ds_2 == null || segment3Ds_2.Count == 0)
            {
                return double.NaN;
            }

            double result = double.MaxValue;
            foreach(Segment3D segment3D_1 in segment3Ds_1)
            {
                foreach (Segment3D segment3D_2 in segment3Ds_2)
                {
                    double distance = segment3D_1.Distance(segment3D_2);
                    if (distance < result)
                    {
                        result = distance;
                    }
                }
            }

            return result;
        }

        public static double Distance(this ISegmentable3D segmentable3D, Point3D point3D)
        {
            if(segmentable3D == null || point3D == null)
            {
                return double.NaN;
            }

            List<Segment3D> segment3Ds = segmentable3D.GetSegments();
            if(segment3Ds == null || segment3Ds.Count == 0)
            {
                return double.NaN;
            }

            double result = double.MaxValue;
            foreach(Segment3D segment3D in segment3Ds)
            {
                double distance = segment3D.Distance(point3D);
                if(distance < result)
                {
                    result = distance;
                }
            }

            return result;
        }
    }
}