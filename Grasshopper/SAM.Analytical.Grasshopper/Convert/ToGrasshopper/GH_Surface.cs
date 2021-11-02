using Grasshopper.Kernel.Types;
using SAM.Geometry.Grasshopper;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Surface ToGrasshopper(this Panel panel, bool cutApertures = false, double tolerance = Core.Tolerance.MicroDistance)
        {
            return panel.GetFace3D(cutApertures).ToGrasshopper(tolerance);
        }

        public static GH_Surface ToGrasshopper(this PlanarBoundary3D planarBoundary3D)
        {
            return new GH_Surface(planarBoundary3D.ToRhino());
        }

        public static GH_Surface ToGrasshopper(this Aperture aperture)
        {
            return new GH_Surface(aperture.GetFace3D().ToRhino_Brep());
        }
    }
}