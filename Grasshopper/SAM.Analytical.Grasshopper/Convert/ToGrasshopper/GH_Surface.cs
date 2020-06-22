using Grasshopper.Kernel.Types;
using SAM.Geometry.Grasshopper;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Surface ToGrasshopper(this Panel panel, bool includeInternalEdges, double tolerance = Core.Tolerance.Distance)
        {
            return panel.PlanarBoundary3D.ToGrasshopper(includeInternalEdges, tolerance);
        }

        public static GH_Surface ToGrasshopper(this PlanarBoundary3D planarBoundary3D, bool includeInternalEdges, double tolerance = Core.Tolerance.Distance)
        {
            return new GH_Surface(planarBoundary3D.ToRhino(includeInternalEdges, tolerance));
        }

        public static GH_Surface ToGrasshopper(this Aperture aperture, bool includeInternalEdges, double tolerance = Core.Tolerance.Distance)
        {
            return new GH_Surface(aperture.GetFace3D().ToRhino_Brep(includeInternalEdges, tolerance));
        }
    }
}