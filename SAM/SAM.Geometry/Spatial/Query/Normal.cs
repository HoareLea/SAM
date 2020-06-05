using System;
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

        public static Vector3D Normal(this Vector3D axisX, Vector3D axisY)
        {
            if (axisX == null || axisY == null)
                return null;

            return axisX.CrossProduct(axisY);
        }

        /// <summary>
        /// Calculates normal Vector3D for Point3Ds
        /// source: <see cref="http://www.ilikebigbits.com/2017_09_25_plane_from_points_2.html"/>
        /// </summary>
        /// <param name="point3Ds">List of points for plane</param>
        /// <returns>Normal Vector3D</returns>
        [Obsolete("This method is obsolete. Query.Normal([...]) instead.", false)]
        public static Vector3D Normal_Legacy(this IEnumerable<Point3D> point3Ds)
        {
            if (point3Ds == null || point3Ds.Count() < 3)
                return null;

            Point3D centroid = point3Ds.Average();

            double xx = 0;
            double xy = 0;
            double xz = 0;
            double yy = 0;
            double yz = 0;
            double zz = 0;

            foreach (Point3D point3D in point3Ds)
            {
                Point3D point3D_Temp = new Point3D(point3D.X - centroid.X, point3D.Y - centroid.Y, point3D.Z - centroid.Z);

                xx += point3D_Temp.X * point3D_Temp.X;
                xy += point3D_Temp.X * point3D_Temp.Y;
                xz += point3D_Temp.X * point3D_Temp.Z;
                yy += point3D_Temp.Y * point3D_Temp.Y;
                yz += point3D_Temp.Y * point3D_Temp.Z;
                zz += point3D_Temp.Z * point3D_Temp.Z;
            }

            int count = point3Ds.Count();

            xx /= count;
            xy /= count;
            xz /= count;
            yy /= count;
            yz /= count;
            zz /= count;

            Vector3D direction_Weighted = new Vector3D(0, 0, 0);

            Vector3D direction_Axis = null;
            double weight;

            double determinant_x = yy * zz - yz * yz;
            direction_Axis = new Vector3D(determinant_x, xz * yz - xy * zz, xy * yz - xz * yy);
            weight = determinant_x * determinant_x;
            if (direction_Weighted.DotProduct(direction_Axis) < 0)
                weight = -weight;

            direction_Weighted += direction_Axis * weight;

            double determinant_y = xx * zz - xz * xz;
            direction_Axis = new Vector3D(xz * yz - xy * zz, determinant_y, xy * xz - yz * xx);
            weight = determinant_y * determinant_y;
            if (direction_Weighted.DotProduct(direction_Axis) < 0)
                weight = -weight;

            direction_Weighted += direction_Axis * weight;

            double determinant_z = xx * yy - xy * xy;
            direction_Axis = new Vector3D(xy * yz - xz * yy, xy * xz - yz * xx, determinant_z);
            weight = determinant_z * determinant_z;
            if (direction_Weighted.DotProduct(direction_Axis) < 0)
                weight = -weight;

            direction_Weighted += direction_Axis * weight;

            if (!direction_Weighted.IsValid())
                return null;

            return direction_Weighted.Unit;
        }
    }
}