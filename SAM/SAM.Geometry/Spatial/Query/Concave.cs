namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool Concave<T>(this T closedPlanar3D) where T: IClosedPlanar3D, ISegmentable3D
        {
            if(closedPlanar3D == null)
            {
                return false;
            }

            Plane plane = closedPlanar3D.GetPlane();
            if(plane == null)
            {
                return false;
            }


            Planar.ISegmentable2D segmentable2D = plane.Convert(closedPlanar3D) as Planar.ISegmentable2D;
            if(segmentable2D == null)
            {
                throw new System.NotImplementedException();
            }

            return Planar.Query.Concave(segmentable2D.GetPoints());
        }

        public static bool Concave(this Face3D face3D, bool externalEdge = true, bool internalEdges = true)
        {
            Plane plane = face3D?.GetPlane();
            if(plane == null)
            {
                return false;
            }

            return Planar.Query.Concave(plane.Convert(face3D), externalEdge, internalEdges);

        }

        public static bool Concave(this IFace3DObject face3Dobject, bool externalEdge = true, bool internalEdges = true)
        {
            return Concave(face3Dobject?.Face3D, externalEdge, internalEdges);
        }
    }
}