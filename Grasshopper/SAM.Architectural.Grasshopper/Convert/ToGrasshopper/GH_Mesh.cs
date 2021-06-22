using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace SAM.Architectural.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Mesh ToGrasshopper_Mesh(this Opening opening)
        {
            Mesh mesh = opening?.ToRhino_Mesh();
            if (mesh == null)
            {
                return null;
            }

            return new GH_Mesh(mesh);
        }

        public static GH_Mesh ToGrasshopper_Mesh(this HostBuildingElement hostBuildingElement, bool cutOpenings = true, bool includeOpenings = true, double tolerance = Core.Tolerance.Distance)
        {
            Mesh mesh = hostBuildingElement?.ToRhino_Mesh(cutOpenings, includeOpenings, tolerance);
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