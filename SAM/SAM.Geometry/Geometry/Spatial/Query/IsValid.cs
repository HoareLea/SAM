using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool IsValid(this Vector3D vector3D)
        {
            if (vector3D == null)
            {
                return false;
            }
                
            double x = vector3D.X;
            double y = vector3D.Y;
            double z = vector3D.Z;

            if (double.IsNaN(x) || double.IsNaN(x) || double.IsNaN(x))
            {
                return false;
            }

            if (x == 0 && y == 0 && z == 0)
            {
                return false;
            }

            return true;
        }

        public static bool IsValid(Point3D point3D)
        {
            if (point3D == null)
                return false;

            double x = point3D.X;
            double y = point3D.Y;
            double z = point3D.Z;

            if (double.IsNaN(x) || double.IsNaN(x) || double.IsNaN(x))
                return false;

            return true;
        }

        public static bool IsValid(this Plane plane)
        {
            if(plane == null)
            {
                return false;
            }

            if(!IsValid(plane.Origin))
            {
                return false;
            }

            if(!IsValid(plane.Normal))
            {
                return false;
            }

            if (!IsValid(plane.AxisY))
            {
                return false;
            }

            return true;
        }

        public static bool IsValid(this Polygon3D polygon3D)
        {
            if (polygon3D == null)
            {
                return false;
            }

            Plane plane = polygon3D.GetPlane();
            if(!IsValid(plane))
            {
                return false;
            }

            List<Point3D> point3Ds = polygon3D.GetPoints();
            if(point3Ds == null || point3Ds.Count < 3)
            {
                return false;
            }

            foreach(Point3D point3D in point3Ds)
            {
                if(!IsValid(point3D))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValid(this BoundingBox3D boundingBox3D)
        {
            if(boundingBox3D == null)
            {
                return false;
            }

            if(!IsValid(boundingBox3D.Min))
            {
                return false;
            }

            if(!IsValid(boundingBox3D.Max))
            {
                return false;
            }

            if(boundingBox3D.Min.Equals(boundingBox3D.Max))
            {
                return false;
            }

            return true;
        }

        public static bool IsValid(this Circle3D circle3D)
        {
            if(circle3D == null)
            {
                return false;
            }

            if(!IsValid(circle3D.GetPlane()))
            {
                return false;
            }

            if(double.IsNaN(circle3D.Radius))
            {
                return false;
            }

            return true;
        }

        public static bool IsValid(this Face3D face3D)
        {
            if(face3D == null)
            {
                return false;
            }
            
            if(!IsValid(face3D.GetPlane()))
            {
                return false;
            }

            if(!Planar.Query.IsValid(face3D.ExternalEdge2D))
            {
                return false;
            }

            List<Planar.IClosed2D> closed2Ds = face3D.InternalEdge2Ds;
            if(closed2Ds != null && closed2Ds.Count != 0)
            {
                foreach(Planar.IClosed2D closed2D in closed2Ds)
                {
                    if (!Planar.Query.IsValid(closed2D))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool IsValid(this Extrusion extrusion)
        {
            if(extrusion == null)
            {
                return false;
            }

            if(!IsValid(extrusion.Vector))
            {
                return false;
            }

            if(!IsValid(extrusion.Face3D))
            {
                return false;
            }

            return true;
        }

        public static bool IsValid(this Line3D line3D)
        {
            if(line3D == null)
            {
                return false;
            }

            if(!IsValid(line3D.Origin))
            {
                return false;
            }

            if(!IsValid(line3D.Direction))
            {
                return false;
            }

            return true;
        }

        public static bool IsValid(this ICurve3D curve3D)
        {
            if(curve3D == null)
            {
                return false;
            }

            return IsValid(curve3D as dynamic);
        }

        public static bool IsValid(this Polycurve3D polycurve3D)
        {
            if (polycurve3D == null)
            {
                return false;
            }

            List<ICurve3D> curve3Ds = polycurve3D.GetCurves();
            if (curve3Ds == null || curve3Ds.Count == 0)
            {
                return false;
            }

            foreach (ICurve3D curve3D in curve3Ds)
            {
                if (!IsValid(curve3D))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValid(this PolycurveLoop3D polycurveLoop3D)
        {
            if (polycurveLoop3D == null)
            {
                return false;
            }

            List<ICurve3D> curve3Ds = polycurveLoop3D.GetCurves();
            if (curve3Ds == null || curve3Ds.Count == 0)
            {
                return false;
            }

            return curve3Ds.TrueForAll(x => x.IsValid());
        }

        public static bool IsValid(this Polyline3D polyline3D)
        {
            if (polyline3D == null)
            {
                return false;
            }

            List<Segment3D> segment3D = polyline3D.GetSegments();
            if (segment3D == null || segment3D.Count == 0)
            {
                return false;
            }

            return segment3D.TrueForAll(x => x.IsValid());
        }

        public static bool IsValid(this Rectangle3D rectangle3D)
        {
            if (rectangle3D == null)
            {
                return false;
            }

            if(!IsValid(rectangle3D.GetPlane()))
            {
                return false;
            }

            if (!Planar.Query.IsValid(rectangle3D.Rectangle2D))
            {
                return false;
            }

            return true;
        }

        public static bool IsValid(this Segment3D segment3D)
        {
            if(segment3D == null)
            {
                return false;
            }

            if(!IsValid(segment3D.GetStart()))
            {
                return false;
            }

            if(!IsValid(segment3D.Direction))
            {
                return false;
            }

            return true;
        }

        public static bool IsValid(this Shell shell)
        {
            if(shell == null)
            {
                return false;
            }

            List<Face3D> face3Ds = shell?.Face3Ds;
            if(face3Ds == null || face3Ds.Count == 0)
            {
                return false;
            }

            return face3Ds.TrueForAll(x => IsValid(x));
        }

        public static bool IsValid(this Sphere sphere)
        {
            if(sphere == null)
            {
                return false;
            }

            if(!IsValid(sphere.Origin))
            {
                return false;
            }

            if(double.IsNaN(sphere.Radius))
            {
                return false;
            }

            return true;
        }

        public static bool IsValid(this IClosed3D closed3D)
        {
            if(closed3D == null)
            {
                return false;
            }

            return IsValid(closed3D as dynamic);
        }

        public static bool IsValid(this Surface surface)
        {
            if(surface == null)
            {
                return false;
            }

            if(!IsValid(surface.ExternalEdge3D))
            {
                return false;
            }

            return true;
        }

        public static bool IsValid(this Triangle3D triangle3D)
        {
            List<Point3D> point3Ds = triangle3D?.GetPoints();
            if(point3Ds == null || point3Ds.Count !=3)
            {
                return false;
            }

            return point3Ds.TrueForAll(x => IsValid(x));
        }
    }
}