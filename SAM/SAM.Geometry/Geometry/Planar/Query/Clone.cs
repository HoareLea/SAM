using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static List<Point2D> Clone(this IEnumerable<Point2D> point2Ds)
        {
            if(point2Ds == null)
            {
                return null;
            }

            List<Point2D> result = new List<Point2D>();
            foreach (Point2D point2D in point2Ds)
            {
                result.Add(new Point2D(point2D));
            }

            return result;
        }

        public static List<Face2D> Clone(this IEnumerable<Face2D> face2Ds)
        {
            List<Face2D> result = new List<Face2D>();
            foreach (Face2D face2D in face2Ds)
                result.Add(new Face2D(face2D));

            return result;
        }

        public static List<Polygon2D> Clone(this IEnumerable<Polygon2D> polygon2Ds)
        {
            List<Polygon2D> result = new List<Polygon2D>();
            foreach (Polygon2D polygon2D in polygon2Ds)
                result.Add(new Polygon2D(polygon2D));

            return result;
        }
    }
}