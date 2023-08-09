using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        /// <summary>
        /// Checks if point is inside or on closed shape in 2D (excluding holes)
        /// </summary>
        /// <param name="closed2D">CLosed2D Shape</param>
        /// <param name="point2D">Point2D</param>
        /// <param name="tolerance">Tolerance for distance comparison</param>
        /// <returns>True if poin2D is inside or on given closed shape</returns>
        public static bool InRange(this IClosed2D closed2D, Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            if (closed2D == null || point2D == null)
                return false;

            IClosed2D closed2D_Temp = closed2D;
            if (closed2D_Temp is Face2D)
                closed2D_Temp = ((Face2D)closed2D_Temp).ExternalEdge2D;

            return closed2D_Temp.On(point2D, tolerance) || closed2D_Temp.Inside(point2D, tolerance);
        }

        public static bool InRange(this IClosed2D closed2D, ISegmentable2D segmentable2D, double tolerance = Core.Tolerance.Distance)
        {
            if (closed2D == null || segmentable2D == null)
                return false;

            IClosed2D closed2D_Temp = closed2D;
            if (closed2D_Temp is Face2D)
                closed2D_Temp = ((Face2D)closed2D_Temp).ExternalEdge2D;

            List<Point2D> point2Ds = segmentable2D.GetPoints();
            if (point2Ds == null || point2Ds.Count == 0)
                return false;

            foreach(Point2D point2D in point2Ds)
            {
                if (closed2D_Temp.InRange(point2D, tolerance))
                    return true;
            }

            if (!(closed2D_Temp is ISegmentable2D))
            {
                if(closed2D_Temp is Circle2D)
                {
                    Circle2D circle2D = (Circle2D)closed2D_Temp;
                    return closed2D_Temp.Inside(circle2D.Center, tolerance) || point2Ds.Find(x => circle2D.Inside(x, tolerance)) != null;
                }
                else
                {
                    throw new System.NotImplementedException();
                }
            }


            if (Intersect(closed2D_Temp as ISegmentable2D, segmentable2D.GetSegments()))
                return true;

            return false;
        }
    }
}