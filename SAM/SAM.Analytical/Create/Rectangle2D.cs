using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static Rectangle2D Rectangle2D(this PlanarBoundary3D planarBoundary3D)
        {
            //TODO: Find better way to determine Height

            IClosed2D closed2D = planarBoundary3D?.Edge2DLoop?.GetClosed2D();
            if (closed2D == null)
                return null;

            ISegmentable2D segmentable2D = closed2D as ISegmentable2D;
            if (segmentable2D == null)
                throw new System.NotImplementedException();

           return Geometry.Planar.Create.Rectangle2D(segmentable2D.GetPoints());
        }
    }
}