using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Point2D BottomLeft(this ISegmentable2D segmentable2D)
        {
            return BottomLeft(segmentable2D?.GetSegments());
        }

        public static Point2D BottomLeft(this IEnumerable<Segment2D> segment2Ds)
        {
            if (segment2Ds == null)
                return null;

            double x_Min = double.MaxValue;
            double y_Min = double.MaxValue;
            foreach (Segment2D segment2D in segment2Ds)
            {
                Point2D point2D;
                double value;

                point2D = segment2D[0];

                value = point2D.X;
                if (value < x_Min)
                    x_Min = value;

                value = point2D.Y;
                if (value < y_Min)
                    y_Min = value;

                point2D = segment2D[1];

                value = point2D.X;
                if (value < x_Min)
                    x_Min = value;

                value = point2D.Y;
                if (value < y_Min)
                    y_Min = value;
            }

            if (x_Min == double.MaxValue || y_Min == double.MaxValue)
                return null;

            return new Point2D(x_Min, y_Min);
        }
    }
}