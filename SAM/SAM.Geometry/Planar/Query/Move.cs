using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Point2D Move(this Point2D point, Vector2D vector)
        {
            Point2D point_Temp = new Point2D(point);
            point_Temp.Move(vector);
            return point_Temp;
        }
    }
}