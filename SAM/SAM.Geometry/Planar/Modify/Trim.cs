using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static List<Segment2D> Trim(this IEnumerable<Segment2D> segment2Ds, double distance, bool trim_Start = true, bool trim_End = true)
        {
            if (segment2Ds == null)
                return null;

            List<Segment2D> result = new List<Segment2D>();
            foreach(Segment2D segment2D in segment2Ds)
            {
                Segment2D segment2D_Temp = Trim(segment2D, distance, trim_Start, trim_End);
                if (segment2D_Temp != null)
                    result.Add(segment2D_Temp);
            }

            return result;
        }

        public static Segment2D Trim(Segment2D segment2D, double distance, bool trim_Start = true, bool trim_End = true)
        {
            if (segment2D == null)
                return null;

            if (!trim_Start && !trim_End)
                return new Segment2D(segment2D);

            if (distance == 0)
                return new Segment2D(segment2D);

            Vector2D vector2D = segment2D.Direction * distance;

            Point2D point2D_Start = segment2D.Start;
            if(trim_Start)
                point2D_Start.Move(vector2D);

            vector2D.Negate();

            Point2D point2D_End = segment2D.End;
            if (trim_End)
                point2D_End.Move(vector2D);

            Segment2D result = new Segment2D(point2D_Start, point2D_End);
            if (!result.Direction.AlmostEqual(segment2D.Direction))
                return null;

            return result;
        }

    }
}
