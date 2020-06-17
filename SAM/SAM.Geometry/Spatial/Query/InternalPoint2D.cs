using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Planar.Point2D InternalPoint2D(this IClosedPlanar3D closedPlanar3D, double tolerance = Core.Tolerance.Distance)
        {
            if (closedPlanar3D == null)
                return null;

            if (closedPlanar3D is Face3D)
                return InternalPoint2D((Face3D)closedPlanar3D);

            Plane plane = closedPlanar3D.GetPlane();
            if (plane == null)
                return null;

            Planar.IClosed2D closed2D = plane.Convert(closedPlanar3D);
            if (closed2D == null)
                return null;

            return Planar.Query.InternalPoint2D(closed2D, tolerance);
        }

        public static Planar.Point2D InternalPoint2D(this Face3D face3D)
        {
            if (face3D == null)
                return null;

            Planar.IClosed2D externalEdge = face3D.ExternalEdge;
            if (externalEdge == null)
                return null;

            List<Planar.IClosed2D> internalEdges = face3D.InternalEdges;
            if (internalEdges == null || internalEdges.Count == 0)
                return externalEdge.GetInternalPoint2D();

            Planar.Point2D result = externalEdge.GetCentroid();
            if (face3D.Inside(result))
                return result;

            if (externalEdge is Planar.ISegmentable2D)
            {
                List<Planar.Point2D> point2Ds = ((Planar.ISegmentable2D)externalEdge).GetPoints();
                if (point2Ds == null || point2Ds.Count == 0)
                    return null;

                foreach (Planar.IClosed2D closed2D in internalEdges)
                {
                    if (closed2D is Planar.ISegmentable2D)
                    {
                        List<Planar.Point2D> point2Ds_Internal = ((Planar.ISegmentable2D)closed2D).GetPoints();
                        if (point2Ds_Internal != null && point2Ds_Internal.Count > 0)
                            point2Ds.AddRange(point2Ds_Internal);
                    }
                }

                int count = point2Ds.Count;
                for (int i = 0; i < count - 2; i++)
                {
                    for (int j = 1; j < count - 1; j++)
                    {
                        Planar.Point2D point2D = Planar.Point2D.Mid(point2Ds[i], point2Ds[j]);
                        if (face3D.Inside(point2D))
                            return point2D;
                    }
                }
            }

            return null;
        }
    }
}