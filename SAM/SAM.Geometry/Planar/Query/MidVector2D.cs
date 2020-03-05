using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Vector2D MidVector2D(this Segment2D segment2D_1, Segment2D segment2D_2)
        {
            if (segment2D_1 == null || segment2D_2 == null)
                return null;

            Point2D point2D_Closets_1 = null;
            Point2D point2D_Closets_2 = null;

            Point2D point2D_Intersection = segment2D_1.Intersection(segment2D_2, out point2D_Closets_1, out point2D_Closets_2);
            if (point2D_Intersection == null)
                return null;

            if (point2D_Closets_1 != null && point2D_Closets_2 != null)
                return MidVector2D(new Vector2D(point2D_Closets_1, point2D_Intersection), new Vector2D(point2D_Closets_2, point2D_Intersection));

            Segment2D segment2D_1_Temp = new Segment2D(segment2D_1);
            Segment2D segment2D_2_Temp = new Segment2D(segment2D_2);
            Modify.OrientByEnds(segment2D_1_Temp, segment2D_2_Temp, true, true);

            return MidVector2D(new Vector2D(segment2D_1_Temp.Start, point2D_Intersection), new Vector2D(segment2D_2_Temp.End, point2D_Intersection));
        }

        public static Vector2D MidVector2D(this Vector2D vector2D_1, Vector2D vector2D_2)
        {
            if (vector2D_1 == null || vector2D_2 == null)
                return null;

            return (vector2D_1.Unit + vector2D_2.Unit).Unit;
        }

    }
}
