using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Vector3D Normal(this IEnumerable<Point3D> point3Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (point3Ds == null || point3Ds.Collinear(tolerance))
                return null;

            int count = point3Ds.Count();

            if (count < 3)
                return null;

            Point3D origin = point3Ds.Average();
            Vector3D normal = new Vector3D();
            if (point3Ds.Coplanar(tolerance))
            {
                for (int i = 0; i < count - 1; i++)
                    normal += (point3Ds.ElementAt(i) - origin).CrossProduct(point3Ds.ElementAt(i + 1) - origin);
                return normal.Unit;
            }

            Math.Matrix matrix = new Math.Matrix(3, 3);
            double[,] normalizedPoints = new double[count, 3];

            for (int i = 0; i < count; i++)
            {
                normalizedPoints[i, 0] = point3Ds.ElementAt(i).X - origin.X;
                normalizedPoints[i, 1] = point3Ds.ElementAt(i).Y - origin.Y;
                normalizedPoints[i, 2] = point3Ds.ElementAt(i).Z - origin.Z;
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    double value = 0;
                    for (int k = 0; k < count; k++)
                    {
                        value += normalizedPoints[k, i] * normalizedPoints[k, j];
                    }
                    matrix[i, j] = value;
                }
            }

            Vector3D[] eigenvectors = Eigenvectors(matrix, tolerance);
            if (eigenvectors == null)
                return null;

            Vector3D result = null;
            double leastSquares = double.PositiveInfinity;
            foreach (Vector3D eigenvector in eigenvectors)
            {
                double squares = 0;
                for (int i = 0; i < count; i++)
                {
                    squares += System.Math.Pow(eigenvector.X * normalizedPoints[i, 0] + eigenvector.Y * normalizedPoints[i, 1] + eigenvector.Z * normalizedPoints[i, 2], 2);
                }

                if (squares <= leastSquares)
                {
                    leastSquares = squares;
                    result = eigenvector;
                }
            }

            if (result != null)
                result = result.Unit;

            return result;
        }

        public static Vector3D Normal(this Point3D point3D_1, Point3D point3D_2, Point3D point3D_3)
        {
            return new Vector3D(point3D_1, point3D_2).CrossProduct(new Vector3D(point3D_1, point3D_3));
        }
    }
}