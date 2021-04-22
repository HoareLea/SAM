using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Mesh ToGrasshopper_Mesh(this Aperture aperture)
        {
            Mesh mesh = aperture?.ToRhino_Mesh();
            if (mesh == null)
            {
                return null;
            }

            return new GH_Mesh(mesh);
        }

        public static GH_Mesh ToGrasshopper_Mesh(this Panel panel, bool cutApertures = true, bool includeApertures = true, double tolerance = Core.Tolerance.MicroDistance)
        {
            Mesh mesh = panel?.ToRhino_Mesh(cutApertures, includeApertures, tolerance);
            if(mesh == null)
            {
                return null;
            }

            return new GH_Mesh(mesh);
        }

        public static GH_Mesh ToGrasshopper_Mesh(this AdjacencyCluster adjacencyCluster, bool cutApertures = false, bool includeApertures = true, double tolerance = Core.Tolerance.MicroDistance)
        {
            Mesh mesh = adjacencyCluster?.ToRhino_Mesh(cutApertures, includeApertures, tolerance);
            if (mesh == null)
            {
                return null;
            }

            return new GH_Mesh(mesh);
        }

        public static GH_Mesh ToGrasshopper_Mesh(this AnalyticalModel analyticalModel, bool cutApertures = false, bool includeApertures = true, double tolerance = Core.Tolerance.MicroDistance)
        {
            Mesh mesh = analyticalModel?.ToRhino_Mesh(cutApertures, includeApertures, tolerance);
            if (mesh == null)
            {
                return null;
            }

            return new GH_Mesh(mesh);
        }
    }
}