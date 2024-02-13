using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public class DailyWeightedExceedance: IAnalyticalObject
    {
        private int dayIndex;
        private IndexedDoubles temperatureDifferences;

        public DailyWeightedExceedance(DailyWeightedExceedance dailyWeightedExceedance)
        {
            if(dailyWeightedExceedance != null)
            {
                dayIndex = dailyWeightedExceedance.dayIndex;
                temperatureDifferences = dailyWeightedExceedance.temperatureDifferences == null ? null : new IndexedDoubles(dailyWeightedExceedance.temperatureDifferences);
            }
        }

        public DailyWeightedExceedance(JObject jObject)
        {
            FromJObject(jObject);
        }

        public DailyWeightedExceedance(int dayIndex, IndexedDoubles temperatureDifferences)
        {
            this.dayIndex = dayIndex;
            this.temperatureDifferences = temperatureDifferences == null ? null : new IndexedDoubles(temperatureDifferences);
        }

        public List<double> UniqueTemperatureDifferences
        {
            get
            {
                return GetCountDictionary()?.Keys?.ToList();
            }
        }

        public List<int> UniqueCounts
        {
            get
            {
                return GetCountDictionary()?.Values?.ToList();
            }
        }

        public int DayIndex
        {
            get
            {
                return dayIndex;
            }
        }

        private Dictionary<double, int> GetCountDictionary()
        {
            IEnumerable<int> keys = temperatureDifferences?.Keys;
            if(keys == null)
            {
                return null;
            }

            Dictionary<double, int> result = new Dictionary<double, int>();
            foreach (int key in keys)
            {
                double temperatureDifference = temperatureDifferences[key];
                if (!result.ContainsKey(temperatureDifference))
                {
                    result[temperatureDifference] = 0;
                }

                result[temperatureDifference]++;
            }

            return result;
        }

        public bool FromJObject(JObject jObject)
        {
            if(jObject == null)
            {
                return false;
            }

            if(jObject.ContainsKey("DayIndex"))
            {
                dayIndex = jObject.Value<int>("DayIndex");
            }

            if (jObject.ContainsKey("TemperatureDifferences"))
            {
                temperatureDifferences = new IndexedDoubles( jObject.Value<JObject>("TemperatureDifferences"));
            }

            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));

            if(dayIndex != -1)
            {
                jObject.Add("DayIndex", dayIndex);
            }

            if(temperatureDifferences != null)
            {
                jObject.Add("TemperatureDifferences", temperatureDifferences.ToJObject());
            }

            return jObject;
        }

        public IndexedDoubles TemperatureDifferences
        {
            get
            {
                return temperatureDifferences == null ? null : new IndexedDoubles(temperatureDifferences);
            }
        }

        public double WeightedExceedance
        {
            get
            {
                Dictionary<double, int> dictionary = GetCountDictionary();
                if (dictionary == null || dictionary.Count == 0)
                {
                    return double.NaN;
                }

                double result = 0;
                foreach (KeyValuePair<double, int> keyValuePair in dictionary)
                {
                    result += keyValuePair.Key * System.Convert.ToDouble(keyValuePair.Value);
                }

                return result;
            }
        }

        public bool Exceed
        {
            get
            {
                double weightedExceedance = WeightedExceedance;
                if(double.IsNaN(weightedExceedance))
                {
                    return false;
                }

                return weightedExceedance > 6;
            }
        }

        public int StartHourIndex
        {
            get
            {
                return dayIndex * 24;
            }
        }

        public int EndHourIndex
        {
            get
            {
                return (dayIndex * 24) + 23;
            }
        }
    }
}