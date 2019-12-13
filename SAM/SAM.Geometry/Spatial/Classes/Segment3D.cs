using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class Segment3D : ICurve3D
    {
        private Point3D origin;
        private Vector3D vector;

        public Segment3D(Point3D start, Point3D end)
        {
            origin = start;
            vector = new Vector3D(start, end);
        }

        public Segment3D(Segment3D segment3D)
        {
            origin = new Point3D(segment3D.origin);
            vector = new Vector3D(segment3D.vector);
        }

        public Point3D this[int index]
        {
            get
            {
                if (index == 0)
                    return origin;
                if (index == 1)
                    return origin.GetMoved(vector);

                return null;
            }
            set
            {
                if (index == 0)
                    origin = value;
                else if (index == 1)
                    vector = new Vector3D(origin, value);
            }
        }

        public Point3D Start
        {
            get
            {
                return new Point3D(origin);
            }
            set
            {
                origin = value;
            }
        }

        public Point3D End
        {
            get
            {
                return origin.GetMoved(vector);
            }
            set
            {
                vector = new Vector3D(origin, value);
            }
        }

        public Vector3D Direction
        {
            get
            {
                return vector.Unit;
            }
        }

        public List<Point3D> GetPoints()
        {
            return new List<Point3D>() { origin, End };
        }

        public IGeometry Clone()
        {
            return new Segment3D(this);
        }
    }
}
