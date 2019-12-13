using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public class Polygon3D : IClosedPlanar3D
    {
        //TODO: Convert to Plane and Point2Ds
        private List<Point3D> points;

        public Polygon3D(IEnumerable<Point3D> points)
        {
            if (points != null)
                this.points = new List<Point3D>(points);
        }

        public List<Point3D> Points
        {
            get
            {
                return new List<Point3D>(points);
            }
        }

        public Segment3D[] GetSegments()
        {
            int count = points.Count;

            Segment3D[] result = new Segment3D[count];
            for (int i = 0; i < count - 1; i++)
                result[i] = new Segment3D(points[i], points[i + 1]);

            result[count - 1] = new Segment3D(new Point3D(points[count - 1]), new Point3D(points[0]));

            return result;
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
            
            return new Plane(points[0], points[1], points[2]);
        }
    }
}
