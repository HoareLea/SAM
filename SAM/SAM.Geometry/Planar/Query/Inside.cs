using System.Collections.Generic;
using System.Linq;


namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static bool Inside(this IEnumerable<Point2D> point2Ds, Point2D point2D)
        {
            if (point2Ds == null || point2D == null)
                return false;

            int aCount = point2Ds.Count();

            if (aCount < 3)
                return false;

            bool result = false;

            int j = aCount - 1;
            for (int i = 0; i < aCount; i++)
            {
                if (point2Ds.ElementAt(i).Y < point2D.Y && point2Ds.ElementAt(j).Y >= point2D.Y || point2Ds.ElementAt(j).Y < point2D.Y && point2Ds.ElementAt(i).Y >= point2D.Y)
                    if (point2Ds.ElementAt(i).X + (point2D.Y - point2Ds.ElementAt(i).Y) / (point2Ds.ElementAt(j).Y - point2Ds.ElementAt(i).Y) * (point2Ds.ElementAt(j).X - point2Ds.ElementAt(i).X) < point2D.X)
                        result = !result;
                j = i;
            }
            return result;
        }

        public static bool Inside(this Face2D face2D, Point2D point2D)
        {
            if (face2D == null || point2D == null)
                return false;

            if (!face2D.ExternalEdge2D.Inside(point2D))
                return false;

            List<IClosed2D> internalEdge2Ds = face2D.InternalEdge2Ds;
            if (internalEdge2Ds == null || internalEdge2Ds.Count == 0)
                return true;

            foreach(IClosed2D closed2D in internalEdge2Ds)
                if (closed2D.Inside(point2D))
                    return false;

            return true;
        }

        //public static bool Inside(this IEnumerable<Point2D> point2Ds, Point2D point2D)
        //{
        //    if (point2Ds == null || point2Ds.Count() < 3)
        //        return false;

        //    bool result = false;
        //    Point2D point2D_1 = point2Ds.Last();
        //    foreach (Point2D point2D_2 in point2Ds)
        //    {
        //        if ((point2D_2.X == point2D.X) && (point2D_2.Y == point2D.Y))
        //            return true;

        //        if ((point2D_2.Y == point2D_1.Y) && (point2D.Y == point2D_1.Y) && (point2D_1.X <= point2D.X) && (point2D.X <= point2D_2.X))
        //            return true;

        //        if ((point2D_2.Y < point2D.Y) && (point2D_1.Y >= point2D.Y) || (point2D_1.Y < point2D.Y) && (point2D_2.Y >= point2D.Y))
        //        {
        //            if (point2D_2.X + (point2D.Y - point2D_2.Y) / (point2D_1.Y - point2D_2.Y) * (point2D_1.X - point2D_2.X) <= point2D.X)
        //                result = !result;
        //        }
        //        point2D_1 = point2D_2;
        //    }
        //    return result;
        //}

        //public static bool Inside(this IEnumerable<Point2D> point2Ds, Point2D point2D, double tolerance = Core.Tolerance.MicroDistance)
        //{
        //    if (point2Ds == null || point2D == null || point2Ds.Count() < 3)
        //        return false;

        //    Polygon polygon = new Polygon(point2Ds.ToNTS_LinearRing(tolerance));
        //    return polygon.Distance(point2D.ToNTS_Point(tolerance)) <= tolerance;
        //}
    }
}