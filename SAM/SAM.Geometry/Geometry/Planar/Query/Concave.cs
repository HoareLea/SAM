using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static bool Concave(this IEnumerable<Point2D> point2Ds)
        {
            if (point2Ds == null || point2Ds.Count() < 3)
            {
                return false;
            }

            List<Point2D> point2Ds_Temp = new List<Point2D>(point2Ds);

            int index = point2Ds_Temp.Count - 1;

            point2Ds_Temp.Add(point2Ds_Temp[0]);
            point2Ds_Temp.Insert(0, point2Ds_Temp[index]);

            int sign = System.Math.Sign(Determinant(point2Ds_Temp[0], point2Ds_Temp[1], point2Ds_Temp[2]));
            for (int i = 2; i < point2Ds_Temp.Count - 1; i++)
            {
                int sign_Temp = System.Math.Sign(Determinant(point2Ds_Temp[i - 1], point2Ds_Temp[i], point2Ds_Temp[i + 1]));
                if(sign != sign_Temp)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Concave<T>(this T segmentable2D) where T: ISegmentable2D, IClosed2D
        {
            if(segmentable2D == null)
            {
                return false;
            }

            return Concave(segmentable2D.GetPoints());
        }

        public static bool Concave(this Face2D face2D, bool externalEdge = true, bool internalEdges = true)
        {
            if(face2D == null)
            {
                return false;
            }

            return !Convex(face2D, externalEdge, internalEdges);
        }
    }
}