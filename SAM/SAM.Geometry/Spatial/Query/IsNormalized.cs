using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool IsNormalized(this Plane plane, Planar.IClosed2D closed2D, Orientation orientation = Orientation.CounterClockwise, EdgeOrientationMethod edgeOrientationMethod = EdgeOrientationMethod.Opposite)
        {
            if (plane == null || closed2D == null)
            {
                return false;
            }

            Planar.IClosed2D externalEdge2D = closed2D;
            if (closed2D is Planar.Face2D)
            {
                externalEdge2D = (Planar.Face2D)externalEdge2D;
            }

            if (externalEdge2D == null)
            {
                return false;
            }

            Planar.ISegmentable2D segmentable2D = externalEdge2D as Planar.ISegmentable2D;
            if (segmentable2D == null)
            {
                throw new System.NotImplementedException();
            }

            Vector3D normal = Normal(plane, segmentable2D.GetPoints());
            if (normal == null)
            {
                return false;
            }

            if (orientation == Orientation.Clockwise)
            {
                normal.GetNegated();
            }

            bool result = plane.Normal.SameHalf(normal);
            if (!(closed2D is Planar.Face2D) || !result)
            {
                return result;
            }

            Planar.Face2D face2D = (Planar.Face2D)closed2D;

            List<Planar.IClosed2D> internalEdge2Ds = face2D.InternalEdge2Ds;
            if (internalEdge2Ds == null || internalEdge2Ds.Count == 0)
            {
                return result;
            }

            Orientation orinetation = Planar.Query.Orientation(externalEdge2D);
            if (edgeOrientationMethod != EdgeOrientationMethod.Similar)
            {
                orinetation = orientation.Opposite();
            }

            foreach (Planar.IClosed2D internalEdge2D in internalEdge2Ds)
            {
                Orientation orientation_Temp = Planar.Query.Orientation(internalEdge2D);
                if (orinetation != orientation_Temp)
                {
                    result = false;
                    break;
                }
            }

            return result;
        }

        public static bool IsNormalized(this IClosedPlanar3D closedPlanar3D, Orientation orientation = Orientation.CounterClockwise, EdgeOrientationMethod edgeOrientationMethod = EdgeOrientationMethod.Opposite)
        {
            Plane plane = closedPlanar3D?.GetPlane();
            if (plane == null)
            {
                return false;
            }

            Planar.IClosed2D closed2D = plane.Convert(closedPlanar3D);
            if(closed2D == null)
            {
                return false;
            }

            return IsNormalized(plane, closed2D, orientation, edgeOrientationMethod);
        }

    }
}