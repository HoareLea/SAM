﻿using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Orientation Orientation(this Point2D point2D_1, Point2D point2D_2, Point2D point2D_3)
        {
            double determinant = Determinant(point2D_1, point2D_2, point2D_3);

            if (determinant == 0)
                return Geometry.Orientation.Collinear;

            if (determinant > 0)
                return Geometry.Orientation.Clockwise;
            else
                return Geometry.Orientation.CounterClockwise;
        }

        public static Orientation Orientation(this Vector2D vector2D_1, Vector2D vector2D_2)
        {
            double determinant = Determinant(vector2D_1, vector2D_2);

            if (determinant == 0)
                return Geometry.Orientation.Collinear;

            if (determinant > 0)
                return Geometry.Orientation.Clockwise;
            else
                return Geometry.Orientation.CounterClockwise;
        }

        public static Orientation Orientation(this IEnumerable<Point2D> point2Ds, bool convexHull = true)
        {
            if (point2Ds == null || point2Ds.Count() == 0)
                return Geometry.Orientation.Undefined;

            List<Point2D> point2Ds_Temp = new List<Point2D>(point2Ds);
            if (point2Ds_Temp == null || point2Ds_Temp.Count < 3)
                return Geometry.Orientation.Undefined;

            if (convexHull)
            {
                List<Point2D> point2Ds_ConvexHull = ConvexHull(point2Ds);

                //ConvexHull may have different orientation so needs to remove unnecessary points from existing point2Ds
                if (point2Ds_ConvexHull != null && point2Ds_ConvexHull.Count > 0)
                {
                    List<Point2D> point2Ds_ConvexHull_Temp = new List<Point2D>(point2Ds);
                    point2Ds_ConvexHull_Temp.RemoveAll(x => point2Ds_ConvexHull.Contains(x));
                    point2Ds_Temp.RemoveAll(x => point2Ds_ConvexHull_Temp.Contains(x));
                }
            }

            point2Ds_Temp.Add(point2Ds_Temp[0]);
            point2Ds_Temp.Add(point2Ds_Temp[1]);

            for (int i = 0; i < point2Ds_Temp.Count - 2; i++)
            {
                Orientation orientation = Orientation(point2Ds_Temp[i], point2Ds_Temp[i + 1], point2Ds_Temp[i + 2]);
                if (orientation != Geometry.Orientation.Collinear && orientation != Geometry.Orientation.Undefined)
                    return orientation;
            }

            return Geometry.Orientation.Undefined;
        }

        public static Orientation Orientation(this IClosed2D closed2D)
        {
            if(closed2D == null)
            {
                return Geometry.Orientation.Undefined;
            }

            if(closed2D is Face2D)
            {
                return Orientation(((Face2D)closed2D).ExternalEdge2D);
            }
            
            ISegmentable2D segmentable2D = closed2D as ISegmentable2D;
            if(segmentable2D == null)
            {
                throw new System.NotImplementedException();
            }

            return Orientation(segmentable2D.GetPoints(), !(segmentable2D is Triangle2D || segmentable2D is Rectangle2D));
        }
    }
}