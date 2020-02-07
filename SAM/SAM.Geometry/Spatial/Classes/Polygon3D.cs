using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public class Polygon3D : SAMGeometry, IClosedPlanar3D, ISegmentable3D
    {
        private List<Point3D> points;

        public Polygon3D(IEnumerable<Point3D> points)
        {
            if (points != null)
            {
                this.points = new List<Point3D>();
                foreach (Point3D point3D in points)
                    this.points. Add(point3D);

                if (this.points.First().Distance(this.points.Last()) == 0)
                    this.points.RemoveAt(this.points.Count - 1);
            }
        }

        public Polygon3D(Polygon3D polygon3D)
        {
            points = Point3D.Clone(polygon3D.points);
        }

        public Polygon3D(JObject jObject)
            : base(jObject)
        {

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

        public Plane GetPlane(double tolerance)
        {
            return Point3D.GetPlane(points, tolerance);
        }

        public Plane GetPlane()
        {
            return Point3D.GetPlane(points, Tolerance.MicroDistance);
        }

        public override ISAMGeometry Clone()
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

        public IClosed3D GetBoundary()
        {
            return new Polygon3D(this);
        }

        public List<ICurve3D> GetCurves()
        {
            return GetSegments().ConvertAll(x => (ICurve3D)x);
        }

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            return new Polygon3D(points.ConvertAll(x => (Point3D)x.GetMoved(vector3D)));
        }

        public double GetArea()
        {
            Plane plane = GetPlane();

            return Planar.Point2D.GetArea(points.ConvertAll(x => plane.Convert(x)));
        }

        public bool Inside(Polygon3D polygon3D, double tolerance = Tolerance.MicroDistance)
        {
            Plane plane_1 = GetPlane();
            Plane plane_2 = polygon3D.GetPlane();

            if (!plane_1.Coplanar(plane_2, tolerance))
                return false;

            List<Planar.Point2D> point2Ds_1 = points.ConvertAll(x => plane_1.Convert(x));
            List<Planar.Point2D> point2Ds_2 = polygon3D.points.ConvertAll(x => plane_1.Convert(x));

            return point2Ds_2.TrueForAll(x => Planar.Point2D.Inside(point2Ds_1, x));
        }

        public void Reverse()
        {
            points.Reverse();
        }

        public override bool FromJObject(JObject jObject)
        {
            points = Create.Point3Ds(jObject.Value<JArray>("Points"));
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Points", Geometry.Create.JArray(points));
            return jObject;
        }
    }
}
