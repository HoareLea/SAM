using SAM.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static Segment2D Segment2D(this Line2D line2D, Rectangle2D rectangle2D)
        {
            if (line2D == null || rectangle2D == null)
            {
                return null;
            }

            List<Point2D> points2D = new List<Point2D>();

            List<Point2D> cornerPoints = rectangle2D.GetPoints();
            for (int i = 0; i < cornerPoints.Count; i++)
            {
                Segment2D rectangleSide = null;
                if (i == 0)
                {
                    rectangleSide = new Segment2D(cornerPoints[cornerPoints.Count - 1], cornerPoints[i]);
                }
                else
                {
                    rectangleSide = new Segment2D(cornerPoints[i - 1], cornerPoints[i]);
                }
                Point2D point = line2D.Intersection(rectangleSide);
                if (point != null)
                {
                    points2D.Add(point);
                }
            }

            if (points2D.Count != 2)
            {
                return null;
            }

            return new Segment2D(points2D[0], points2D[1]);
        }

    }
}