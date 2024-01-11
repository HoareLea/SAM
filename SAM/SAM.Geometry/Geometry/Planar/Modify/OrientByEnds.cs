using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        /// <summary>
        /// Orients two curves by its ends (end point of first curve become the closest to start
        /// point of second curve)
        /// </summary>
        /// <returns>Returns true if at least one curve orientation changed</returns>
        /// <param name="curve2D_1">First curve to be orieted</param>
        /// <param name="curve2D_2">Second curve to be orieted</param>
        /// <param name="Orient_1">True if update orientation of first curve</param>
        /// <param name="Orient_2">True if update orientation of second curve</param>
        public static bool OrientByEnds(ICurve2D curve2D_1, ICurve2D curve2D_2, bool Orient_1 = true, bool Orient_2 = true)
        {
            if (curve2D_1 == null || curve2D_2 == null)
                return false;

            if (!Orient_1 && !Orient_2)
                return false;

            Point2D point2D_Previous_Start = curve2D_1.GetStart();
            Point2D point2D_Previous_End = curve2D_1.GetEnd();
            Point2D point2D_Start = curve2D_2.GetStart();
            Point2D point2D_End = curve2D_2.GetEnd();

            List<double> distances = new List<double>();
            distances.Add(point2D_Previous_Start.Distance(point2D_End));
            distances.Add(point2D_Previous_Start.Distance(point2D_Start));
            distances.Add(point2D_Previous_End.Distance(point2D_End));
            distances.Add(point2D_Previous_End.Distance(point2D_Start));

            double distance = distances.Min();

            bool result = false;
            if (distance == distances[0])
            {
                if (Orient_1)
                {
                    curve2D_1.Reverse();
                    result = true;
                }

                if (Orient_2)
                {
                    curve2D_2.Reverse();
                    result = true;
                }
            }
            else if (distance == distances[1])
            {
                if (Orient_1)
                {
                    curve2D_1.Reverse();
                    result = true;
                }
            }
            else if (distance == distances[2])
            {
                if (Orient_2)
                {
                    curve2D_2.Reverse();
                    result = true;
                }
            }
            else //if (distance == distances[3])
            {
                //Do nothing
            }

            return result;
        }

        public static bool OrientByEnds(List<ICurve2D> curves2D)
        {
            if (curves2D == null)
                return false;

            if (curves2D.Count == 0)
                return false;

            if (curves2D.Count == 1)
                return false;

            List<ICurve2D> curves2D_Temp = curves2D.ConvertAll(x => (ICurve2D)x.Clone());

            int count = curves2D_Temp.Count();

            curves2D.Clear();
            for (int i = 0; i < count - 1; i++)
            {
                int count_Result = curves2D.Count;

                ICurve2D curve2D = (ICurve2D)curves2D_Temp[i].Clone();

                if (count_Result > 0)
                    OrientByEnds(curves2D[count_Result - 1], curve2D, count_Result == 1, true);

                curves2D.Add(curves2D_Temp[i]);
            }
            return true;
        }
    }
}