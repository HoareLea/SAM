using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class Triangle3D : SAMGeometry, IClosedPlanar3D, ISegmentable3D, IBoundable3D
    {
        private Point3D[] points = new Point3D[3];

        public Triangle3D(Point3D point3D_1, Point3D point3D_2, Point3D point3D_3)
        {
            points[0] = point3D_1;
            points[1] = point3D_2;
            points[2] = point3D_3;
        }

        public Triangle3D(Triangle3D triangle3D)
        {
            points = Point3D.Clone(triangle3D.points).ToArray();
        }

        public Triangle3D(JObject jObject)
            : base(jObject)
        {

        }

        public List<Point3D> GetPoints()
        {
            return new List<Point3D>() { new Point3D(points[0]), new Point3D(points[1]), new Point3D(points[2]) };
        }

        public Vector3D GetNormal()
        {
            if (points.Length < 3)
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
            throw new NotImplementedException();
        }

        public List<Segment3D> GetSegments()
        {
            return new List<Segment3D>() { new Segment3D(points[0], points[1]), new Segment3D(points[1], points[2]), new Segment3D(points[2], points[0]) };
        }

        public List<ICurve3D> GetCurves()
        {
            return new List<ICurve3D>() { new Segment3D(points[0], points[1]), new Segment3D(points[1], points[2]), new Segment3D(points[2], points[0]) };
        }

        public Polygon3D ToPolygon()
        {
            return new Polygon3D(points);
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox3D(points, offset);
        }
    
        public static List<Polygon3D> ToPolygons(IEnumerable<Triangle3D> triangle3Ds)
        {
            List<Polygon3D> result = new List<Polygon3D>();
            foreach (Triangle3D triangle3D in triangle3Ds)
                result.Add(triangle3D.ToPolygon());

            return result;
        }

        public IClosed3D GetExternalEdge()
        {
            return new Triangle3D(this);
        }

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            return new Triangle3D((Point3D)points[0].GetMoved(vector3D), (Point3D)points[1].GetMoved(vector3D), (Point3D)points[2].GetMoved(vector3D));
        }

        public double GetArea()
        {
            Plane plane = GetPlane();

            return Planar.Point2D.GetArea(points.ToList().ConvertAll(x => plane.Convert(x)));
        }

        public void Reverse()
        {
            points.Reverse();
        }

        public override bool FromJObject(JObject jObject)
        {
            points = Geometry.Create.ISAMGeometries<Point3D>(jObject.Value<JArray>("Points")).ToArray();
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
