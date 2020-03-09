using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static List<Segment2D> Segment2Ds(this BoundingBox2D boundingBox2D, double? offset_Height, double? offset_Width, Corner corner = Corner.TopLeft)
        {
            if (boundingBox2D == null)
                return null;

            List<Segment2D> result = null; 

            if (offset_Height != null && offset_Height.HasValue && offset_Height.Value != 0)
            {
                result = new List<Segment2D>();

                double x_Start = boundingBox2D.Min.X;
                double x_End = boundingBox2D.Max.X;

                double y_Start = double.NaN;
                double factor_direction = 1;
                switch(corner)
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

                double offset = offset_Height.Value;
                int count = System.Convert.ToInt32(boundingBox2D.Height / offset);
                offset = offset * factor_direction;
                for (int i= 0; i < count; i++)
                {
                    double y = y_Start + offset * (i + 1);
                    result.Add(new Segment2D(new Point2D(x_Start, y), new Point2D(x_End, y)));
                }
            }

            if(offset_Width != null && offset_Width.HasValue && offset_Width.Value != 0)
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
                        x_Start = boundingBox2D.Min.Y;
                        factor_direction = 1;
                        break;

                    case Corner.TopRight:
                    case Corner.BottomRight:
                        x_Start = boundingBox2D.Max.Y;
                        factor_direction = -1;
                        break;
                }

                double offset = offset_Width.Value * factor_direction;
                int count = System.Convert.ToInt32(boundingBox2D.Width / offset);
                offset = offset * factor_direction;
                for (int i = 0; i < count; i++)
                {
                    double x = x_Start + offset * (i + 1);
                    result.Add(new Segment2D(new Point2D(x, y_Start), new Point2D(x, y_End)));
                }
            }

            return result;
        }
    }
}
