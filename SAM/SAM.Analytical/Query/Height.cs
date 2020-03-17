using System;


namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double Height(this PlanarBoundary3D planarBoundary3D)
        {
            Geometry.Planar.IClosed2D closed2D = planarBoundary3D?.Edge2DLoop?.GetClosed2D();
            if (closed2D == null)
                return double.NaN;


            return closed2D.GetBoundingBox().Height;
        }
    }
}
