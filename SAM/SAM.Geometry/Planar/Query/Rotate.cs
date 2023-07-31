using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        /// <summary>
        /// Rotate point2D by given angle [rad]
        /// </summary>
        /// <param name="point2D">Point2D to be rotated</param>
        /// <param name="point2D_Center">Center of rotation</param>
        /// <param name="angle">Angle [rad]</param>
        /// <returns>Point2D</returns>
        public static Point2D Rotate(this Point2D point2D, Point2D point2D_Center, double angle)
        {
            if(point2D == null || point2D_Center == null || double.IsNaN(angle))
            {
                return null;
            }

            double cosTheta = System.Math.Cos(angle);
            double sinTheta = System.Math.Sin(angle);
            return new Point2D((cosTheta * (point2D.X - point2D_Center.X) - sinTheta * (point2D.Y - point2D_Center.Y) + point2D_Center.X), (sinTheta * (point2D.X - point2D_Center.X) + cosTheta * (point2D.Y - point2D_Center.Y) + point2D_Center.Y));
        }

        public static Vector2D Rotate(this Vector2D vector2D, double angle)
        {
            double cosTheta = System.Math.Cos(angle);
            double sinTheta = System.Math.Sin(angle);
            return new Vector2D(cosTheta * vector2D.X - sinTheta * vector2D.Y, sinTheta * vector2D.X + cosTheta * vector2D.Y);
        }

        public static Rectangle2D Rotate(this Rectangle2D rectangle2D, Point2D point2D, double angle)
        {
            if(rectangle2D == null || point2D == null || double.IsNaN(angle))
            {
                return null;
            }

            Point2D origin = rectangle2D.Origin.Rotate(point2D, angle);
            Vector2D heightDirection = rectangle2D.HeightDirection.Rotate(angle);

            Rectangle2D result = new Rectangle2D(origin, rectangle2D.Width, rectangle2D.Height, heightDirection);

            return result;
        }

        public static Polygon2D Rotate(this Polygon2D polygon2D, Point2D point2D, double angle)
        {
            if (polygon2D == null || point2D == null || double.IsNaN(angle))
            {
                return null;
            }

            List<Point2D> point2Ds = Rotate(polygon2D.Points, point2D, angle);
            if(point2Ds == null || point2Ds.Count == 0)
            {
                return null;
            }


            return new Polygon2D(point2Ds);
        }

        public static List<Point2D> Rotate(this IEnumerable<Point2D> point2Ds, Point2D point2D, double angle)
        {
            if (point2Ds == null || point2D == null || double.IsNaN(angle))
            {
                return null;
            }

            List<Point2D> result = new List<Point2D> (point2Ds);
            for (int i = 0; i < result.Count; i++)
            {
                result[i] = result[i].Rotate(point2D, angle);
            }

            return result;
        }

        public static Triangle2D Rotate(this Triangle2D triangle2D, Point2D point2D, double angle)
        {
            if (triangle2D == null || point2D == null || double.IsNaN(angle))
            {
                return null;
            }

            List<Point2D> point2Ds = triangle2D.GetPoints();

            return new Triangle2D(
                point2Ds[0].Rotate(point2D, angle),
                point2Ds[1].Rotate(point2D, angle),
                point2Ds[2].Rotate(point2D, angle));
        }

        public static Mesh2D Rotate(this Mesh2D mesh2D, Point2D point2D, double angle)
        {
            if (mesh2D == null || point2D == null || double.IsNaN(angle))
            {
                return null;
            }

            List<Point2D> point2Ds = Rotate(mesh2D.GetPoints(), point2D, angle);

            return new Mesh2D(point2Ds, mesh2D.GetIndexes());
        }

        public static Segment2D Rotate(this Segment2D segment2D, Point2D point2D, double angle)
        {
            if (segment2D == null || point2D == null || double.IsNaN(angle))
            {
                return null;
            }

            return new Segment2D(segment2D.Start.Rotate(point2D, angle), segment2D.End.Rotate(point2D, angle));
        }
    }
}