using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double Width(this PlanarBoundary3D planarBoundary3D)
        {
            //TODO: Find better way to determine Width

            Rectangle2D rectangle2D = Create.Rectangle2D(planarBoundary3D);
            if (rectangle2D == null)
                return double.NaN;

            Plane plane = Plane.WorldXY;

            if (plane.Coplanar(planarBoundary3D.Plane))
                return rectangle2D.Width;

            Vector3D vector3D_HeightDirection = plane.Convert(rectangle2D.HeightDirection);

            double angle_AxisX = System.Math.Min(plane.AxisX.SmallestAngle(vector3D_HeightDirection), plane.AxisX.GetNegated().SmallestAngle(vector3D_HeightDirection));
            double angle_AxisY = System.Math.Min(plane.AxisY.SmallestAngle(vector3D_HeightDirection), plane.AxisY.GetNegated().SmallestAngle(vector3D_HeightDirection));

            double result = rectangle2D.Width;
            if (angle_AxisY > angle_AxisX)
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