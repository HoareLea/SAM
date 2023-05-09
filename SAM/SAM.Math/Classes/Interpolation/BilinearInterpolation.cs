using Newtonsoft.Json.Linq;
using SAM.Core;

namespace SAM.Math
{

    /// <summary>
    /// Class to perform Bilinear Interpolation on a set of data.
    /// </summary>
    public class BilinearInterpolation : IJSAMObject
    {
        /// <summary>
        /// Array of row (horizontal) values.
        /// </summary>
        private double[] xArray;

        /// <summary>
        /// Array of column (vertical) values.
        /// </summary>
        private double[] yArray;

        /// <summary>
        /// The data matrix for interpolation, in [y, x] format.
        /// </summary>
        private double[,] values;

        /// <summary>
        /// Initializes a new instance of the BilinearInterpolation class using a JSON object.
        /// </summary>
        /// <param name="jObject">The JSON object containing the data.</param>
        public BilinearInterpolation(JObject jObject)
        {
            FromJObject(jObject);
        }

        /// <summary>
        /// Initializes a new instance of the BilinearInterpolation class using a 2D data array.
        /// </summary>
        /// <param name="data">The 2D data array.</param>
        public BilinearInterpolation(double[,] data)
        {
            Load(data);
        }

        /// <summary>
        /// Copy constructor. Initializes a new instance of the BilinearInterpolation class by copying an existing instance.
        /// </summary>
        /// <param name="bilinearInterpolation">The instance to copy.</param>
        public BilinearInterpolation(BilinearInterpolation bilinearInterpolation)
        {
            if (bilinearInterpolation != null)
            {
                xArray = bilinearInterpolation.xArray?.Clone<double>();
                yArray = bilinearInterpolation.yArray?.Clone<double>();
                if(bilinearInterpolation.values != null)
                {
                    values = new double[bilinearInterpolation.values.GetLength(0), bilinearInterpolation.values.GetLength(1)];
                    for (int i = 0; i < bilinearInterpolation.values.GetLength(0); i++)
                    {
                        for (int j = 0; j < bilinearInterpolation.values.GetLength(1); j++)
                        {
                            values[i, j] = bilinearInterpolation.values[i, j];
                        }
                    }
                }
                
            }
        }

        /// <summary>
        /// Default constructor. Initializes a new instance of the BilinearInterpolation class.
        /// </summary>
        public BilinearInterpolation()
        {

        }

        /// <summary>
        /// Load data to BilinearInterpolation class
        /// </summary>
        /// <param name="data">Data in format [y, x], first row - xArray, first column - yArray</param>
        /// <returns></returns>
        public bool Load(double[,] data)
        {
            if (data == null || data.GetLength(0) < 1 || data.GetLength(1) < 1)
            {
                return false;
            }

            yArray = new double[data.GetLength(0) - 1];
            for (int i = 1; i < data.GetLength(0); i++)
            {
                yArray[i - 1] = data[i, 0];
            }

            xArray = new double[data.GetLength(1) - 1];
            for (int i = 1; i < data.GetLength(1); i++)
            {
                xArray[i - 1] = data[0, i];
            }

            values = new double[data.GetLength(0) - 1, data.GetLength(1) - 1];
            for (int i = 1; i < data.GetLength(0); i++)
            {
                for (int j = 1; j < data.GetLength(1); j++)
                {
                    values[i - 1, j - 1] = data[i, j];
                }
            }

            return true;
        }

        public bool Load(string path, string separator = "\t")
        {
            double[,] data = Core.Create.Array(path, separator, true, double.NaN);
            if(data == null)
            {
                return false;
            }

            return Load(data);
        }

        /// <summary>
        /// Calculate value
        /// </summary>
        /// <param name="x">Column value (Horizontal value)</param>
        /// <param name="y">Row value (Vertical value)</param>
        /// <returns></returns>
        public double Calculate(double x, double y)
        {
            if (xArray == null || yArray == null || values == null || double.IsNaN(x) || double.IsNaN(y))
            {
                return double.NaN;
            }

            Query.NeigbourIndices(xArray, x, out int lowerIndex_X, out int upperIndex_X);
            if (lowerIndex_X == -1 || upperIndex_X == -1)
            {
                return double.NaN;
            }

            Query.NeigbourIndices(yArray, y, out int lowerIndex_Y, out int upperIndex_Y);
            if (lowerIndex_Y == -1 || upperIndex_Y == -1)
            {
                return double.NaN;
            }

            double fQ11 = values[lowerIndex_Y, lowerIndex_X];
            if (lowerIndex_X == upperIndex_X && lowerIndex_Y == upperIndex_Y)
            {
                return fQ11;
            }

            double fQ21 = values[lowerIndex_Y, upperIndex_X];
            double fQ12 = values[upperIndex_Y, lowerIndex_X];
            double fQ22 = values[upperIndex_Y, upperIndex_X];

            double y1 = yArray[lowerIndex_Y];
            double y2 = yArray[upperIndex_Y];

            if (lowerIndex_X == upperIndex_X)
            {
                return fQ11 + (fQ12 - fQ11) * (y - y1) / (y2 - y1);
            }

            double x1 = xArray[lowerIndex_X];
            double x2 = xArray[upperIndex_X];

            if (lowerIndex_Y == upperIndex_Y)
            {
                return fQ11 + (fQ22 - fQ11) * (x - x1) / (x2 - x1);
            }

            double result = fQ11 * (x2 - x) * (y2 - y);
            result = result + fQ21 * (x - x1) * (y2 - y);
            result = result + fQ12 * (x2 - x) * (y - y1);
            result = result + fQ22 * (x - x1) * (y - y1);
            result = result / ((x2 - x1) * (y2 - y1));

            return result;
        }

        /// <summary>
        /// Populates the properties of this instance from a JSON object.
        /// </summary>
        /// <param name="jObject">The JSON object.</param>
        /// <returns>true if the operation was successful; otherwise, false.</returns>
        public virtual bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            if (jObject.ContainsKey("XArray"))
            {
                JArray jArray = jObject.Value<JArray>("XArray");
                xArray = new double[jArray.Count];
                for (int i = 0; i < jArray.Count; i++)
                {
                    xArray[i] = jArray[i].Value<double>();
                }
            }

            if (jObject.ContainsKey("YArray"))
            {
                JArray jArray = jObject.Value<JArray>("YArray");
                yArray = new double[jArray.Count];
                for (int i = 0; i < jArray.Count; i++)
                {
                    yArray[i] = jArray[i].Value<double>();
                }
            }

            if (jObject.ContainsKey("Values"))
            {
                JArray jArray = jObject.Value<JArray>("Values");
                values = Core.Query.Array<double>(jArray);
            }

            return true;
        }

        /// <summary>
        /// Converts the properties of this instance to a JSON object.
        /// </summary>
        /// <returns>The JSON object.</returns>
        public virtual JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if(xArray != null)
            {
                jObject.Add("XArray", new JArray(xArray));
            }

            if (yArray != null)
            {
                jObject.Add("YArray", new JArray(yArray));
            }

            if(values != null)
            {
                JArray jArray = Core.Query.JArray(values);
                jObject.Add("Values", jArray);
            }

            return jObject;
        }
    }
}
