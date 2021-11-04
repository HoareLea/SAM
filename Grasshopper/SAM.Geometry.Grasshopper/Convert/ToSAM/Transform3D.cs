using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SAM.Math;

namespace SAM.Geometry.Grasshopper
{
    public static partial class Convert
    {
        public static Spatial.Transform3D ToSAM_Transform3D(this Rhino.Geometry.Matrix matrix)
        {
            Matrix4D matrix4D = Math.Grasshopper.Convert.ToSAM(matrix) as Matrix4D;
            if (matrix4D == null)
                return null;

            return new Spatial.Transform3D(matrix4D);
        }

        public static Spatial.Transform3D ToSAM_Transform3D(this GH_Matrix matrix)
        {
            return ToSAM_Transform3D(matrix.Value);
        }

        public static Spatial.Transform3D ToSAM(this Transform transform)
        {
            if (transform == null)
                return null;

            return ToSAM_Transform3D(new Rhino.Geometry.Matrix(transform));
        }

        public static Spatial.Transform3D ToSAM(this GH_Transform transform)
        {
            return ToSAM(transform.Value);
        }
    }
}