using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace SAM.Architectural.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Mesh ToGrasshopper_Mesh(this IOpening opening)
        {
            Mesh mesh = opening?.ToRhino_Mesh();
            if (mesh == null)
            {
                return null;
            }

            return new GH_Mesh(mesh);
        }

        public static GH_Mesh ToGrasshopper_Mesh(this IPartition partition, bool cutOpenings = true, bool includeOpenings = true, double tolerance = Core.Tolerance.Distance)
        {
            if(partition == null)
            {
                return null;
            }

            Mesh mesh = mesh = ((IHostPartition)partition).ToRhino_Mesh(cutOpenings, includeOpenings, tolerance);
            if (mesh == null)
            {
                return null;
            }

            return new GH_Mesh(mesh);
        }

        public static GH_Mesh ToGrasshopper_Mesh(this ArchitecturalModel architecturalModel, bool cutOpenings = true, bool includeOpenings = true, double tolerance = Core.Tolerance.Distance)
        {
            Mesh mesh = architecturalModel?.ToRhino_Mesh(cutOpenings, includeOpenings, tolerance);
            if (mesh == null)
            {
                return null;
            }

            return new GH_Mesh(mesh);
        }
    }
}