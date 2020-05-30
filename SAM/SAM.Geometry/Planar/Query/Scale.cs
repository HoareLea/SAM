using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static ISAMGeometry2D Scale(this ISAMGeometry2D sAMGeometry2D, double factor)
        {
            return Scale(sAMGeometry2D as dynamic, factor);
        }
        
        public static BoundingBox2D Scale(this BoundingBox2D boudingBox2D, double factor)
        {
            if (boudingBox2D == null)
                return null;

            return new BoundingBox2D(boudingBox2D.Min.GetScaled(factor), boudingBox2D.Max.GetScaled(factor));
        }

        public static Segment2D Scale(this Segment2D segment2D, double factor)
        {
            if (segment2D == null)
                return null;

            return new Segment2D(segment2D[0].GetScaled(factor), segment2D[1].GetScaled(factor));
        }

        public static Polygon2D Scale(this Polygon2D polygon2D, double factor)
        {
            if (polygon2D == null)
                return null;

            List<Point2D> point2Ds = polygon2D.GetPoints();
            if (point2Ds == null)
                return null;

            Modify.Scale(point2Ds, factor);

            return new Polygon2D(point2Ds);
        }

        public static Polyline2D Scale(this Polyline2D polyline2D, double factor)
        {
            if (polyline2D == null)
                return null;

            List<Point2D> point2Ds = polyline2D.GetPoints();
            if (point2Ds == null)
                return null;

            Modify.Scale(point2Ds, factor);

            return new Polyline2D(point2Ds);
        }
    }
}