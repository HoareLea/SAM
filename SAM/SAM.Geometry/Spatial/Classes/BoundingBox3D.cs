using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class BoundingBox3D : IClosed3D, ISegmentable3D
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
            max = Point3D.Max(point3D_1, point3D_2);
            min = Point3D.Min(point3D_1, point3D_2);
        }

        public BoundingBox3D(Point3D point3D_1, Point3D point3D_2, double offset)
        {
            max = Point3D.Max(point3D_1, point3D_2);
            min = Point3D.Min(point3D_1, point3D_2);

            max = new Point3D(max.X + offset, max.Y + offset, max.Z + offset);
            min = new Point3D(max.X - offset, max.Y - offset, max.Z - offset);
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
            max = new Point3D(boundingBox3D.min.X + offset, boundingBox3D.min.Y + offset, boundingBox3D.min.Z + offset);
        }

        public BoundingBox3D(IEnumerable<BoundingBox3D> boundingBox3Ds)
        { 
            foreach(BoundingBox3D boundingBox3D in boundingBox3Ds)
            {
                if (min == null)
                    min = new Point3D(boundingBox3D.Min);
                else
                    min = Point3D.Min(boundingBox3D.Min, min);

                if (max == null)
                    max = new Point3D(boundingBox3D.Max);
                else
                    max = Point3D.Max(boundingBox3D.Max, max);
            }
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
                    max = Point3D.Max(max, value);
                    min = Point3D.Min(max, value);
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
                    max = Point3D.Max(min, value);
                    min = Point3D.Min(min, value);
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

        public bool Inside(BoundingBox3D boundingBox3D)
        {
            return Inside(boundingBox3D.max) && Inside(boundingBox3D.min);
        }

        public bool Inside(Point3D point3D)
        {
            return point3D.X > min.X && point3D.X < max.X && point3D.Y < max.Y && point3D.Y > min.Y && point3D.Z < max.Z && point3D.Z > min.Z;
        }

        public bool Inside(Point3D point3D, bool acceptOnEdge = true, double tolerance = Tolerance.MicroDistance)
        {
            if (point3D == null)
                return false;

            if (acceptOnEdge)
                return (point3D.X >= min.X - tolerance && point3D.X <= max.X + tolerance && point3D.Y >= min.Y - tolerance && point3D.Y <= max.Y + tolerance && point3D.Z >= min.Z - tolerance && point3D.Z <= max.Z + tolerance);

            return (point3D.X > min.X + tolerance && point3D.X < max.X - tolerance && point3D.Y > min.Y + tolerance && point3D.Y < max.Y - tolerance && point3D.Z > min.Z + tolerance && point3D.Z < max.Z - tolerance);
        }

        public IGeometry Clone()
        {
            return new BoundingBox3D(this);
        }

        public List<Segment3D> GetSegments()
        {
            double x = Width;
            double z = Height;
            double y = Depth;
            
            List<Segment3D> result = new List<Segment3D>();
            result.Add(new Segment3D(new Point3D(min), new Point3D(min.X + x, min.Y, Min.Z)));
            result.Add(new Segment3D(new Point3D(min.X + x, min.Y, Min.Z), new Point3D(min.X + x, min.Y + y, Min.Z)));
            result.Add(new Segment3D(new Point3D(min.X + x, min.Y + y, Min.Z), new Point3D(min.X, min.Y + y, Min.Z)));
            result.Add(new Segment3D(new Point3D(min.X, min.Y + y, Min.Z), new Point3D(min)));

            result.Add(new Segment3D(new Point3D(min.X, min.Y, min.Z + z), new Point3D(min.X + x, min.Y, Min.Z + z)));
            result.Add(new Segment3D(new Point3D(min.X + x, min.Y, Min.Z + z), new Point3D(min.X + x, min.Y + y, Min.Z + z)));
            result.Add(new Segment3D(new Point3D(min.X + x, min.Y + y, Min.Z + z), new Point3D(min.X, min.Y + y, Min.Z + z)));
            result.Add(new Segment3D(new Point3D(min.X, min.Y + y, Min.Z + z), new Point3D(min.X, min.Y, min.Z + z)));

            result.Add(new Segment3D(new Point3D(min), new Point3D(min.X, min.Y, Min.Z + z)));
            result.Add(new Segment3D(new Point3D(min.X + x, min.Y, Min.Z), new Point3D(min.X + x, min.Y, Min.Z + z)));
            result.Add(new Segment3D(new Point3D(min.X + x, min.Y + y, Min.Z), new Point3D(min.X + x, min.Y + y, Min.Z + z)));
            result.Add(new Segment3D(new Point3D(min.X + x, min.Y + y, Min.Z), new Point3D(min.X + x, min.Y + y, Min.Z + z)));
            result.Add(new Segment3D(new Point3D(min.X, min.Y + y, Min.Z), new Point3D(min.X, min.Y + y, Min.Z + z)));
            return result;
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox3D(this, offset);
        }
    }
}
