using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<double> Determinants<T>(this T segmentable2D) where T: IClosed2D, ISegmentable2D
        {
            List<Point2D> point2Ds = segmentable2D.GetPoints();
            if (point2Ds == null || point2Ds.Count == 0)
                return null;

            int index = point2Ds.Count - 1;

            point2Ds.Add(point2Ds[0]);
            point2Ds.Insert(0, point2Ds[index]);
            

            List<double> result = new List<double>();
            for (int i = 1; i < point2Ds.Count - 1; i++)
                result.Add(Determinant(point2Ds[i - 1], point2Ds[i], point2Ds[i + 1]));

            return result;
        }
    }
}
