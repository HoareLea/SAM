using Grasshopper.Kernel.Types;
using SAM.Geometry.Spatial;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Surface ToGrasshopper(this Surface surface, double tolerance = Core.Tolerance.Distance)
        {
            IClosed3D closed3D = surface?.ExternalEdge3D;
            if (closed3D == null)
                return null;

            //global::Rhino.Geometry.Brep brep = Rhino.Convert.ToRhino_Brep(new IClosed3D[] { closed3D }, tolerance);
            global::Rhino.Geometry.Brep brep = Rhino.Convert.ToRhino_Brep(closed3D as IClosedPlanar3D, null, tolerance);
            if (brep == null)
                return null;

            return new GH_Surface(brep);
        }

        public static GH_Surface ToGrasshopper(this Face3D face3D, double tolerance = Core.Tolerance.Distance)
        {
            return new GH_Surface(Rhino.Convert.ToRhino_Brep(face3D, tolerance));
        }
    }
}