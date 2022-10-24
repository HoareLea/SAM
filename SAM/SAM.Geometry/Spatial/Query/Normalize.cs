using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static T Normalize<T>(this T closedPlanar3D, Orientation orientation = Orientation.CounterClockwise, EdgeOrientationMethod edgeOrientationMethod = EdgeOrientationMethod.Opposite) where T : IClosedPlanar3D
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

            closed2D = Normalize(plane, closed2D, orientation, edgeOrientationMethod);
            if (closed2D == null)
            {
                return default(T);
            }

            IClosedPlanar3D result = plane.Convert(closed2D);

            return result is T ? (T)result : default(T);
        }

        public static T Normalize<T>(this Plane plane, T closed2D, Orientation orientation = Orientation.CounterClockwise, EdgeOrientationMethod edgeOrientationMethod = EdgeOrientationMethod.Opposite) where T: Planar.IClosed2D
        {
            if(plane == null || closed2D == null)
            {
                return default(T);
            }

            Planar.IClosed2D result = null;
            if (closed2D is Planar.Face2D)
            {
                result = Normalize(plane, (Planar.Face2D)(object)closed2D, orientation, edgeOrientationMethod);
            }
            else
            {
                result = Normalize(plane, closed2D as dynamic, orientation);
            }


            return result is T ? (T)result : default(T);
        }

        public static List<Planar.Point2D> Normalize(this Plane plane, IEnumerable<Planar.Point2D> point2Ds, Orientation orientation = Orientation.CounterClockwise)
        {
            if (point2Ds == null || plane == null)
            {
                return null;
            }

            Vector3D normal = Normal(plane, point2Ds);
            if (normal == null)
            {
                return null;
            }

            if (orientation == Orientation.Clockwise)
            {
                normal.Negate();
            }

            List<Planar.Point2D> result = new List<Planar.Point2D>(point2Ds);

            if (!plane.Normal.SameHalf(normal))
            {
                result.Reverse();
            }

            return result;
        }

        public static Planar.Face2D Normalize(this Plane plane, Planar.Face2D face2D, Orientation orientation = Orientation.CounterClockwise, EdgeOrientationMethod edgeOrientationMethod = EdgeOrientationMethod.Opposite)
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

            externalEdge2D = Normalize(plane, externalEdge2D as dynamic, orientation);
            if(externalEdge2D == null)
            {
                return null;
            }

            return Planar.Create.Face2D(externalEdge2D, face2D.InternalEdge2Ds, edgeOrientationMethod);
        }

        public static Planar.Polygon2D Normalize(this Plane plane, Planar.Polygon2D polygon2D, Orientation orientation = Orientation.CounterClockwise)
        {
            if(plane == null || polygon2D == null)
            {
                return null;
            }

            List<Planar.Point2D> point2Ds = Normalize(plane, polygon2D.Points, orientation);
            if(point2Ds == null)
            {
                return null;
            }

            return new Planar.Polygon2D(point2Ds);
        }
    }
}