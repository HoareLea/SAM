namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double MaxElevation(this Panel panel)
        {
            Geometry.Spatial.BoundingBox3D boundingBox3D = panel?.PlanarBoundary3D?.GetBoundingBox();
            if (boundingBox3D == null)
                return double.NaN;

            return boundingBox3D.Max.Z;
        }
    }
}