using Grasshopper.Kernel.Types;
using SAM.Geometry.Grasshopper;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Surface ToGrasshopper(this Panel panel)
        {
            return panel.PlanarBoundary3D.ToGrasshopper();
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