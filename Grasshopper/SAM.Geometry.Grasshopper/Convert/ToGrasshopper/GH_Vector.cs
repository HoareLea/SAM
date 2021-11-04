using Grasshopper.Kernel.Types;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static GH_Vector ToGrasshopper(this Spatial.Vector3D vector3D)
        {
            return new GH_Vector(Rhino.Convert.ToRhino(vector3D));
        }

        public static GH_Vector ToGrasshopper(this Planar.Vector2D vector2D)
        {
            return new GH_Vector(Rhino.Convert.ToRhino(vector2D));
        }
    }
}