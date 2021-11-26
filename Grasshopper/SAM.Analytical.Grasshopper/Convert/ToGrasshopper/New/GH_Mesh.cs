using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Mesh ToGrasshopper_Mesh(this IOpening opening)
        {
            Mesh mesh = Rhino.Convert.ToRhino_Mesh(opening);
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

            Mesh mesh = mesh = Rhino.Convert.ToRhino_Mesh((IHostPartition)partition, cutOpenings, includeOpenings, tolerance);
            if (mesh == null)
            {
                return null;
            }

            return new GH_Mesh(mesh);
        }

        public static GH_Mesh ToGrasshopper_Mesh(this BuildingModel buildingModel, bool cutOpenings = true, bool includeOpenings = true, double tolerance = Core.Tolerance.Distance)
        {
            Mesh mesh = Rhino.Convert.ToRhino_Mesh(buildingModel, cutOpenings, includeOpenings, tolerance);
            if (mesh == null)
            {
                return null;
            }

            return new GH_Mesh(mesh);
        }
    }
}