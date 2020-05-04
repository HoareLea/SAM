using Grasshopper.Kernel.Types;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Surface ToGrasshopper(this Surface surface, double tolerance = Core.Tolerance.Distance)
        {
            IClosed3D closed3D = surface?.GetExternalEdge();
            if (closed3D == null)
                return null;

            Rhino.Geometry.Brep brep = ToRhino_Brep(new IClosed3D[] { closed3D }, tolerance);
            if (brep == null)
                return null;

            return new GH_Surface(brep);
        }

        public static GH_Surface ToGrasshopper(this Face3D face, double tolerance = Core.Tolerance.Distance)
        {
            return new GH_Surface(ToRhino_Brep(face, tolerance));
        }
    }
}