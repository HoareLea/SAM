using SAM.Core;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Polyline2D> Cut(this Polyline2D polyline2D, Point2D point2D_1, Point2D point2D_2)
        {
            if (polyline2D == null || point2D_1 == null || point2D_2 == null)
                return null;

            Point2D point2D_1_Closest = polyline2D.Closest(point2D_1, true);
            Point2D point2D_2_Closest = polyline2D.Closest(point2D_2, true);
            if (point2D_1_Closest.Equals(point2D_2_Closest))
            {
                Polyline2D polyline2D_Temp = new Polyline2D(polyline2D);
                polyline2D_Temp.InsertClosest(point2D_1_Closest);
                int index = polyline2D_Temp.IndexOfClosestPoint2D(point2D_1);
                //polyline2D_Temp.Reorder(index); //DODO: Check if this line is necessary

                return new List<Polyline2D>() { polyline2D_Temp };
            }

            double parameter_1 = polyline2D.GetParameter(point2D_1_Closest);
            double parameter_2 = polyline2D.GetParameter(point2D_2_Closest);

            if (parameter_1 > parameter_2)
            {
                Point2D point2D = point2D_1_Closest;
                point2D_1_Closest = point2D_2_Closest;
                point2D_2_Closest = point2D;

                double parameter = parameter_1;
                parameter_1 = parameter_2;
                parameter_2 = parameter;
            }

            List<Polyline2D> result = new List<Polyline2D>();

            if (parameter_1 == 0)
            {
                if (1 - parameter_2 < parameter_2)
                {
                    result.Add(polyline2D.Trim(parameter_2) as Polyline2D);
                    return result;
                }

                Polyline2D polyline2D_Temp = new Polyline2D(polyline2D);
                polyline2D_Temp.Reverse();
                polyline2D_Temp = polyline2D_Temp.Trim(1 - parameter_2) as Polyline2D;
                polyline2D_Temp.Reverse();

                result.Add(polyline2D_Temp);
                return result;
            }

            result.Add(polyline2D.Trim(parameter_1) as Polyline2D);

            Polyline2D polyline2D_Reversed = new Polyline2D(polyline2D);
            polyline2D_Reversed.Reverse();

            if (parameter_2 != 1)
                result.Add(polyline2D_Reversed.Trim(1 - parameter_2) as Polyline2D);

            return result;
        }
    }
}