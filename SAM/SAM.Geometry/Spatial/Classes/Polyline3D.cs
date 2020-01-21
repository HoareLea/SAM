using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class Polyline3D : IBoundable3D, ISegmentable3D, ICurve3D
    {
        private List<Point3D> points;

        public Polyline3D(IEnumerable<Point3D> point3Ds)
        {
            this.points = Point3D.Clone(point3Ds);
        }

        public Polyline3D(Polyline3D polyline3D)
        {
            this.points = Point3D.Clone(polyline3D.points);
        }

        public List<Point3D> Points
        {
            get
            {
                return new List<Point3D>(points);
            }
        }

        public List<Segment3D> GetSegments()
        {
            return Point3D.GetSegments(points, false);
        }

        public List<ICurve3D> GetCurves()
        {
            return GetSegments().ConvertAll(x => (ICurve3D)x);
        }

        public bool IsClosed()
        {
            return points.First().Equals(points.Last());
        }

        public bool IsClosed(double tolerance)
        {
            return points.First().Distance(points.Last()) < tolerance;
        }

        public void Close()
        {
            if (IsClosed())
                return;

            points.Add(new Point3D(points.First()));
        }

        public void Open()
        {
            if (!IsClosed())
                return;

            points.RemoveAt(points.Count - 1);
        }

        public Polygon3D ToPolygon3D()
        {
            
            
            return new Polygon3D(points);
        }

        public IGeometry Clone()
        {
            return new Polyline3D(this);
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox3D(points);
        }

        public Point3D GetStart()
        {
            return new Point3D(points.First());
        }

        public Point3D GetEnd()
        {
            return new Point3D(points.Last());
        }

        public void Reverse()
        {
            points.Reverse();
        }

        public IGeometry3D GetMoved(Vector3D vector3D)
        {
            return new Polyline3D(points.ConvertAll(x => (Point3D)x.GetMoved(vector3D)));
        }
    }
}
