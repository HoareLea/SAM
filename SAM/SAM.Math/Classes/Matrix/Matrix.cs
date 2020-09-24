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
            int count_Rows = values.GetLength(0);
            int count_Columns = values.GetLength(1);

            this.values = new double[count_Rows, count_Columns];
            for (int i = 0; i < count_Rows; i++)
                for (int j = 0; j < count_Columns; j++)
                    this.values[i, j] = values[i, j];
        }

        public Matrix(JObject jObject)
        {
            values = default;
            FromJObject(jObject);
        }

        public Matrix(Matrix matrix)
        {
            if (matrix == null)
                return;

            int count_Rows = matrix.values.GetLength(0);
            int count_Columns = matrix.values.GetLength(1);

            values = new double[count_Rows, count_Columns];
            for (int i = 0; i < count_Rows; i++)
                for (int j = 0; j < count_Columns; j++)
                    values[i, j] = matrix.values[i, j];
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

        public void SetValues(double value)
        {
            int count_Rows = values.GetLength(0);
            int count_Columns = values.GetLength(1);

            for (int i = 0; i < count_Rows; i++)
                for (int j = 0; j < count_Columns; j++)
                    values[i, j] = value;
        }

        public virtual Matrix Clone()
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

        public double[,] ToArray()
        {
            int count_Rows = values.GetLength(0);
            int count_Columns = values.GetLength(1);

            double[,] result = new double[count_Rows, count_Columns];
            for (int i = 0; i < count_Rows; i++)
                for (int j = 0; j < count_Columns; j++)
                    result[i, j] = values[i, j];

            return result;
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

            JArray jArray_Column = jObject.Value<JArray>("Values");

            List<List<double>> valuesList = new List<List<double>>();
            foreach (JArray jArray_Row in jArray_Column)
            {
                List<double> values = new List<double>();
                foreach(double value in jArray_Row)
                    values.Add(value);

                valuesList.Add(values);
            }

            values = new double[valuesList.Count, valuesList.ConvertAll(x => x.Count).Max()];
            for (int i = 0; i < valuesList.Count; i++)
                for (int j = 0; j < valuesList[i].Count; j++)
                    values[i, j] = valuesList[i][j];

            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            JArray jArray_Column = new JArray();

            for (int i=0; i < values.GetLength(0); i++)
            {
                JArray jArray_Row = new JArray();
                for (int j = 0; j < values.GetLength(1); j++)
                    jArray_Row.Add(values[i, j]);

                jArray_Column.Add(jArray_Row);
            }

            jObject.Add("Values", jArray_Column);

            return jObject;
        }

        public override int GetHashCode()
        {
            int result = int.MinValue;
            for(int i=0; i < values.GetLength(0); i++)
            {
                for (int j = 0; j < values.GetLength(1); j++)
                {
                    if(result == int.MinValue)
                        result = values[i, j].GetHashCode();
                    else
                        result = result ^ values[i, j].GetHashCode();
                }
            }

            return result;
        }

        public void Round(double tolerance = Core.Tolerance.Distance)
        {
            int count_Rows = values.GetLength(0);
            int count_Columns = values.GetLength(1);

            for (int i = 0; i < count_Rows; i++)
                for (int j = 0; j < count_Columns; j++)
                    values[i, j] = Core.Query.Round(values[i, j], tolerance);
        }

        public void Transpose()
        {
            int count_Rows = values.GetLength(0);
            int count_Columns = values.GetLength(1);

            double[,] values_Temp = new double[count_Columns, count_Rows];

            for (int i = 0; i < count_Rows; i++)
                for (int j = 0; j < count_Columns; j++)
                    values_Temp[j, i] = values[i, j];

            values = values_Temp;
        }

        public Matrix GetTransposed()
        {
            Matrix result = Clone();
            result.Transpose();
            return result;
        }

        public void Inverse()
        {
            if (!IsSquare())
                return;

            double[,] values_Temp = this.ToMathNet().Inverse()?.ToArray();
            if (values_Temp == null)
                return;

            values = values_Temp;
        }

        public void Negate()
        {
            int count_Rows = values.GetLength(0);
            int count_Columns = values.GetLength(1);

            for (int i = 0; i < count_Rows; i++)
                for (int j = 0; j < count_Columns; j++)
                    values[i, j] = -values[i, j];
        }

        public Matrix GetInversed()
        {
            if (!IsSquare())
                return null;

            Matrix result = Clone();
            result.Inverse();
            return result;
        }

        public Matrix GetMinorMatrix(int row, int column)
        {
            int count_Rows = values.GetLength(0);
            int count_Columns = values.GetLength(1);
            if (count_Columns <= 1 || count_Rows <= 1)
                return null;

            double[,] values_Temp = new double[count_Columns - 1, count_Rows - 1];
            for (int i = 0; i < count_Rows; i++)
            {
                if (i == row)
                    continue;

                int index_row = i;
                if (index_row > row)
                    index_row--;

                for (int j = 0; j < count_Columns; j++)
                {
                    if (j == column)
                        continue;

                    int index_column = j;
                    if (index_column > column)
                        index_column--;

                    values_Temp[index_column, index_row] = values[i, j];
                }
                    
            }

            return new Matrix(values_Temp);
        }

        public Matrix GetMinorsMatrix()
        {
            int count_Rows = values.GetLength(0);
            int count_Columns = values.GetLength(1);

            double[,] values_Temp = new double[count_Rows, count_Columns];
            for (int i = 0; i < count_Rows; i++)
                for (int j = 0; j < count_Columns; j++)
                    values_Temp[i, j] = GetMinorMatrix(i, j).Determinant();

            return new Matrix(values_Temp);
        }

        public Matrix GetCofactorsMatrix()
        {
            int count_Rows = values.GetLength(0);
            int count_Columns = values.GetLength(1);

            double[,] values_Temp = new double[count_Rows, count_Columns];
            for (int i = 0; i < count_Rows; i++)
                for (int j = 0; j < count_Columns; j++)
                    values_Temp[i, j] = GetCofactor(i, j);

            return new Matrix(values_Temp);
        }

        public double Determinant()
        {
            if (!IsSquare())
                return double.NaN;

            int count = values.GetLength(0);
            
            if (count == 1)
                return values[0, 0];

            if(count == 2)
                return (values[0, 0] * values[1, 1]) - (values[1, 0] * values[0, 1]);

            if (count == 3)
                return 
                    + (values[0, 0] * ((values[1, 1] * values[2, 2]) - (values[1, 2] * values[2, 1])))
                    - (values[0, 1] * ((values[1, 0] * values[2, 2]) - (values[1, 2] * values[2, 0])))
                    + (values[0, 2] * ((values[1, 0] * values[2, 1]) - (values[1, 1] * values[2, 0])));

            MathNet.Numerics.LinearAlgebra.Matrix<double> matrix = Convert.ToMathNet(this);
            return matrix.Determinant();
        }

        public bool IsSquare()
        {
            return values.GetLength(0) == values.GetLength(1);
        }

        public bool SizeEqual(Matrix matrix)
        {
            return values.GetLength(0) == matrix.values.GetLength(0) && values.GetLength(1) == matrix.values.GetLength(1);
        }

        public Matrix Multiply(Matrix matrix)
        {
            return this * matrix;
        }

        public Matrix Multiply(double value)
        {
            return this * value;
        }

        public Matrix Size()
        {
            return new Matrix(new double[] { values.GetLength(0), values.GetLength(1) });
        }

        public static double GetCofactor(int row, int column)
        {
            if ((row + column) % 2 == 1)
                return -1;
            else
                return 1;
        }

        public static Matrix GetIdentity(int count = 3)
        {
            Matrix matrix = new Matrix(new double[count, count]);
            for (int i = 0; i < count; i++)
                matrix[i, i] = 1;

            return matrix;
        }

        public static Matrix GetUnset(int rowCount, int columnCount)
        {
            Matrix result = new Matrix(rowCount, columnCount);
            result.SetValues(double.MinValue);

            return result;
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

            Matrix result = new Matrix(rowCount_1, columnCount_2);
            for (int i = 0; i < rowCount_1; i++)
                for (int j = 0; j < columnCount_2; j++)
                {
                    result[i, j] = 0;
                    for (int k = 0; k < columnCount_1; k++)
                        result[i, j] += matrix_1[i, k] * matrix_2[k, j];
                }

            return result;
        }

        public static Matrix operator *(Matrix matrix, double value)
        {
            int count_Rows = matrix.values.GetLength(0);
            int count_Columns = matrix.values.GetLength(1);

            double[,] values_Temp = new double[count_Columns, count_Rows];

            for (int i = 0; i < count_Rows; i++)
                for (int j = 0; j < count_Columns; j++)
                    values_Temp[i, j] = matrix.values[i, j] * value;

            return new Matrix(values_Temp);
        }

        public static Matrix operator +(Matrix matrix, double value)
        {
            int count_Rows = matrix.values.GetLength(0);
            int count_Columns = matrix.values.GetLength(1);

            double[,] values_Temp = new double[count_Columns, count_Rows];

            for (int i = 0; i < count_Rows; i++)
                for (int j = 0; j < count_Columns; j++)
                    values_Temp[i, j] = matrix.values[i, j] + value;

            return new Matrix(values_Temp);
        }

        public static Matrix operator +(Matrix matrix_1, Matrix matrix_2)
        {
            if (matrix_1 == null || matrix_2 == null)
                return null;

            if (!matrix_1.SizeEqual(matrix_2))
                return null;

            Matrix result = new Matrix(matrix_1);

            int count_Rows = result.values.GetLength(0);
            int count_Columns = result.values.GetLength(1);

            for (int i = 0; i < count_Rows; i++)
                for (int j = 0; j < count_Columns; j++)
                    result.values[i, j] += matrix_2.values[i, j];

            return result;
        }

        public static Matrix operator -(Matrix matrix_1, Matrix matrix_2)
        {
            if (matrix_1 == null || matrix_2 == null)
                return null;

            if (!matrix_1.SizeEqual(matrix_2))
                return null;

            Matrix result = new Matrix(matrix_1);

            int count_Rows = result.values.GetLength(0);
            int count_Columns = result.values.GetLength(1);

            for (int i = 0; i < count_Rows; i++)
                for (int j = 0; j < count_Columns; j++)
                    result.values[i, j] -= matrix_2.values[i, j];

            return result;
        }

        public static Matrix operator -(Matrix matrix, double value)
        {
            return matrix + (-value);
        }

        public override bool Equals(object obj)
        {
            Matrix matrix = obj as Matrix;
            if (matrix == null)
                return false;

            if (!SizeEqual(matrix))
                return false;

            int count_Rows = values.GetLength(0);
            int count_Columns = values.GetLength(1);

            for (int i = 0; i < count_Rows; i++)
                for (int j = 0; j < count_Columns; j++)
                    if (!values[i, j].Equals(matrix.values[i, j]))
                        return false;

            return true;
        }


        public static explicit operator Matrix(double[,] values)
        {
            if (values == null)
                return null;

            return new Matrix(values);
        }
    }
}