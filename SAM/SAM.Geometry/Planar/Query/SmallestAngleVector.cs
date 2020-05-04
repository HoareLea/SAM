using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Vector2D SmallestAngleVector(this IEnumerable<Vector2D> vector2Ds, Vector2D vector2D)
        {
            if (vector2Ds == null || vector2D == null)
                return null;

            Vector2D result = null;
            double angle_Min = double.MaxValue;
            foreach (Vector2D vector2D_Temp in vector2Ds)
            {
                double angle = vector2D_Temp.SmallestAngle(vector2D);
                if (angle < angle_Min)
                {
                    result = vector2D_Temp;
                    angle_Min = angle;
                }
            }

            return result;
        }
    }
}