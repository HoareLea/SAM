using Rhino.Geometry;
using SAM.Math;

namespace SAM.Geometry.Rhino
{
    public static partial class Convert
    {
        public static Spatial.Transform3D ToSAM_Transform3D(this global::Rhino.Geometry.Matrix matrix)
        {
            Matrix4D matrix4D = Math.Rhino.Convert.ToSAM(matrix) as Matrix4D;
            if (matrix4D == null)
                return null;

            return new Spatial.Transform3D(matrix4D);
        }

        public static Spatial.Transform3D ToSAM(this Transform transform)
        {
            if (transform == null)
                return null;

            return ToSAM_Transform3D(new global::Rhino.Geometry.Matrix(transform));
        }
    }
}