
using SAM.Geometry.Planar;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double Width(this PlanarBoundary3D planarBoundary3D)
        {
            //TODO: Find better way to determine Width

            IClosed2D closed2D = planarBoundary3D?.Edge2DLoop?.GetClosed2D();
            if (closed2D == null)
                return double.NaN;

            ISegmentable2D segmentable2D = closed2D as ISegmentable2D;

            if (segmentable2D == null)
                throw new System.NotImplementedException();

            Rectangle2D rectangle2D = Geometry.Planar.Create.Rectangle2D(segmentable2D.GetPoints());
            if (rectangle2D == null)
                return double.NaN;

            return rectangle2D.Width;
        }
    }
}