using SAM.Geometry.Planar;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Polygon3D Simplify(this Circle3D circle3D, int density)
        {
            Plane plane = circle3D?.GetPlane();
            if(plane == null)
            {
                return null;
            }

            Circle2D circle2D = new Circle2D(plane.Convert(circle3D.Center), circle3D.Radious);

            Polygon2D polygon2Ds = circle2D.Simplify(density);

            return plane.Convert(polygon2Ds);
        }
    }
}