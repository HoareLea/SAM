using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        /// <summary>
        /// Creates new polygon2D which is inside given polygon but his edges are vertical or horizontal
        /// </summary>
        /// <param name="polygon2D">Polygon2D</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>Verticaly Aligned Ortogonal Polygon2D</returns>
        public static Polygon2D Straight(this Polygon2D polygon2D, double tolerance = Core.Tolerance.Distance)
        {
            List<Segment2D> segment2Ds = polygon2D?.GetSegments();
            if(segment2Ds == null || segment2Ds.Count == 0)
            {
                return null;
            }

            Vector2D vector2D_X = Vector2D.WorldX;
            Vector2D vector2D_Y = Vector2D.WorldY;

            Vector2D vector2D_X_Negated = vector2D_X.GetNegated();
            Vector2D vector2D_Y_Negated = vector2D_Y.GetNegated();

            List<Polygon2D> polygon2Ds = new List<Polygon2D>();
            foreach (Segment2D segment2D in segment2Ds)
            {
                Vector2D direction = segment2D.Direction;

                if (direction.AlmostEqual(vector2D_X, tolerance) || direction.AlmostEqual(vector2D_X_Negated, tolerance))
                    continue;

                if (direction.AlmostEqual(vector2D_Y, tolerance) || direction.AlmostEqual(vector2D_Y_Negated, tolerance))
                    continue;

                BoundingBox2D boundingBox2D = segment2D.GetBoundingBox();
                if (boundingBox2D == null|| boundingBox2D.Width <= tolerance || boundingBox2D.Height <= tolerance)
                    continue;

                polygon2Ds.Add(boundingBox2D);
            }


            polygon2Ds = polygon2D.Difference(polygon2Ds, tolerance);
            if (polygon2Ds == null || polygon2Ds.Count == 0)
                return null;

            if (polygon2Ds.Count == 1)
                return polygon2Ds[0];

            polygon2Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

            return polygon2Ds[0];


            //List<Point2D> point2Ds = polygon2D?.GetPoints();
            //if(point2Ds == null || point2Ds.Count == 0)
            //{
            //    return null;
            //}

            //List<Polygon2D> polygon2Ds = new List<Polygon2D>();
            //for(int i=0; i < point2Ds.Count - 2; i++)
            //{
            //    for (int j = i + 1; j < point2Ds.Count - 1; j++)
            //    {
            //        BoundingBox2D boundingBox2D = new BoundingBox2D(point2Ds[i], point2Ds[j]);
            //        if(boundingBox2D.GetArea() <= tolerance)
            //        {
            //            continue;
            //        }

            //        if (boundingBox2D.GetPoints().TrueForAll(x => polygon2D.Inside(x, tolerance) || polygon2D.On(x, tolerance)))
            //            polygon2Ds.Add(boundingBox2D);
            //    }
            //}

            //polygon2Ds = polygon2Ds.Union(tolerance);
            //if (polygon2Ds == null || polygon2Ds.Count == 0)
            //    return null;

            //if (polygon2Ds.Count == 1)
            //    return polygon2Ds[0];

            //polygon2Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

            //return polygon2Ds[0];
        }
    }
}