// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace SAM.Math
{

    /// <summary>
    /// Linear Interpolation of given vlues
    /// </summary>
    public class LinearInterpolation : IJSAMObject
    {
        private List<KeyValuePair<double, double>> values;

        public LinearInterpolation(JsonObject jsonObject)
        {
            FromJsonObject(jsonObject);
        }

        public LinearInterpolation(double x_1, double y_1, double x_2, double y_2)
        {
            Add(x_1, y_1);
            Add(x_2, y_2);
        }

        public LinearInterpolation()
        {
        }

        public LinearInterpolation(LinearInterpolation linearInterpolation)
        {
            if (linearInterpolation.values != null)
            {
                foreach (KeyValuePair<double, double> keyValuePair in linearInterpolation.values)
                    Add(keyValuePair);
            }
        }

        public bool Add(KeyValuePair<double, double> keyValuePair)
        {
            return Add(keyValuePair.Key, keyValuePair.Value);
        }

        public bool Add(double x, double y)
        {
            if (double.IsNaN(x) || double.IsNaN(y))
                return false;

            if (values == null)
                values = new List<KeyValuePair<double, double>>();

            values.Add(new KeyValuePair<double, double>(x, y));
            return true;
        }

        public double CalculateY(double x)
        {
            List<double> ys = CalculateYs(x, 1);
            if (ys == null || ys.Count == 0)
                return double.NaN;

            return ys[0];
        }

        public List<double> CalculateYs(double x, int maxCount = int.MaxValue)
        {
            if (double.IsNaN(x) || !InRange(x))
                return null;

            double key = double.NaN;
            double value = double.NaN;

            List<double> result = new List<double>();
            foreach (KeyValuePair<double, double> keyValuePair in values)
            {
                if (x == keyValuePair.Key)
                {
                    result.Add(keyValuePair.Value);
                }
                else if (!double.IsNaN(key) && !double.IsNaN(value))
                {
                    double max = System.Math.Max(key, keyValuePair.Key);
                    double min = System.Math.Min(key, keyValuePair.Key);
                    if (x > min && x < max)
                    {
                        if (keyValuePair.Value == key)
                            result.Add(key);
                        else
                            result.Add(value + ((keyValuePair.Value - value) * ((x - key) / (keyValuePair.Key - key))));
                    }
                }

                if (result.Count >= maxCount)
                    break;

                key = keyValuePair.Key;
                value = keyValuePair.Value;
            }

            return result;
        }

        public bool InRange(double x)
        {
            return x >= MinX && x <= MaxX;
        }

        public bool Load(double[,] data)
        {
            if (data == null || data.GetLength(0) < 2 || data.GetLength(1) == 0)
            {
                return false;
            }

            values = new List<KeyValuePair<double, double>>();
            for (int i = 0; i < data.GetLength(1); i++)
            {
                values.Add(new KeyValuePair<double, double>(data[0, i], data[1, i]));
            }

            return true;
        }

        public bool Load(string path, string separator = "\t")
        {
            double[,] data = Core.Create.Array(path, separator, true, double.NaN);
            if (data == null)
            {
                return false;
            }

            return Load(data);
        }

        public double MaxX
        {
            get
            {
                if (values == null)
                    return double.NaN;

                return values.ConvertAll(x => x.Key).Max();
            }
        }

        public double MinX
        {
            get
            {
                if (values == null)
                    return double.NaN;

                return values.ConvertAll(x => x.Key).Min();
            }
        }

        public int Count
        {
            get
            {
                if (values == null)
                    return -1;

                return values.Count;
            }
        }

        public virtual bool FromJsonObject(JsonObject jsonObject)
        {
            if (jsonObject == null)
                return false;

            if (jsonObject["Values"] is JsonArray jsonArray_Values)
            {
                values = new List<KeyValuePair<double, double>>();
                for (int i = 0; i < jsonArray_Values.Count; i++)
                {
                    if (jsonArray_Values[i] is JsonArray jsonArray)
                    {
                        if (jsonArray.Count >= 2)
                        {
                            //Core.Query.TryGetDouble rather than a CLR type test on GetValue<object>():
                            //a parsed JsonValue holds a JsonElement, which no such test recognises, so
                            //every pair was skipped and any saved file read back with no values at all.
                            if (jsonArray[0] == null || !jsonArray[0].TryGetDouble(out double key) || double.IsNaN(key))
                                continue;

                            if (jsonArray[1] == null || !jsonArray[1].TryGetDouble(out double value) || double.IsNaN(value))
                                continue;

                            values.Add(new KeyValuePair<double, double>(key, value));
                        }
                    }
                }
            }

            return true;
        }

        public virtual JsonObject ToJsonObject()
        {
            JsonObject jsonObject = new JsonObject
            {
                ["_type"] = Core.Query.FullTypeName(this)
            };

            if (values != null)
            {
                JsonArray jsonArray = new JsonArray();
                foreach (KeyValuePair<double, double> keyValuePair in values)
                {
                    JsonArray jsonArray_Temp = new JsonArray
                    {
                        keyValuePair.Key,
                        keyValuePair.Value
                    };

                    jsonArray.Add(jsonArray_Temp);
                }

                jsonObject["Values"] = jsonArray;
            }

            return jsonObject;
        }

    }
}
