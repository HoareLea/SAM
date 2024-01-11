using Newtonsoft.Json.Linq;
using SAM.Geometry.Planar;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public class Rectangle3D : SAMGeometry, IClosedPlanar3D, ISegmentable3D
    {
        private Rectangle2D rectangle2D;
        private Plane plane;

        public double Width
        {
            get
            {
                return rectangle2D.Width;
            }
            set
            {
                rectangle2D.Width = value;
            }
        }

        public double Height
        {
            get
            {
                return rectangle2D.Height;
            }
            set
            {
                rectangle2D.Height = value;
            }
        }

        public Point3D Origin
        {
            get
            {
                return plane.Convert(rectangle2D.Origin);
            }
        }

        public Rectangle3D(JObject jObject)
            : base(jObject)
        {

        }

        public Rectangle3D(Rectangle3D rectangle3D)
        {
            rectangle2D = new Rectangle2D(rectangle3D.rectangle2D);
            plane = new Plane(rectangle3D.plane);
        }

        public Rectangle3D(Plane plane, Rectangle2D rectangle2D)
        {
            this.rectangle2D = new Rectangle2D(rectangle2D);
            this.plane = new Plane(plane);
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox3D(GetPoints(), offset);
        }

        public List<ICurve3D> GetCurves()
        {
            return GetSegments().ConvertAll(x => (ICurve3D)x );
        }

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            return new Rectangle3D((Plane)plane.GetMoved(vector3D), rectangle2D);
        }

        public ISAMGeometry3D GetTransformed(Transform3D transform3D)
        {
            if (transform3D == null)
            {
                return null;
            }

            return Query.Transform(this, transform3D);
        }

        public List<Point3D> GetPoints()
        {
            return rectangle2D.GetPoints().ConvertAll(x => plane.Convert(x));
        }

        public List<Segment3D> GetSegments()
        {
            return rectangle2D.GetSegments().ConvertAll(x => plane.Convert(x));
        }

        public Vector3D WidthDirection
        {
            get
            {
                return plane?.Convert(rectangle2D?.HeightDirection?.GetPerpendicular(Orientation.Clockwise));
            }
        }

        public Vector3D HeightDirection
        {
            get
            {
                return plane?.Convert(rectangle2D?.HeightDirection);
            }
        }

        public bool On(Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            return rectangle2D.On(plane.Convert(point3D), tolerance);
        }

        public override bool FromJObject(JObject jObject)
        {
            rectangle2D = new Rectangle2D(jObject.Value<JObject>("Rectangle2D"));
            plane = new Plane(jObject.Value<JObject>("Plane"));

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Rectangle2D", rectangle2D.ToJObject());
            jObject.Add("Plane", plane.ToJObject());

            return jObject;
        }

        public double GetArea()
        {
            return rectangle2D.GetArea();
        }

        public Plane GetPlane()
        {
            if(plane == null)
            {
                return null;
            }
            
            return new Plane(plane);
        }

        public Rectangle2D Rectangle2D
        {
            get
            {
                if(rectangle2D == null)
                {
                    return null;
                }

                return new Rectangle2D(rectangle2D);
            }
        }

        public void Reverse()
        {
            throw new System.NotImplementedException();
        }

        public override ISAMGeometry Clone()
        {
            return new Rectangle3D(plane, (Rectangle2D)rectangle2D?.Clone());
        }

        public Point3D GetCentroid()
        {
            return plane?.Convert(Planar.Query.Centroid(rectangle2D?.GetPoints()));
        }

        public double GetLength()
        {
            if(rectangle2D == null)
            {
                return double.NaN;
            }

            return rectangle2D.GetLength();
        }
    }
}