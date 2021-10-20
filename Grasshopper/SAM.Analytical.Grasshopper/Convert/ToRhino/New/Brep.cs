using SAM.Geometry.Spatial;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Convert
    {
        public static Rhino.Geometry.Brep ToRhino(this IPartition partition, bool cutOpenings = false, double tolerance = Core.Tolerance.MicroDistance)
        {
            if(partition == null)
            {
                return null;
            }

            Face3D face3D = null;
            if (partition is IHostPartition)
            {
                face3D = ((IHostPartition)partition).Face3D(cutOpenings, tolerance);
            }
            else
            {
                face3D = partition.Face3D;
            }

            return Geometry.Grasshopper.Convert.ToRhino_Brep(face3D);
        }
    }
}