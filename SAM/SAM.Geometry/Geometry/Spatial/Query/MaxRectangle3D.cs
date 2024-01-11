namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Rectangle3D MaxRectangle3D(this IClosedPlanar3D closedPlanar3D)
        {
            if(closedPlanar3D == null)
            {
                return null;
            }

            IClosedPlanar3D closedPlanar3D_Temp = closedPlanar3D;
            if(closedPlanar3D_Temp is Face3D)
            {
                closedPlanar3D_Temp = ((Face3D)closedPlanar3D_Temp).GetExternalEdge3D();
                if(closedPlanar3D_Temp == null)
                {
                    return null;
                }
            }

            Plane plane = closedPlanar3D_Temp.GetPlane();
            if(plane == null)
            {
                return null;
            }

            Planar.ISegmentable2D segmentable2D = plane.Convert(closedPlanar3D_Temp) as Planar.ISegmentable2D;
            if(segmentable2D == null)
            {
                throw new System.NotImplementedException();
            }

            Planar.Rectangle2D rectangle2D = Planar.Create.Rectangle2D(segmentable2D.GetPoints());

            return plane.Convert(rectangle2D);
        }
    }
}