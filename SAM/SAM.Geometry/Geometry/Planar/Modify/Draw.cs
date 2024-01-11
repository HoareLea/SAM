using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Modify
    {
        public static bool Draw_Segment2Ds(this Graphics graphics, Color color, double thickness, IEnumerable<Point2D> point2Ds, bool close = false)
        {
            if (graphics == null || point2Ds == null)
                return false;

            if (point2Ds.Count() < 2)
                return false;

            if (close && point2Ds.Count() < 3)
                return false;

            Pen pen = new Pen(color, System.Convert.ToSingle(thickness));

            List<Segment2D> segment2Ds = Create.Segment2Ds(point2Ds, close);
            foreach (Segment2D segment2D in segment2Ds)
                Draw(graphics, pen, segment2D);

            return true;
        }

        public static bool Draw(this Graphics graphics, Color color, double thickness, Segment2D segment2D)
        {
            return Draw(graphics, new Pen(color, System.Convert.ToSingle(thickness)), segment2D);
        }

        public static bool Draw(this Graphics graphics, Pen pen, Segment2D segment2D)
        {
            if (graphics == null || segment2D == null)
                return false;

            Point2D point2D_1 = segment2D[0];
            if (!Query.IsValid(point2D_1))
                return false;

            Point2D point2D_2 = segment2D[1];
            if (!Query.IsValid(point2D_2))
                return false;

            if (point2D_1.Distance(point2D_2) < Core.Tolerance.MacroDistance)
                return false;

            graphics.DrawLine(pen, System.Convert.ToSingle(segment2D[0].X), System.Convert.ToSingle(segment2D[0].Y), System.Convert.ToSingle(segment2D[1].X), System.Convert.ToSingle(segment2D[1].Y));
            return true;
        }

        public static bool Draw(this Graphics graphics, Pen pen, IEnumerable<Segment2D> segment2Ds)
        {
            if (graphics == null || segment2Ds == null)
                return false;

            foreach (Segment2D segment2D in segment2Ds)
                Draw(graphics, pen, segment2D);

            return true;
        }

        public static bool Draw(this Graphics graphics, Color color, double thickness, ISegmentable2D segmentable2D)
        {
            return Draw(graphics, new Pen(color, System.Convert.ToSingle(thickness)), segmentable2D);
        }

        public static bool Draw(this Graphics graphics, Pen pen, ISegmentable2D segmentable2D)
        {
            if (graphics == null || pen == null || segmentable2D == null)
                return false;

            return Draw(graphics, pen, segmentable2D.GetSegments());
        }

        public static bool Draw(this Graphics graphics, Pen pen, Polyline2D polyline2D)
        {
            return Draw(graphics, pen, polyline2D as ISegmentable2D);
        }

        public static bool Draw(this Graphics graphics, Pen pen, Polygon2D polygon2D)
        {
            return Draw(graphics, pen, polygon2D as ISegmentable2D);
        }
    }
}