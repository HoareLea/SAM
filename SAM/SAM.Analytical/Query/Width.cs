
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;

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

            Plane plane = Plane.WorldXY;

            Vector3D vector3D_WidthDirection = plane.Convert(rectangle2D.WidthDirection);
            Vector3D vector3D_HeightDirection = plane.Convert(rectangle2D.HeightDirection);

            Vector3D axisX = plane.AxisX;

            double result = rectangle2D.Width;
            if (axisX.SmallestAngle(vector3D_HeightDirection) < axisX.SmallestAngle(vector3D_WidthDirection))
                result = rectangle2D.Height;

            return result;

            //Method 2
            //IClosedPlanar3D closedPlanar3D = Plane.WorldXY.Convert(closed2D);
            //if (closedPlanar3D == null)
            //    return double.NaN;

            //BoundingBox3D boundingBox3D = closedPlanar3D.GetBoundingBox();
            //if (boundingBox3D == null)
            //    return double.NaN;

            //return boundingBox3D.Max.X - boundingBox3D.Min.X;


            //Method 1
            //ISegmentable2D segmentable2D = closed2D as ISegmentable2D;

            //if (segmentable2D == null)
            //    throw new System.NotImplementedException();

            //Rectangle2D rectangle2D = Geometry.Planar.Create.Rectangle2D(segmentable2D.GetPoints());
            //if (rectangle2D == null)
            //    return double.NaN;

            //return rectangle2D.Width;
        }
    }
}