using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace SAM.Core
{
    public class TableModifier : SimpleModifier
    {
        public bool Extrapolate { get; set; }

        private SortedDictionary<int, string> headers = new SortedDictionary<int, string>();
        private List<SortedDictionary<int, double>> values = new List<SortedDictionary<int, double>>();

        public TableModifier(ArithmeticOperator arithmeticOperator, IEnumerable<string> headers)
        {
            ArithmeticOperator = arithmeticOperator;
            Headers = headers;
        }

        public TableModifier(TableModifier tableModifier)
            : base(tableModifier)
        {
            if(tableModifier != null)
            {
                if(tableModifier.headers != null)
                {
                    foreach(KeyValuePair<int, string> keyValuePair in tableModifier.headers)
                    {
                        headers[keyValuePair.Key] = keyValuePair.Value;
                    }
                }

                if(tableModifier.values != null)
                {
                    foreach(SortedDictionary<int, double> sortedDictionary in tableModifier.values)
                    {
                        if(sortedDictionary == null)
                        {
                            continue;
                        }

                        SortedDictionary<int, double> sortedDictionary_New = new SortedDictionary<int, double>();
                        foreach(KeyValuePair<int, double> keyValuePair in sortedDictionary)
                        {
                            sortedDictionary_New[keyValuePair.Key] = keyValuePair.Value;
                        }

                        values.Add(sortedDictionary_New);
                    }
                }

                Extrapolate = tableModifier.Extrapolate;
            }
        }

        public TableModifier(JObject jObject)
            :base(jObject)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            bool result = base.FromJObject(jObject);
            if(!result)
            {
                return result;
            }

            if(jObject.ContainsKey("Extrapolate"))
            {
                Extrapolate = jObject.Value<bool>("Extrapolate");
            }

            if(jObject.ContainsKey("Headers"))
            {
                JArray jArray = jObject.Value<JArray>("Headers");
                if(jArray != null)
                {
                    headers = new SortedDictionary<int, string>();
                    foreach(JArray jArray_Header in jArray)
                    {
                        headers[((JValue)jArray_Header[0]).Value<int>()] = ((JValue)jArray_Header[1]).Value<string>();
                    }
                }
            }

            if (jObject.ContainsKey("Values"))
            {
                JArray jArray = jObject.Value<JArray>("Values");
                if (jArray != null)
                {
                    values = new List<SortedDictionary<int, double>>();
                    foreach (JArray jArray_Row in jArray)
                    {
                        SortedDictionary<int, double> sortedDictionary = new SortedDictionary<int, double>();
                        foreach (JArray jArray_Values in jArray_Row)
                        {
                            sortedDictionary[((JValue)jArray_Values[0]).Value<int>()] = ((JValue)jArray_Values[1]).Value<double>();
                        }

                        values.Add(sortedDictionary);
                    }
                }
            }

            return result;
        }

        public IEnumerable<string> Headers
        {
            get
            {
                if(headers == null)
                {
                    return null;
                }

                List<string> result = new List<string>();
                if(headers.Count == 0)
                {
                    return result;
                }

                for(int i = headers.Keys.First(); i <= headers.Keys.Max(); i++)
                {
                    if(!headers.TryGetValue(i, out string header))
                    {
                        header = null;
                    }

                    result.Add(header);
                }

                return result;
            }

            set
            {
                headers.Clear();
                if(value == null)
                {
                    return;
                }

                for (int i = 0; i < value.Count(); i++)
                {
                    headers[i] = value.ElementAt(i);
                }
            }
        }

        public int GetHeaderIndex(string name)
        {
            if(headers == null || headers.Count == 0)
            {
                return -1;
            }

            foreach(KeyValuePair<int, string> keyValuePair in headers)
            {
                if(name == keyValuePair.Value)
                {
                    return keyValuePair.Key;
                }
            }

            return -1;
        }

        public bool AddValues(IDictionary<int, double> values)
        {
            if(values == null)
            {
                return false;
            }

            SortedDictionary<int, double> values_Temp = new SortedDictionary<int, double>();
            foreach(KeyValuePair<int, string> keyValuePair in headers)
            {
                if(!values.TryGetValue(keyValuePair.Key, out double value))
                {
                    continue;
                }

                values_Temp.Add(keyValuePair.Key, value);
            }

            this.values.Add(values_Temp);
            return true;
        }

        public bool AddValues(IDictionary<string, double> values, bool addMissingHeaders = false)
        {
            if(values == null)
            {
                return false;
            }

            SortedDictionary<int, double> values_Temp = new SortedDictionary<int, double>();
            foreach (KeyValuePair<string, double> keyValuePair in values)
            {
                int index = GetHeaderIndex(keyValuePair.Key);
                if(index == -1)
                {
                    if(!addMissingHeaders)
                    {
                        continue;
                    }

                    index = headers.Count == 0 ? 0 : headers.Keys.Last() + 1;
                    headers[index] = keyValuePair.Key;
                }

                values_Temp[index] = keyValuePair.Value;
            }

            return AddValues(values_Temp);
        }

        public List<double> GetColumnValues(int columnIndex)
        {
            if (columnIndex == -1 || headers == null || !headers.ContainsKey(columnIndex) || values == null)
            {
                return null;
            }

            List<double> result = new List<double>();
            foreach (SortedDictionary<int, double> sortedDictionary in values)
            {
                if (!sortedDictionary.TryGetValue(columnIndex, out double value))
                {
                    value = double.NaN;
                }

                result.Add(value);
            }

            return result;
        }

        public Dictionary<int, double> GetDictionary(int rowIndex)
        {
            if(rowIndex < 0 || values == null || values.Count == 0 || values.Count <= rowIndex)
            {
                return null;
            }

            Dictionary<int, double> result = new Dictionary<int, double>();
            foreach(KeyValuePair<int, double> keyValuePair in values[rowIndex])
            {
                result[keyValuePair.Key] = keyValuePair.Value;
            }

            return result;
        }

        public Dictionary<int, double> GetDictionary(int rowIndex, IEnumerable<int> columnIndexes)
        {
            if(columnIndexes == null)
            {
                return null;
            }

            SortedDictionary<int, double> sortedDictionary = values[rowIndex];

            Dictionary<int, double> result = new Dictionary<int, double>();
            foreach (int columnIndex in columnIndexes)
            {
                if(!sortedDictionary.TryGetValue(columnIndex, out double value))
                {
                    continue;
                }

                result[columnIndex] = value;
            }

            return result;
        }

        public List<int> FindIndexes(IDictionary<int, double> values)
        {
            if(values == null || this.values == null || headers == null)
            {
                return null;
            }

            List<int> result = new List<int>();

            for(int i=0; i < this.values.Count; i++)
            {
                SortedDictionary<int, double> sortedDictionary = this.values[i];

                bool add = true;
                foreach (KeyValuePair<int, double> keyValuePair in values)
                {

                }

                if (!add)
                {
                    continue;
                }

                result.Add(i);

            }

            return result;
        }

        public bool RemoveColumn(int index)
        {
            if(index < 0)
            {
                return false;
            }

            if(!headers.Remove(index))
            {
                return false;
            }

            foreach (SortedDictionary<int, double> sortedDictionary in values)
            {
                sortedDictionary?.Remove(index);
            }

            return true;
        }

        public int RowCount
        {
            get 
            {
                if (values != null)
                {
                    return values.Count;
                }

                return -1;
            }
        }

        public double[,] GetValues(out List<string> columnHeader, out List<List<string>> rowHeaders)
        {
            columnHeader = null;
            rowHeaders = null;

            if(headers == null || headers.Count == 0)
            {
                return null;
            }

            double[,] result = null;
            if(headers.Count < 3)
            {
                columnHeader = Headers?.ToList();
                rowHeaders = new List<List<string>>()
                {
                    GetColumnValues(headers.Keys.First())?.ConvertAll(x => x.ToString())
                };

                result = new double[values.Count, headers.Count];
                for (int i = 0; i < values.Count; i++)
                {
                    SortedDictionary<int, double> sortedDictionary = values[i];

                    for (int j = 0; j < headers.Count; j++)
                    {
                        if (sortedDictionary.TryGetValue(j, out double value))
                        {
                            result[i, j] = value;
                        }
                    }
                }

                return result;
            }

            int columnIndex_Value = headers.Keys.Max();
            int columnIndex_ToRemove = columnIndex_Value - 1;

            Dictionary<int, string> headers_Temp = new Dictionary<int, string>();


            List<int> headerIndexes = new List<int>();
            for (int i = 0; i < columnIndex_ToRemove; i++)
            {
                headers_Temp[i] = headers[i];
                headerIndexes.Add(i);
            }

            IEnumerable<double> uniqueValues = GetColumnValues(columnIndex_ToRemove).Distinct();
            foreach(double uniqueValue in uniqueValues)
            {
                headers_Temp[headers_Temp.Keys.Max() + 1] = uniqueValue.ToString();
            }

            columnHeader = uniqueValues.ToList().ConvertAll(x => x.ToString());

            TableModifier tableModifier = new TableModifier(ArithmeticOperator, headers_Temp.Values);

            List<SortedDictionary<int, double>> sortedDictionaries = new List<SortedDictionary<int, double>>(values);
            while(sortedDictionaries.Count > 0)
            {
                Dictionary<int, double> dictionary_Full = new Dictionary<int, double>(values[0]);
                Dictionary<int, double> dictionary_Filtered = GetDictionary(sortedDictionaries, 0, headerIndexes);

                List<int> indexes = FindIndexes(sortedDictionaries, dictionary_Filtered);
                foreach(int index in indexes)
                {
                    double uniqueValue = sortedDictionaries[index][columnIndex_ToRemove];
                    int columnIndex_UniqueValue = tableModifier.GetHeaderIndex(uniqueValue.ToString());
                    dictionary_Filtered[columnIndex_UniqueValue] = sortedDictionaries[index][columnIndex_Value];
                }

                tableModifier.AddValues(dictionary_Filtered);

                indexes.Sort((x, y) => y.CompareTo(x));
                indexes.ForEach(x => sortedDictionaries.RemoveAt(x));
            }

            rowHeaders = new List<List<string>>();
            foreach(int index in headerIndexes)
            {
                rowHeaders.Add(tableModifier.GetColumnValues(index).ConvertAll(x => x.ToString()));
            }

            headerIndexes.Reverse();
            headerIndexes.ForEach(x => tableModifier.RemoveColumn(x));

            result = new double[tableModifier.values.Count, tableModifier.headers.Count];
            for (int i = 0; i < tableModifier.values.Count; i++)
            {
                SortedDictionary<int, double> sortedDictionary = tableModifier.values[i];

                for (int j = 0; j < tableModifier.headers.Count; j++)
                {
                    if (sortedDictionary.TryGetValue(tableModifier.headers.Keys.ElementAt(j), out double value))
                    {
                        result[i, j] = value;
                    }
                }
            }

            return result;
        }

        public static Dictionary<int, double> GetDictionary(IEnumerable<SortedDictionary<int, double>> sortedDictionaries, int rowIndex, IEnumerable<int> columnIndexes)
        {
            if (columnIndexes == null)
            {
                return null;
            }

            SortedDictionary<int, double> sortedDictionary = sortedDictionaries.ElementAt(rowIndex);

            Dictionary<int, double> result = new Dictionary<int, double>();
            foreach (int columnIndex in columnIndexes)
            {
                if (!sortedDictionary.TryGetValue(columnIndex, out double value))
                {
                    continue;
                }

                result[columnIndex] = value;
            }

            return result;
        }

        public List<int> FindIndexes(IEnumerable<SortedDictionary<int, double>> sortedDictionaries, IDictionary<int, double> values)
        {
            if (sortedDictionaries == null || sortedDictionaries == null || headers == null)
            {
                return null;
            }

            List<int> result = new List<int>();

            for (int i = 0; i < sortedDictionaries.Count(); i++)
            {
                SortedDictionary<int, double> sortedDictionary = sortedDictionaries.ElementAt(i);

                bool add = true;
                foreach (KeyValuePair<int, double> keyValuePair in values)
                {
                    if (!sortedDictionary.TryGetValue(keyValuePair.Key, out double value) || keyValuePair.Value != value)
                    {
                        add = false;
                        break;
                    }
                }

                if (!add)
                {
                    continue;
                }

                result.Add(i);

            }

            return result;
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if(result == null)
            {
                return null;
            }

            result.Add("Extrapolate", Extrapolate);

            if(headers != null)
            {
                JArray jArray = new JArray();
                foreach(KeyValuePair<int, string> keyValuePair in headers)
                {
                    jArray.Add(new JArray() { keyValuePair.Key, keyValuePair.Value });
                }

                result.Add("Headers", jArray);
            }

            if(values != null)
            {
                JArray jArray_Values = new JArray();
                foreach(SortedDictionary<int, double> sortedDictionary in values)
                {
                    JArray jArray_Row = new JArray();
                    foreach (KeyValuePair<int, double> keyValuePair in sortedDictionary)
                    {
                        jArray_Row.Add(new JArray() { keyValuePair.Key, keyValuePair.Value });
                    }
                    jArray_Values.Add(jArray_Row);
                }

                result.Add("Values", jArray_Values);
            }

            return result;
        }
    }
}