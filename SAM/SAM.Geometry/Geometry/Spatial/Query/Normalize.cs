﻿using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static T Normalize<T>(this T closedPlanar3D, Orientation orientation = Geometry.Orientation.CounterClockwise, EdgeOrientationMethod edgeOrientationMethod = EdgeOrientationMethod.Opposite, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance) where T : IClosedPlanar3D
        {
            Plane plane = closedPlanar3D?.GetPlane();
            if (plane == null)
            {
                return default(T);
            }

            Planar.IClosed2D closed2D = plane.Convert(closedPlanar3D);
            if (closed2D == null)
            {
                return default(T);
            }

            closed2D = Normalize(plane, closed2D, orientation, edgeOrientationMethod, tolerance_Angle, tolerance_Distance);
            if (closed2D == null)
            {
                return default(T);
            }

            IClosedPlanar3D result = plane.Convert(closed2D);

            return result is T ? (T)result : default(T);
        }

        public static T Normalize<T>(this Plane plane, T closed2D, Orientation orientation = Geometry.Orientation.CounterClockwise, EdgeOrientationMethod edgeOrientationMethod = EdgeOrientationMethod.Opposite, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance) where T: Planar.IClosed2D
        {
            if(plane == null || closed2D == null)
            {
                return default(T);
            }

            Planar.IClosed2D result = null;
            if (closed2D is Planar.Face2D)
            {
                result = Normalize(plane, (Planar.Face2D)(object)closed2D, orientation, edgeOrientationMethod, tolerance_Angle, tolerance_Distance);
            }
            else
            {
                result = Normalize(plane, closed2D as dynamic, orientation, tolerance_Angle, tolerance_Distance);
            }


            return result is T ? (T)result : default(T);
        }

        public static List<Planar.Point2D> Normalize(this Plane plane, IEnumerable<Planar.Point2D> point2Ds, Orientation orientation = Geometry.Orientation.CounterClockwise, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (point2Ds == null || plane == null)
            {
                return null;
            }

            Orientation orientation_Point3Ds = Orientation(plane.Convert(point2Ds), plane.Normal, tolerance_Angle, tolerance_Distance);
            if(orientation_Point3Ds == Geometry.Orientation.Undefined || orientation_Point3Ds == Geometry.Orientation.Collinear)
            {
                return null;
            }

            List<Planar.Point2D> result = new List<Planar.Point2D>(point2Ds);
            if (orientation_Point3Ds != orientation )
            {
                result.Reverse();
            }

            return result;
        }

        public static Planar.Face2D Normalize(this Plane plane, Planar.Face2D face2D, Orientation orientation = Geometry.Orientation.CounterClockwise, EdgeOrientationMethod edgeOrientationMethod = EdgeOrientationMethod.Opposite, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if(plane == null || face2D is null)
            {
                return null;
            }

            Planar.IClosed2D externalEdge2D = face2D.ExternalEdge2D;
            if(externalEdge2D == null)
            {
                return null;
            }

            externalEdge2D = Normalize(plane, externalEdge2D as dynamic, orientation, tolerance_Angle, tolerance_Distance);
            if(externalEdge2D == null)
            {
                return null;
            }

            return Planar.Create.Face2D(externalEdge2D, face2D.InternalEdge2Ds, edgeOrientationMethod);
        }

        public static Planar.Polygon2D Normalize(this Plane plane, Planar.Polygon2D polygon2D, Orientation orientation = Geometry.Orientation.CounterClockwise, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if(plane == null || polygon2D == null)
            {
                return null;
            }

            List<Planar.Point2D> point2Ds = Normalize(plane, polygon2D.Points, orientation, tolerance_Angle, tolerance_Distance);
            if(point2Ds == null)
            {
                return null;
            }

            return new Planar.Polygon2D(point2Ds);
        }

        public static Planar.Triangle2D Normalize(this Plane plane, Planar.Triangle2D triangle2D, Orientation orientation = Geometry.Orientation.CounterClockwise, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (plane == null || triangle2D == null)
            {
                return null;
            }

            List<Planar.Point2D> point2Ds = Normalize(plane, triangle2D.GetPoints(), orientation, tolerance_Angle, tolerance_Distance);
            if (point2Ds == null)
            {
                return null;
            }

            return new Planar.Triangle2D(point2Ds[0], point2Ds[1], point2Ds[2]);
        }

        public static Planar.Rectangle2D Normalize(this Plane plane, Planar.Rectangle2D rectangle2D, Orientation orientation = Geometry.Orientation.CounterClockwise, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (plane == null || rectangle2D == null)
            {
                return null;
            }

           if(IsNormalized(plane, rectangle2D, orientation, tolerance_Angle: tolerance_Angle, tolerance_Distance: tolerance_Distance))
            {
                return new Planar.Rectangle2D(rectangle2D);
            }

            return new Planar.Rectangle2D(rectangle2D.Origin, rectangle2D.Width, rectangle2D.Height, rectangle2D.WidthDirection);
        }

        public static Planar.Polyline2D Normalize(this Plane plane, Planar.Polyline2D polyline2D, Orientation orientation = Geometry.Orientation.CounterClockwise, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (plane == null || polyline2D == null)
            {
                return null;
            }

            List<Planar.Point2D> point2Ds = Normalize(plane, polyline2D.Points, orientation, tolerance_Angle, tolerance_Distance);
            if (point2Ds == null)
            {
                return null;
            }

            return new Planar.Polyline2D(point2Ds);
        }
    }
}