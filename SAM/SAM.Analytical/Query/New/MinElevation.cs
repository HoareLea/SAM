
namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double MinElevation(this ArchitecturalModel architecturalModel, Space space)
        {
            if(architecturalModel == null || space == null)
            {
                return double.NaN;
            }

            Geometry.Spatial.BoundingBox3D boundingBox3D = architecturalModel.GetShell(space)?.GetBoundingBox();
            if(boundingBox3D == null)
            {
                return double.NaN;
            }

            return boundingBox3D.Min.Z;
        }
    }
}