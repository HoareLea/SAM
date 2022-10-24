using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool IsNormalized(this Plane plane, Planar.IClosed2D closed2D, Orientation orientation = Geometry.Orientation.CounterClockwise, EdgeOrientationMethod edgeOrientationMethod = EdgeOrientationMethod.Opposite)
        {
            if (closed2D == null)
            {
                return false;
            }

            return IsNormalized(plane.Convert(closed2D), orientation, edgeOrientationMethod);
        }

        public static bool IsNormalized(this IClosedPlanar3D closedPlanar3D, Orientation orientation = SAM.Geometry.Orientation.CounterClockwise, EdgeOrientationMethod edgeOrientationMethod = EdgeOrientationMethod.Opposite)
        {
            Plane plane = closedPlanar3D?.GetPlane();
            if (plane == null)
            {
                return false;
            }

            IClosedPlanar3D externalEdge3D = closedPlanar3D;
            if (closedPlanar3D is Face3D)
            {
                externalEdge3D = ((Face3D)externalEdge3D).GetExternalEdge3D();
            }

            if (externalEdge3D == null)
            {
                return false;
            }

            Vector3D normal = plane.Normal;
            if(normal == null)
            {
                return false;
            }

            Orientation orientation_ExternalEdge3D = Orientation(externalEdge3D, normal);

            bool result = orientation_ExternalEdge3D == orientation;
            if (!(closedPlanar3D is Face3D) || !result)
            {
                return result;
            }

            Face3D face3D = (Face3D)closedPlanar3D;

            List<IClosedPlanar3D> internalEdge3Ds = face3D.GetInternalEdge3Ds();
            if (internalEdge3Ds == null || internalEdge3Ds.Count == 0)
            {
                return result;
            }

            if (edgeOrientationMethod != EdgeOrientationMethod.Similar)
            {
                orientation_ExternalEdge3D = orientation.Opposite();
            }

            foreach (IClosedPlanar3D internalEdge3D in internalEdge3Ds)
            {
                Orientation orientation_Temp = Orientation(internalEdge3D, normal);
                if (orientation_ExternalEdge3D != orientation_Temp)
                {
                    result = false;
                    break;
                }
            }

            return result;

        }

    }
}