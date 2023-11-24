using Grasshopper.Kernel.Types;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Surface ToGrasshopper(this PlanarBoundary3D planarBoundary3D)
        {
            return new GH_Surface(Rhino.Convert.ToRhino(planarBoundary3D));
        }
    }
}