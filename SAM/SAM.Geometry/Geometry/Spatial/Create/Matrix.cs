using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static Math.Matrix Matrix(this IEnumerable<Vector3D> vector3Ds)
        {
            if (vector3Ds == null)
                return null;

            int count = vector3Ds.Count();

            Math.Matrix matrix = new Math.Matrix(count, 3);
            for (int i = 0; i < count; i++)
            {
                Vector3D vector3D = vector3Ds.ElementAt(i);
                matrix[i, 0] = vector3D[0];
                matrix[i, 1] = vector3D[1];
                matrix[i, 2] = vector3D[2];
            }

            return matrix;
        }

        public static Math.Matrix Matrix(this IEnumerable<Point3D> point3Ds)
        {
            if (point3Ds == null)
                return null;

            int count = point3Ds.Count();

            Math.Matrix matrix = new Math.Matrix(count, 3);
            for (int i = 0; i < count - 1; i++)
            {
                Point3D point3D = point3Ds.ElementAt(i);
                matrix[i, 0] = point3D[0];
                matrix[i, 1] = point3D[1];
                matrix[i, 2] = point3D[2];
            }

            return matrix;
        }
    }
}