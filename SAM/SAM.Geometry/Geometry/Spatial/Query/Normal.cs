using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Vector3D Normal(this Plane plane, IEnumerable<Planar.Point2D> point2Ds)
        {
            if(plane == null || point2Ds == null)
            {
                return null;
            }

            int count = point2Ds.Count();

            if(count < 3)
            {
                return null;
            }

            for (int i = 0; i < count; i++)
            {
                int index_1 = i;
                int index_2 = Core.Query.Next(count, index_1);
                int index_3 = Core.Query.Next(count, index_2);

                Point3D point3D_1 = plane.Convert(point2Ds.ElementAt(index_1));
                Point3D point3D_2 = plane.Convert(point2Ds.ElementAt(index_2));
                Point3D point3D_3 = plane.Convert(point2Ds.ElementAt(index_3));

                Vector3D normal = Normal(point3D_1, point3D_2, point3D_3);
                if(normal == null || !normal.IsValid())
                {
                    continue;
                }

                return normal;
            }

            return null;
        }

        public static Vector3D Normal(this IEnumerable<Point3D> point3Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (point3Ds == null || point3Ds.Collinear(tolerance))
                return null;

            int count = point3Ds.Count();

            if (count < 3)
            {
                return null;
            }

            if(count == 3)
            {
                return Normal(point3Ds.ElementAt(0), point3Ds.ElementAt(1), point3Ds.ElementAt(2));
            }

            Point3D origin = point3Ds.Average();
            Vector3D normal = new Vector3D();
            if (point3Ds.Coplanar(tolerance))
            {
                for (int i = 0; i < count - 1; i++)
                {
                    normal += (point3Ds.ElementAt(i) - origin).CrossProduct(point3Ds.ElementAt(i + 1) - origin);
                }

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

            if(result == null)
            {
                return null;
            }

            result = result.Unit;

            Plane plane = new Plane(origin, result);

            bool invalid = false;
            foreach(Point3D point3D in point3Ds)
            {
                if(plane.Distance(point3D) > tolerance)
                {
                    invalid = true;
                    break;
                }
            }

            if(invalid)
            {
                normal = new Vector3D();
                for (int i = 0; i < count - 1; i++)
                {
                    normal += (point3Ds.ElementAt(i) - origin).CrossProduct(point3Ds.ElementAt(i + 1) - origin);
                }

                normal = normal.Unit;

                Plane plane_Temp = new Plane(origin, normal);

                double max = double.MinValue;
                double max_Temp = double.MinValue;
                foreach(Point3D point3D in point3Ds)
                {
                    double distance = double.NaN;

                    distance = plane.Distance(point3D);
                    if(distance > max)
                    {
                        max = distance;
                    }

                    distance = plane_Temp.Distance(point3D);
                    if (distance > max_Temp)
                    {
                        max_Temp = distance;
                    }
                }

                if(max_Temp < max)
                {
                    result = normal;
                }
            }

            return result;
        }

        public static Vector3D Normal(this Point3D point3D_1, Point3D point3D_2, Point3D point3D_3)
        {
            return new Vector3D(point3D_1, point3D_2).CrossProduct(new Vector3D(point3D_1, point3D_3));
        }

        public static Vector3D Normal(this Vector3D axisX, Vector3D axisY)
        {
            if (axisX == null || axisY == null)
                return null;

            return axisX.CrossProduct(axisY);
        }
    }
}