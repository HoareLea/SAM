using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool Coplanar(this IEnumerable<Point3D> point3Ds, double tolerance = Core.Tolerance.Distance)
        {
            // Inspired by BHoM

            if (point3Ds == null)
                return false;

            int count = point3Ds.Count();

            if (count < 4)
                return true;

            List<Vector3D> vector3Ds = new List<Vector3D>();
            for (int i = 0; i < count - 1; i++)
                vector3Ds.Add(new Vector3D(point3Ds.ElementAt(0), point3Ds.ElementAt(i + 1)));

            Math.Matrix matrix = Create.Matrix(vector3Ds);

            double tolerance_REF = matrix.REFTolerance(tolerance);
            Math.Matrix matrix_REF = matrix.RowEchelonForm(true, tolerance_REF);
            int nonZeroRows = matrix_REF.CountNonZeroRows(tolerance_REF);

            return nonZeroRows < 3;
        }
    }
}