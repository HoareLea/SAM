using SAM.Geometry.Planar;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static Rectangle2D Rectangle2D(this Geometry.Spatial.Face3D face3D)
        {
            //TODO: Find better way to determine Height

            IClosed2D closed2D = face3D?.ExternalEdge2D;
            if (closed2D == null)
                return null;

            ISegmentable2D segmentable2D = closed2D as ISegmentable2D;
            if (segmentable2D == null)
                throw new System.NotImplementedException();

           return Geometry.Planar.Create.Rectangle2D(segmentable2D.GetPoints());
        }
    }
}