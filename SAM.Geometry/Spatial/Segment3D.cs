using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class Segment3D : ICurve3D
    {
        private Point3D[] points = new Point3D[2];

        public Segment3D(Point3D start, Point3D end)
        {
            points[0] = start;
            points[1] = end;
        }

        public Segment3D(Segment3D segment3D)
        {
            points[0] = new Point3D(segment3D.points[0]);
            points[1] = new Point3D(segment3D.points[1]);
        }

        public Point3D this[int index]
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

        public Point3D Start
        {
            get
            {
                return points[0];
            }
            set
            {
                if (points == null)
                    points = new Point3D[2];

                points[0] = value;
            }
        }

        public Point3D End
        {
            get
            {
                return points[1];
            }
            set
            {
                if (points == null)
                    points = new Point3D[2];

                points[1] = value;
            }
        }
    }
}
