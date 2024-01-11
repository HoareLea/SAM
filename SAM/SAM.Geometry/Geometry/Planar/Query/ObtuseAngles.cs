using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<bool> ObtuseAngles<T>(this T segmentable2D) where T: IClosed2D, ISegmentable2D
        {
            List<double> determinats = Determinants(segmentable2D);
            if (determinats == null)
                return null;

            List<bool> result = new List<bool>();

            if (determinats.Count == 0)
                return result;

            List<Point2D> point2Ds = segmentable2D.GetPoints();
            if (point2Ds == null)
                return null;

            if (point2Ds.Count != determinats.Count)
                return null;

            Orientation orientation = Orientation(point2Ds, true);

            switch(orientation)
            {
                case Geometry.Orientation.Clockwise:
                    return determinats.ConvertAll(x => x < 0);
                case Geometry.Orientation.CounterClockwise:
                    return determinats.ConvertAll(x => x > 0);
            }

            return null;
        }
    }
}
