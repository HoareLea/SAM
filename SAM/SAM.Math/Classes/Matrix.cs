using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Math
{
    public class Matrix : Core.IJSAMObject
    {
        private double[,] values;

        public Matrix(int rowCount, int columnCount)
        {
            values = new double[rowCount, columnCount];
        }

        public Matrix(double[] values)
        {
            if (values == null)
                this.values = default;

            this.values = new double[1, values.Length];
            for (int i = 0; i < values.Length; i++)
                this.values[0, i] = values[i];
        }

        public Matrix(double[,] values)
        {
            this.values = values;
        }

        public Matrix(JObject jObject)
        {
            values = default;
            FromJObject(jObject);
        }

        public double this[int row, int column]
        {
            get
            {
                if (values == null)
                    return double.NaN;

                return values[row, column];
            }
            set
            {
                if (values == null)
                    return;

                values[row, column] = value;
            }
        }

        public Matrix Clone()
        {
            return new Matrix((double[,])values.Clone());
        }

        public int RowCount()
        {
            if (values == null)
                return -1;

            return values.GetLength(0);
        }

        public int ColumnCount()
        {
            if (values == null)
                return -1;

            return values.GetLength(1);
        }

        public double REFTolerance(double tolerance = Core.Tolerance.Distance)
        {
            // Inspired by BHoM

            int length_1 = values.GetLength(0);
            int length_2 = values.GetLength(1);
            double maxRowSum = 0;

            for (int i = 0; i < length_1; i++)
            {
                double rowSum = 0;
                for (int j = 0; j < length_2; j++)
                    rowSum += System.Math.Abs(values[i, j]);

                maxRowSum = System.Math.Max(maxRowSum, rowSum);
            }

            double result = tolerance * System.Math.Max(length_1, length_2) * maxRowSum;
            if (result >= 1)
                result = 1 - tolerance;

            return result;
        }

        public Matrix RowEchelonForm(bool reduced = true, double tolerance = Core.Tolerance.Distance)
        {
            // Inspired by BHoM Strongly inspired by https://rosettacode.org/wiki/Reduced_row_echelon_form

            Matrix matrix = this.Clone();
            int lead = 0;
            int rowCount = matrix.RowCount();
            int columnCount = matrix.ColumnCount();

            for (int r = 0; r < rowCount; r++)
            {
                if (columnCount == lead)
                    break;

                int i = r;
                while (System.Math.Abs(matrix[i, lead]) < tolerance)
                {
                    i++;
                    if (i == rowCount)
                    {
                        i = r;
                        lead++;
                        if (columnCount == lead)
                        {
                            lead--;
                            break;
                        }
                    }
                }

                for (int j = 0; j < columnCount; j++)
                {
                    double temp = matrix[r, j];
                    matrix[r, j] = matrix[i, j];
                    matrix[i, j] = temp;
                }

                double div = matrix[r, lead];
                if (System.Math.Abs(div) >= tolerance)
                    for (int j = 0; j < columnCount; j++) matrix[r, j] /= div;

                int w = reduced ? 0 : r + 1;
                for (; w < rowCount; w++)
                {
                    if (w != r)
                    {
                        double sub = matrix[w, lead];
                        for (int k = 0; k < columnCount; k++)
                        {
                            matrix[w, k] -= (sub * matrix[r, k]);
                        }
                    }
                }

                lead++;
            }

            return matrix;
        }

        public int CountNonZeroRows(double tolerance = Core.Tolerance.Distance)
        {
            // Inspired by BHoM

            int rowCount = RowCount();
            int columnCount = ColumnCount();
            int count = 0;

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    if (System.Math.Abs(this[i, j]) >= tolerance)
                    {
                        count++;
                        break;
                    }
                }
            }

            return count;
        }

        public double[] Eigenvalues(double tolerance = Core.Tolerance.Distance)
        {
            // Inspired by BHoM

            int rowCount = RowCount();
            int columnCount = ColumnCount();

            if (rowCount != 3 || columnCount != 3)
                throw new NotImplementedException();

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (System.Math.Abs(this[i, j] - this[j, i]) > tolerance)
                        throw new NotImplementedException();
                }
            }

            double a = this[0, 0];
            double b = this[0, 1];
            double c = this[0, 2];
            double d = this[1, 1];
            double e = this[1, 2];
            double f = this[2, 2];

            double A = 1;
            double B = -(a + d + f);
            double C = a * d + a * f + d * f - b * b - c * c - e * e;
            double D = -(a * d * f + 2 * b * c * e - a * e * e - d * c * c - f * b * b);

            return Query.RealCubicRoots_ThreeRootsOnly(A, B, C, D, tolerance);
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            throw new Exception();

            JObject jObject_Values = jObject.Value<JObject>("values");

            JArray jArray = jObject.Value<JArray>("values");
            foreach (JArray jArray_Row in jArray)
            {
            }
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            throw new Exception();

            return jObject;
        }

        //// Solve Ax^3 + Bx^2 + Cx + D = 0 following http://www.code-kings.com/2013/11/cubic-equation-roots-in-csharp-code.html
        //private static double[] RealCubicRoots(double A, double B, double C, double D, double tolerance = Core.Tolerance.Distance)
        //{
        //    double f = (3 * C / A - B * B / (A * A)) / 3;
        //    double g = (2 * System.Math.Pow(B, 3) / System.Math.Pow(A, 3) - (9 * B * C) / System.Math.Pow(A, 2) + 27 * D / A) / 27;
        //    double h = Core.Modify.Round(System.Math.Pow(g, 2) * 0.25 + System.Math.Pow(f, 3) / 27, tolerance);

        //    if (h <= 0)
        //    {
        //        double i = System.Math.Pow(System.Math.Pow(g, 2) * 0.25 - h, 0.5);
        //        double j = System.Math.Pow(i, 0.333333333333333333333333);
        //        double k = System.Math.Acos(-g / (2 * i));
        //        double l = -j;
        //        double m = System.Math.Cos(k / 3);
        //        double n = System.Math.Pow(3, 0.5) * System.Math.Sin(k / 3);
        //        double p = -B / (3 * A);
        //        double x = 2 * j * System.Math.Cos(k / 3) - B / (3 * A);
        //        double y = l * (m + n) + p;
        //        double z = l * (m - n) + p;
        //        return new double[] { x, y, z };
        //    }
        //    else
        //        return null;
        //}

        public static Matrix GetIdentity(int count = 3)
        {
            Matrix matrix = new Matrix(new double[count, count]);
            for (int i = 0; i < 3; i++)
                matrix[i, i] = 1;

            return matrix;
        }

        public static Matrix GetScale(IEnumerable<double> values)
        {
            if (values == null)
                return null;

            int count = values.Count();

            Matrix matrix = GetIdentity(count + 1);

            for (int i = 0; i < count; i++)
                matrix[i, i] = values.ElementAt(i);

            return matrix;
        }

        public static Matrix GetScale(int count, double factor)
        {
            Matrix matrix = GetIdentity(count + 1);

            for (int i = 0; i < count; i++)
                matrix[i, i] = factor;

            return matrix;
        }

        public static Matrix operator *(Matrix matrix_1, Matrix matrix_2)
        {
            if (matrix_1 == null || matrix_2 == null)
                return null;

            int columnCount_1 = matrix_1.ColumnCount();

            if (columnCount_1 != matrix_2.RowCount())
                return null;

            int rowCount_1 = matrix_1.RowCount();

            int columnCount_2 = matrix_2.ColumnCount();

            Matrix result = new Matrix(rowCount_1, columnCount_1);
            for (int i = 0; i < rowCount_1; i++)
                for (int j = 0; j < columnCount_2; j++)
                {
                    result[i, j] = 0;
                    for (int k = 0; k < columnCount_1; k++)
                        result[i, j] += matrix_1[i, k] * matrix_2[k, j];
                }

            return result;
        }
    }
}