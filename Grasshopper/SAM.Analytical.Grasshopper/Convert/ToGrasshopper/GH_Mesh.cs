using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Mesh ToGrasshopper_Mesh(this Aperture aperture)
        {
            Mesh mesh = Rhino.Convert.ToRhino_Mesh(aperture);
            if (mesh == null)
            {
                return null;
            }

            return new GH_Mesh(mesh);
        }

        public static GH_Mesh ToGrasshopper_Mesh(this Panel panel, bool cutApertures = true, bool includeApertures = true, double tolerance = Core.Tolerance.Distance)
        {
            Mesh mesh = Rhino.Convert.ToRhino_Mesh(panel, cutApertures, includeApertures, tolerance);
            if(mesh == null)
            {
                return null;
            }

            return new GH_Mesh(mesh);
        }

        public static GH_Mesh ToGrasshopper_Mesh(this AdjacencyCluster adjacencyCluster, bool cutApertures = true, bool includeApertures = true, double tolerance = Core.Tolerance.Distance)
        {
            Mesh mesh = Rhino.Convert.ToRhino_Mesh(adjacencyCluster, cutApertures, includeApertures, tolerance);
            if (mesh == null)
            {
                return null;
            }

            return new GH_Mesh(mesh);
        }

        public static GH_Mesh ToGrasshopper_Mesh(this AnalyticalModel analyticalModel, bool cutApertures = true, bool includeApertures = true, double tolerance = Core.Tolerance.Distance)
        {
            Mesh mesh = Rhino.Convert.ToRhino_Mesh(analyticalModel, cutApertures, includeApertures, tolerance);
            if (mesh == null)
            {
                return null;
            }

            return new GH_Mesh(mesh);
        }
    }
}