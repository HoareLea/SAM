using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public class Polygon3D : IClosedPlanar3D, ISegmentable3D
    {
        //TODO: Convert to Plane and Point2Ds
        private List<Point3D> points;

        public Polygon3D(IEnumerable<Point3D> points)
        {
            if (points != null)
                this.points = new List<Point3D>(points);
        }

        public Polygon3D(Polygon3D polygon3D)
        {
            points = Point3D.Clone(polygon3D.points);
        }

        public List<Point3D> GetPoints()
        {
            return points.ConvertAll(x => new Point3D(x));
        }

        public Vector3D GetNormal()
        {
            if (points.Count < 3)
                return null;

            return Point3D.GetNormal(points[0], points[1], points[2]);
        }

        public Plane GetPlane()
        {
            if (points.Count < 3)
                return null;
            
            return new Plane(new Point3D(points[0]), new Point3D(points[1]), new Point3D(points[2]));
        }

        public IGeometry Clone()
        {
            return new Polygon3D(this);
        }

        public List<Segment3D> GetSegments()
        {
            int count = points.Count;

            Segment3D[] result = new Segment3D[count];
            for (int i = 0; i < count - 1; i++)
                result[i] = new Segment3D(points[i], points[i + 1]);

            result[count - 1] = new Segment3D(new Point3D(points[count - 1]), new Point3D(points[0]));

            return result.ToList();
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox3D(points);
        }

        public Face ToFace()
        {
            return new Face(this);
        }

        public static Polygon3D Snap(IEnumerable<Point3D> point3Ds, Polygon3D polygon3D, double maxDistance = double.NaN)
        {
            List<Point3D> point3Ds_Temp = new List<Point3D>();

            foreach (Point3D point3D in polygon3D.GetPoints())
                point3Ds_Temp.Add(new Point3D(Point3D.Snap(point3Ds, point3D, maxDistance)));

            return new Polygon3D(point3Ds_Temp);
        }
    }
}
