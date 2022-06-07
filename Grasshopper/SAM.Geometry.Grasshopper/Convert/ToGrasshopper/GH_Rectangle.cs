using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Rectangle ToGrasshopper(this Spatial.Rectangle3D rectangle3D)
        {
            if(rectangle3D == null)
            {
                return null;
            }

            return new GH_Rectangle(Rhino.Convert.ToRhino(rectangle3D));
        }
    }
}