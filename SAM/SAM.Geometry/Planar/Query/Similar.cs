using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static bool Similar(this Segment2D segment2D_1, Segment2D segment2D_2, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2D_1 == segment2D_2)
                return true;

            if (segment2D_1 == null || segment2D_2 == null)
                return false;

            return (segment2D_1[0].AlmostEquals(segment2D_2[0], tolerance) && segment2D_1[1].AlmostEquals(segment2D_2[1], tolerance)) || (segment2D_1[0].AlmostEquals(segment2D_2[1], tolerance) && segment2D_1[1].AlmostEquals(segment2D_2[0], tolerance));
        }

        //public static bool Similar(this Polygon2D polygon2D_1, Polygon2D polygon2D_2, double tolerance = Core.Tolerance.Distance)
        //{
        //    if (polygon2D_1 == polygon2D_2)
        //        return true;

        //    if (polygon2D_1 == null || polygon2D_2 == null)
        //        return false;

        //    List<Point2D> point2Ds = polygon2D_1.GetPoints();
        //    foreach(Point2D point2D in point2Ds)
        //        if (!polygon2D_2.On(point2D, tolerance))
        //            return false;


        //    point2Ds = polygon2D_2.GetPoints();
        //    foreach (Point2D point2D in point2Ds)
        //        if (!polygon2D_1.On(point2D, tolerance))
        //            return false;

        //    return System.Math.Abs(polygon2D_1.GetArea() - polygon2D_2.GetArea()) <= tolerance;
        //}

        public static bool Similar(Face2D face2D_1, Face2D face2D_2, double tolerance = Core.Tolerance.Distance)
        {
            if (face2D_1 == face2D_2)
            {
                return true;
            }

            if (face2D_1 == null || face2D_2 == null)
            {
                return false;
            }

            if (!Similar(face2D_1.ExternalEdge2D, face2D_2.ExternalEdge2D, tolerance))
            {
                return false;
            }

            List<IClosed2D> internalEdge2Ds_1 = face2D_1.InternalEdge2Ds;
            List<IClosed2D> internalEdge2Ds_2 = face2D_1.InternalEdge2Ds;
            if(internalEdge2Ds_1 == internalEdge2Ds_2)
            {
                return true;
            }

            if (internalEdge2Ds_1 == null || internalEdge2Ds_2 == null)
            {
                return false;
            }

            if(internalEdge2Ds_1.Count != internalEdge2Ds_2.Count)
            {
                return false;
            }

            foreach(IClosed2D internalEdge2D_1 in internalEdge2Ds_1)
            {
                int index = internalEdge2Ds_2.FindIndex(x => Similar(internalEdge2D_1, x, tolerance));
                if(index == -1)
                {
                    return false;
                }

                internalEdge2Ds_2.RemoveAt(index);
            }

            return true;
        }

        public static bool Similar(this IClosed2D closed2D_1, IClosed2D closed2D_2, double tolerance = Core.Tolerance.Distance)
        {
            if (closed2D_1 == closed2D_2)
                return true;

            if (closed2D_1 == null || closed2D_2 == null)
                return false;

            if(closed2D_1 is Face2D || closed2D_2 is Face2D)
            {
                if(closed2D_1 is Face2D && closed2D_2 is Face2D)
                {
                    return Similar((Face2D)closed2D_1, (Face2D)closed2D_2, tolerance);
                }

                throw new System.NotImplementedException();
            }

            ISegmentable2D segmentable2D_1 = closed2D_1 as ISegmentable2D;
            if(segmentable2D_1 == null)
            {
                throw new System.NotImplementedException();
            }

            ISegmentable2D segmentable2D_2 = closed2D_2 as ISegmentable2D;
            if (segmentable2D_2 == null)
            {
                throw new System.NotImplementedException();
            }

            List<Point2D> point2Ds = segmentable2D_1.GetPoints();
            foreach (Point2D point2D in point2Ds)
                if (!segmentable2D_2.On(point2D, tolerance))
                    return false;


            point2Ds = segmentable2D_2.GetPoints();
            foreach (Point2D point2D in point2Ds)
                if (!segmentable2D_1.On(point2D, tolerance))
                    return false;

            return System.Math.Abs(closed2D_1.GetArea() - closed2D_2.GetArea()) <= tolerance;
        }
    }
}