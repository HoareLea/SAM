using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static List<Segment2D> Segment2Ds(this BoundingBox2D boundingBox2D, Alignment alignment, double offset, Corner corner = Corner.TopLeft, bool includeEdge = true)
        {
            double offset_Horizontal = 0;
            double offset_Vertical = 0;
            switch (alignment)
            {
                case Alignment.Horizontal:
                    offset_Horizontal = offset;
                    break;

                case Alignment.Vertical:
                    offset_Vertical = offset;
                    break;

                default:
                    return null;
            }

            return Segment2Ds(boundingBox2D, offset_Horizontal, offset_Vertical, corner, includeEdge);
        }

        public static List<Segment2D> Segment2Ds(this BoundingBox2D boundingBox2D, double? offset_Horizontal, double? offset_Vertical, Point2D point2D, bool includeEdge = true)
        {
            return Segment2Ds(boundingBox2D, offset_Horizontal, offset_Vertical, boundingBox2D.GetCorner(point2D), true);
        }

        public static List<Segment2D> Segment2Ds(this BoundingBox2D boundingBox2D, Alignment alignment, double offset, Point2D point2D, bool includeEdge = true)
        {
            return Segment2Ds(boundingBox2D, alignment, offset, boundingBox2D.GetCorner(point2D), true);
        }

        public static List<Segment2D> Segment2Ds(this BoundingBox2D boundingBox2D, double? offset_Horizontal, double? offset_Vertical, Corner corner = Corner.TopLeft, bool includeEdge = true)
        {
            if (boundingBox2D == null || corner == Corner.Undefined)
                return null;

            List<Segment2D> result = null;

            if (offset_Horizontal != null && offset_Horizontal.HasValue && offset_Horizontal.Value != 0)
            {
                result = new List<Segment2D>();

                double x_Start = boundingBox2D.Min.X;
                double x_End = boundingBox2D.Max.X;

                double y_Start = double.NaN;
                double factor_direction = 1;
                switch (corner)
                {
                    case Corner.TopLeft:
                    case Corner.TopRight:
                        y_Start = boundingBox2D.Max.Y;
                        factor_direction = -1;
                        break;

                    case Corner.BottomLeft:
                    case Corner.BottomRight:
                        y_Start = boundingBox2D.Min.Y;
                        factor_direction = 1;
                        break;
                }

                double offset = offset_Horizontal.Value;
                int count = System.Convert.ToInt32(boundingBox2D.Height / offset);
                if (!includeEdge)
                    count--;
                else
                    count++;

                offset = offset * factor_direction;
                for (int i = 0; i < count; i++)
                {
                    int index = i;
                    if (!includeEdge)
                        index++;

                    double y = y_Start + (offset * index);
                    result.Add(new Segment2D(new Point2D(x_Start, y), new Point2D(x_End, y)));
                }
            }

            if (offset_Vertical != null && offset_Vertical.HasValue && offset_Vertical.Value != 0)
            {
                if (result == null)
                    result = new List<Segment2D>();

                double y_Start = boundingBox2D.Min.Y;
                double y_End = boundingBox2D.Max.Y;

                double x_Start = double.NaN;
                double factor_direction = 1;
                switch (corner)
                {
                    case Corner.TopLeft:
                    case Corner.BottomLeft:
                        x_Start = boundingBox2D.Min.X;
                        factor_direction = 1;
                        break;

                    case Corner.TopRight:
                    case Corner.BottomRight:
                        x_Start = boundingBox2D.Max.X;
                        factor_direction = -1;
                        break;
                }

                double offset = offset_Vertical.Value;
                int count = System.Convert.ToInt32(boundingBox2D.Width / offset);

                if (!includeEdge)
                    count--;
                else
                    count++;

                offset = offset * factor_direction;
                for (int i = 0; i < count; i++)
                {
                    int index = i;
                    if (!includeEdge)
                        index++;

                    double x = x_Start + (offset * index);
                    result.Add(new Segment2D(new Point2D(x, y_Start), new Point2D(x, y_End)));
                }
            }

            return result;
        }

        public static List<Segment2D> Segment2Ds(IEnumerable<Point2D> point2Ds, bool close = false)
        {
            if (point2Ds == null)
                return null;

            List<Segment2D> result = new List<Segment2D>();
            if (point2Ds.Count() < 2)
                return result;

            int aCount = point2Ds.Count();

            for (int i = 0; i < aCount - 1; i++)
                result.Add(new Segment2D(point2Ds.ElementAt(i), point2Ds.ElementAt(i + 1)));

            if (close)
                result.Add(new Segment2D(point2Ds.Last(), point2Ds.First()));

            return result;
        }
    }
}