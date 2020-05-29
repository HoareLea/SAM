
namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double Width(this PlanarBoundary3D planarBoundary3D)
        {
            //TODO: Find better way to determine Width

            Geometry.Planar.IClosed2D closed2D = planarBoundary3D?.Edge2DLoop?.GetClosed2D();
            if (closed2D == null)
                return double.NaN;

            //Geometry.Spatial.Vector3D vector3D_X = Geometry.Spatial.Vector3D.

            return closed2D.GetBoundingBox().Height;
        }
    }
}