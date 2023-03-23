using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public class BoundingBox3D : SAMGeometry, IClosed3D, ISegmentable3D
    {
        private Point3D min;
        private Point3D max;

        public BoundingBox3D(IEnumerable<Point3D> point3Ds)
        {
            double aX_Min = double.MaxValue;
            double aX_Max = double.MinValue;
            double aY_Min = double.MaxValue;
            double aY_Max = double.MinValue;
            double aZ_Min = double.MaxValue;
            double aZ_Max = double.MinValue;
            foreach (Point3D point3D in point3Ds)
            {
                if (point3D.X > aX_Max)
                    aX_Max = point3D.X;
                if (point3D.X < aX_Min)
                    aX_Min = point3D.X;
                if (point3D.Y > aY_Max)
                    aY_Max = point3D.Y;
                if (point3D.Y < aY_Min)
                    aY_Min = point3D.Y;
                if (point3D.Z > aZ_Max)
                    aZ_Max = point3D.Z;
                if (point3D.Z < aZ_Min)
                    aZ_Min = point3D.Z;
            }

            min = new Point3D(aX_Min, aY_Min, aZ_Min);
            max = new Point3D(aX_Max, aY_Max, aZ_Max);
        }

        public BoundingBox3D(Point3D point3D_1, Point3D point3D_2)
        {
            max = Query.Max(point3D_1, point3D_2);
            min = Query.Min(point3D_1, point3D_2);
        }

        public BoundingBox3D(Point3D point3D_1, Point3D point3D_2, double offset)
        {
            max = Query.Max(point3D_1, point3D_2);
            min = Query.Min(point3D_1, point3D_2);

            max = new Point3D(max.X + offset, max.Y + offset, max.Z + offset);
            min = new Point3D(min.X - offset, min.Y - offset, min.Z - offset);
        }

        public BoundingBox3D(Point3D point3D, double offset)
        {
            min = new Point3D(point3D.X - offset, point3D.Y - offset, point3D.Z - offset);
            max = new Point3D(point3D.X + offset, point3D.Y + offset, point3D.Z - offset);
        }

        public BoundingBox3D(IEnumerable<Point3D> point3Ds, double offset)
        {
            double aX_Min = double.MaxValue;
            double aX_Max = double.MinValue;
            double aY_Min = double.MaxValue;
            double aY_Max = double.MinValue;
            double aZ_Min = double.MaxValue;
            double aZ_Max = double.MinValue;
            foreach (Point3D point3D in point3Ds)
            {
                if (point3D.X > aX_Max)
                    aX_Max = point3D.X;
                if (point3D.X < aX_Min)
                    aX_Min = point3D.X;
                if (point3D.Y > aY_Max)
                    aY_Max = point3D.Y;
                if (point3D.Y < aY_Min)
                    aY_Min = point3D.Y;
                if (point3D.Z > aZ_Max)
                    aZ_Max = point3D.Z;
                if (point3D.Z < aZ_Min)
                    aZ_Min = point3D.Z;
            }

            min = new Point3D(aX_Min - offset, aY_Min - offset, aZ_Min - offset);
            max = new Point3D(aX_Max + offset, aY_Max + offset, aZ_Max + offset);
        }

        public BoundingBox3D(BoundingBox3D boundingBox3D)
        {
            min = new Point3D(boundingBox3D.min);
            max = new Point3D(boundingBox3D.max);
        }

        public BoundingBox3D(BoundingBox3D boundingBox3D, double offset)
        {
            min = new Point3D(boundingBox3D.min.X - offset, boundingBox3D.min.Y - offset, boundingBox3D.min.Z - offset);
            max = new Point3D(boundingBox3D.max.X + offset, boundingBox3D.max.Y + offset, boundingBox3D.max.Z + offset);
        }

        public BoundingBox3D(IEnumerable<BoundingBox3D> boundingBox3Ds)
        {
            foreach (BoundingBox3D boundingBox3D in boundingBox3Ds)
            {
                if (min == null)
                    min = new Point3D(boundingBox3D.Min);
                else
                    min = Query.Min(boundingBox3D.Min, min);

                if (max == null)
                    max = new Point3D(boundingBox3D.Max);
                else
                    max = Query.Max(boundingBox3D.Max, max);
            }
        }

        public BoundingBox3D(JObject jObject)
            : base(jObject)
        {
        }

        public bool Intersect(BoundingBox3D boundingBox3D)
        {
            return (min.X <= boundingBox3D.max.X && max.X >= boundingBox3D.min.X) && (min.Y <= boundingBox3D.max.Y && max.Y >= boundingBox3D.min.Y) && (min.Z <= boundingBox3D.max.Z && max.Z >= boundingBox3D.min.Z);
        }

        public bool Intersect(Segment3D segment3D, double tolerance = Core.Tolerance.Distance)
        {
            if (segment3D == null)
            {
                return false;
            }

            Point3D point3D_1 = segment3D[0];
            Point3D point3D_2 = segment3D[1];

            if(!InRange(new BoundingBox3D(point3D_1, point3D_2)))
            {
                return false;
            }

            bool inside_1 = Inside(point3D_1);
            bool inside_2 = Inside(point3D_2);

            if (inside_1 != inside_2)
            {
                return true;
            }


            if (inside_1 && inside_2)
            {
                return false;
            }

            foreach (Plane plane in GetPlanes())
            {
                PlanarIntersectionResult planarIntersectionResult = Create.PlanarIntersectionResult(plane, segment3D, tolerance);
                if (planarIntersectionResult != null && planarIntersectionResult.Intersecting)
                {
                    ISAMGeometry3D geometry3D = planarIntersectionResult.Geometry3D;
                    if (geometry3D is Point3D)
                    {
                        if (On((Point3D)geometry3D, tolerance))
                        {
                            return true;
                        }
                    }
                    else if (geometry3D is Segment3D)
                    {
                        Segment3D segment3D_Temp = (Segment3D)geometry3D;
                        if (On(point3D_1, tolerance) || On(point3D_2, tolerance))
                        {
                            return true;
                        }
                    }
                }
                return true;
            }

            return false;
        }

        public Point3D Min
        {
            get
            {
                return new Point3D(min);
            }
            set
            {
                if (max == null)
                {
                    max = new Point3D(value);
                    min = new Point3D(value);
                }
                else
                {
                    max = Query.Max(max, value);
                    min = Query.Min(max, value);
                }
            }
        }

        public Point3D Max
        {
            get
            {
                return new Point3D(max);
            }
            set
            {
                if (min == null)
                {
                    max = new Point3D(value);
                    min = new Point3D(value);
                }
                else
                {
                    max = Query.Max(min, value);
                    min = Query.Min(min, value);
                }
            }
        }

        public double Width
        {
            get
            {
                return max.X - min.X;
            }
        }

        public double Height
        {
            get
            {
                return max.Z - min.Z;
            }
        }

        public double Depth
        {
            get
            {
                return max.Y - min.Y;
            }
        }

        public List<Plane> GetPlanes()
        {
            List<Plane> planes = new List<Plane>();
            planes.Add(new Plane(min, Vector3D.WorldX));
            planes.Add(new Plane(min, Vector3D.WorldY));
            planes.Add(new Plane(min, Vector3D.WorldZ));
            planes.Add(new Plane(max, Vector3D.WorldX));
            planes.Add(new Plane(max, Vector3D.WorldY));
            planes.Add(new Plane(max, Vector3D.WorldZ));
            return planes;
        }

        public bool Inside(BoundingBox3D boundingBox3D)
        {
            return Inside(boundingBox3D.max) && Inside(boundingBox3D.min);
        }

        public bool Inside(Point3D point3D)
        {
            return point3D.X > min.X && point3D.X < max.X && point3D.Y < max.Y && point3D.Y > min.Y && point3D.Z < max.Z && point3D.Z > min.Z;
        }

        public bool Inside(Point3D point3D, bool acceptOnEdge = true, double tolerance = Core.Tolerance.Distance)
        {
            if (point3D == null)
                return false;

            if (acceptOnEdge)
                return (point3D.X >= min.X - tolerance && point3D.X <= max.X + tolerance && point3D.Y >= min.Y - tolerance && point3D.Y <= max.Y + tolerance && point3D.Z >= min.Z - tolerance && point3D.Z <= max.Z + tolerance);

            return (point3D.X > min.X + tolerance && point3D.X < max.X - tolerance && point3D.Y > min.Y + tolerance && point3D.Y < max.Y - tolerance && point3D.Z > min.Z + tolerance && point3D.Z < max.Z - tolerance);
        }

        public bool Inside(Segment3D segment3D, bool acceptOnEdge = true, double tolerance = Core.Tolerance.Distance)
        {
            if (segment3D == null)
                return false;

            if (!Inside(segment3D[0], acceptOnEdge, tolerance))
                return false;

            if (!Inside(segment3D[1], acceptOnEdge, tolerance))
                return false;

            return true;
        }

        public bool InRange(BoundingBox3D boundingBox3D, double tolerance = Core.Tolerance.Distance)
        {
            return Query.InRange(this, boundingBox3D, tolerance);
        }
        
        public override ISAMGeometry Clone()
        {
            return new BoundingBox3D(this);
        }

        public double GetArea()
        {
            return Width * Height;
        }

        public double GetVolume()
        {
            return Width * Height * Depth;
        }

        public List<Segment3D> GetSegments()
        {
            double x = Width;
            double z = Height;
            double y = Depth;

            List<Segment3D> result = new List<Segment3D>();
            result.Add(new Segment3D(new Point3D(min), new Point3D(min.X + x, min.Y, min.Z)));
            result.Add(new Segment3D(new Point3D(min.X + x, min.Y, min.Z), new Point3D(min.X + x, min.Y + y, min.Z)));
            result.Add(new Segment3D(new Point3D(min.X + x, min.Y + y, min.Z), new Point3D(min.X, min.Y + y, min.Z)));
            result.Add(new Segment3D(new Point3D(min.X, min.Y + y, min.Z), new Point3D(min)));

            result.Add(new Segment3D(new Point3D(min.X, min.Y, min.Z + z), new Point3D(min.X + x, min.Y, min.Z + z)));
            result.Add(new Segment3D(new Point3D(min.X + x, min.Y, min.Z + z), new Point3D(min.X + x, min.Y + y, min.Z + z)));
            result.Add(new Segment3D(new Point3D(min.X + x, min.Y + y, min.Z + z), new Point3D(min.X, min.Y + y, min.Z + z)));
            result.Add(new Segment3D(new Point3D(min.X, min.Y + y, min.Z + z), new Point3D(min.X, min.Y, min.Z + z)));

            result.Add(new Segment3D(new Point3D(min), new Point3D(min.X, min.Y, min.Z + z)));
            result.Add(new Segment3D(new Point3D(min.X + x, min.Y, min.Z), new Point3D(min.X + x, min.Y, min.Z + z)));
            result.Add(new Segment3D(new Point3D(min.X + x, min.Y + y, min.Z), new Point3D(min.X + x, min.Y + y, min.Z + z)));
            result.Add(new Segment3D(new Point3D(min.X + x, min.Y + y, min.Z), new Point3D(min.X + x, min.Y + y, min.Z + z)));
            result.Add(new Segment3D(new Point3D(min.X, min.Y + y, min.Z), new Point3D(min.X, min.Y + y, min.Z + z)));
            return result;
        }

        public List<Point3D> GetPoints()
        {
            double x = Width;
            double z = Height;
            double y = Depth;

            List<Point3D> point3Ds = new List<Point3D>();
            point3Ds.Add(new Point3D(min));
            point3Ds.Add(new Point3D(min.X + x, min.Y, Min.Z));
            point3Ds.Add(new Point3D(min.X + x, min.Y + y, Min.Z));
            point3Ds.Add(new Point3D(min.X, min.Y + y, Min.Z));

            point3Ds.Add(new Point3D(max));
            point3Ds.Add(new Point3D(max.X + x, max.Y, max.Z));
            point3Ds.Add(new Point3D(max.X + x, max.Y + y, max.Z));
            point3Ds.Add(new Point3D(max.X, max.Y + y, max.Z));

            return point3Ds;
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox3D(this, offset);
        }

        public IClosed3D GetExternalEdge()
        {
            return new BoundingBox3D(this);
        }

        public List<ICurve3D> GetCurves()
        {
            return GetSegments().ConvertAll(x => (ICurve3D)x);
        }

        public Point3D GetCentroid()
        {
            return Query.Mid(min, max);
        }

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            return new BoundingBox3D((Point3D)min.GetMoved(vector3D), (Point3D)max.GetMoved(vector3D));
        }

        public override bool FromJObject(JObject jObject)
        {
            max = new Point3D(jObject.Value<JObject>("Max"));
            min = new Point3D(jObject.Value<JObject>("Min"));

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Max", max.ToJObject());
            jObject.Add("Min", min.ToJObject());

            return jObject;
        }

        public bool On(Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            return Query.On(this, point3D, tolerance);
        }

        public bool Include(BoundingBox3D boundingBox3D)
        {
            if (boundingBox3D == null)
                return false;

            max = Query.Max(max, boundingBox3D.Max);
            min = Query.Min(min, boundingBox3D.Min);
            return true;
        }

        public bool Include(Point3D point3D)
        {
            if(point3D == null)
            {
                return false;
            }

            max = Query.Max(max, point3D);
            min = Query.Min(min, point3D);
            return true;
        }

        public bool Include(IEnumerable<Point3D> point3Ds)
        {
            if(point3Ds == null || point3Ds.Count() == 0)
            {
                return false;
            }

            max = Query.Max(max, Query.Max(point3Ds));
            min = Query.Min(min, Query.Min(point3Ds));
            return true;
        }

        public ISAMGeometry3D GetTransformed(Transform3D transform3D)
        {
            if(transform3D == null)
            {
                return null;
            }    

            return Query.Transform(this, transform3D);
        }

        public double GetLength()
        {
            return (4 * Height) + (4 * Width) + (4 * Depth);
        }

        public static bool operator ==(BoundingBox3D boundingBox3D_1, BoundingBox3D boundingBox3D_2)
        {
            if (ReferenceEquals(boundingBox3D_1, null) && ReferenceEquals(boundingBox3D_2, null))
                return true;

            if (ReferenceEquals(boundingBox3D_1, null))
                return false;

            if (ReferenceEquals(boundingBox3D_2, null))
                return false;

            return boundingBox3D_1.min == boundingBox3D_2.min && boundingBox3D_1.max == boundingBox3D_2.max;
        }

        public static bool operator !=(BoundingBox3D boundingBox3D_1, BoundingBox3D boundingBox3D_2)
        {
            if (ReferenceEquals(boundingBox3D_1, null) && ReferenceEquals(boundingBox3D_2, null))
                return false;

            if (ReferenceEquals(boundingBox3D_1, null))
                return true;

            if (ReferenceEquals(boundingBox3D_2, null))
                return true;

            return boundingBox3D_1.min != boundingBox3D_2.min || boundingBox3D_1.max != boundingBox3D_2.max;
        }
    }
}