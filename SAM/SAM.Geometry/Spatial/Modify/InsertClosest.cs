using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Modify
    {
        /// <summary>
        /// Inserts new point on one of the edges (closest to given point3D)
        /// </summary>
        /// <param name="point3Ds">Point3D list will be modified</param>
        /// <param name="point3D">Point3D will be inserted</param>
        /// <param name="close">Is Closed</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>Inserted Point2D</returns>
        public static Point3D InsertClosest(this List<Point3D> point3Ds, Point3D point3D, bool close = false, double tolerance = Core.Tolerance.Distance)
        {
            List<Segment3D> segment3Ds = Create.Segment3Ds(point3Ds, close);
            if (segment3Ds == null || segment3Ds.Count == 0)
                return null;

            int index = -1;
            Point3D point3D_Closest = null;
            double distance_Min = double.MaxValue;

            for (int i = 0; i < segment3Ds.Count; i++)
            {
                Segment3D segment3D = segment3Ds[i];

                Point3D point3D_Closest_Temp = segment3D.Closest(point3D);
                double distance = point3D.Distance(point3D_Closest_Temp);
                if (distance < distance_Min)
                {
                    distance_Min = distance;
                    point3D_Closest = point3D_Closest_Temp;
                    index = i;
                }
            }

            if (index == -1)
                return null;

            Segment3D segment3D_Temp = segment3Ds[index];
            if (point3D_Closest.AlmostEquals(segment3D_Temp[0], tolerance))
                return segment3D_Temp[0];

            if(point3D_Closest.AlmostEquals(segment3D_Temp[1], tolerance))
                return segment3D_Temp[1];

            segment3Ds[index] = new Segment3D(segment3D_Temp[0], point3D_Closest);
            segment3Ds.Insert(index + 1, new Segment3D(point3D_Closest, segment3D_Temp[1]));

            point3Ds.Clear();
            point3Ds.AddRange(segment3Ds.ConvertAll(x => x.GetStart()));
            if (!close)
                point3Ds.Add(segment3Ds.Last()[1]);

            return point3D_Closest;
        }
    }
}