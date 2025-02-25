using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Core.Attributes;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SAM.Analytical
{
    public class DailyModifier : IndexedSimpleModifier
    {
        private List<string> dayNames;
        private Dictionary<string, double[]> values;

        public DailyModifier(ArithmeticOperator arithmeticOperator, IEnumerable<KeyValuePair<string, double[]>> values)
        {
            ArithmeticOperator = arithmeticOperator;
            if(values != null)
            {
                this.values = new Dictionary<string, double[]>();
                dayNames = new List<string>();
                foreach (KeyValuePair<string, double[]> keyValuePair in values)
                {
                    double[] values_Day = new double[24];

                    int count = keyValuePair.Value.Length;
                    for (int i = 0; i < 24; i++)
                    {
                        values_Day[i] = keyValuePair.Value[i % count];
                    }

                    this.values[keyValuePair.Key] = values_Day;
                    dayNames.Add(keyValuePair.Key);
                }
            }
        }

        public DailyModifier(DailyModifier dailyModifier)
            : base(dailyModifier)
        {
            if(dailyModifier != null)
            {
                dayNames = dailyModifier.dayNames == null ? null : new List<string>(dailyModifier.dayNames);
                if (dailyModifier.values != null)
                {
                    values = new Dictionary<string, double[]>();
                    foreach (KeyValuePair<string, double[]> keyValuePair in dailyModifier.values)
                    {
                        double[] values_Day = new double[24];

                        int count = keyValuePair.Value.Length;
                        for (int i = 0; i < 24; i++)
                        {
                            values_Day[i] = keyValuePair.Value[i % count];
                        }

                        values[keyValuePair.Key] = values_Day;
                    }
                }

            }
        }

        public DailyModifier(JObject jObject)
            :base(jObject)
        {

        }

        public List<string> DayNames
        {
            get
            {
                return dayNames == null ? values == null ? null : values.Keys.ToList() : new List<string>(dayNames);
            }
        }

        public override bool ContainsIndex(int index)
        {
            if (values == null)
            {
                return false;
            }

            if (index < 0)
            {
                return false;
            }

            int count = dayNames == null ? values == null ? 0 : values.Count : dayNames.Count;

            return index >= count * 24;
        }

        public override bool FromJObject(JObject jObject)
        {
            bool result = base.FromJObject(jObject);
            if(!result)
            {
                return result;
            }

            if(jObject.ContainsKey("DayNames"))
            {
                dayNames = new List<string>();
                foreach(string dayName in jObject.Value<JArray>("DayNames"))
                {
                    dayNames.Add(dayName);
                }
            }

            if(jObject.ContainsKey("Values"))
            {
                values = new Dictionary<string, double[]>();
                foreach(JArray jArray in jObject.Value<JArray>("Values"))
                {
                    string dayName = jArray[0]?.ToString();

                    List<double> values_Temp = new List<double>();
                    foreach(double value in jArray[1])
                    {
                        values_Temp.Add(value);
                    }

                    values[dayName] = values_Temp.ToArray();
                }
            }

            return result;
        }
        
        public override double GetCalculatedValue(int index, double value)
        {
            if (values == null)
            {
                return value;
            }

            List<string> dayNames_Temp = dayNames == null ? values == null ? null : values.Keys.ToList() : dayNames;
            if (dayNames_Temp == null || dayNames_Temp.Count == 0)
            {
                return value;
            }

            int index_Temp = index % (dayNames_Temp.Count * 24);

            int index_DayName = index_Temp / 24;
            int index_Hour = index_Temp % 24;

            string dayName = dayNames_Temp[index_DayName];

            double value_Temp = values[dayName][index_Hour];

            if (double.IsNaN(value_Temp))
            {
                return double.NaN;
            }

            return Core.Query.Calculate(ArithmeticOperator, value, value_Temp);
        }

        public double GetValue(string dayName, int index)
        {
            if(dayName == null)
            {
                return double.NaN;
            }

            if(values.TryGetValue(dayName, out double[] values_Day))
            {
                return values_Day[index];
            }

            return double.NaN;
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if(result == null)
            {
                return null;
            }

            if(dayNames != null)
            {
                JArray jArray = new JArray();
                dayNames.ForEach(x => jArray.Add(x));

                result.Add("DayNames", jArray);
            }

            if(values != null)
            {
                JArray jArray = new JArray();
                foreach(KeyValuePair<string, double[]> keyValuePair in values)
                {
                    JArray jArray_Temp = new JArray();
                    jArray_Temp.Add(keyValuePair.Key);

                    JArray jArray_Values = new JArray();
                    foreach(double value in keyValuePair.Value)
                    {
                        jArray_Values.Add(value);
                    }
                    jArray_Temp.Add(jArray_Values);

                    jArray.Add(jArray_Temp);
                }

                result.Add("Values", jArray);
            }

            return result;
        }
    }
}