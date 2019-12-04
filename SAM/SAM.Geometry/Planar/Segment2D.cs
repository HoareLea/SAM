using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Planar
{
    public class Segment2D : ICurve2D
    {
        private Point2D[] points = new Point2D[2];

        public Segment2D(Point2D start, Point2D end)
        {
            points[0] = start;
            points[1] = end;
        }

        public Segment2D(Segment2D segment2D)
        {
            points[0] = new Point2D(segment2D.points[0]);
            points[1] = new Point2D(segment2D.points[1]);
        }

        public Point2D this[int index]
        {
            get
            {
                return points[index];
            }
            set
            {
                points[index] = value;
            }
        }

        public Point2D Start
        {
            get
            {
                return points[0];
            }
            set
            {
                if (points == null)
                    points = new Point2D[2];

                points[0] = value;
            }
        }

        public Point2D End
        {
            get
            {
                return points[1];
            }
            set
            {
                if (points == null)
                    points = new Point2D[2];

                points[1] = value;
            }
        }
    }
}
