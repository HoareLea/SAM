using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public class TM52ExtendedResult : TMExtendedResult
    {
        public List<double> GetOccupiedTemperatureDifferencesExceedingComfortRange()
        {
            HashSet<int> hourIndicesExceedingComfortRange = GetOccupiedHourIndicesExceedingComfortRange();
            if(hourIndicesExceedingComfortRange == null)
            {
                return null;
            }

            List<double> result = new List<double>();
            foreach(int hourIndex in hourIndicesExceedingComfortRange)
            {
                result.Add(GetTemperatureDifference(hourIndex));
            }


            return result;
        }

        private Dictionary<int, HashSet<int>> GetOccupiedDailyHourIndicesDictionary()
        {
            HashSet<int> occupiedHourIndices = OccupiedHourIndices;
            if (occupiedHourIndices == null)
            {
                return null;
            }

            Dictionary<int, HashSet<int>> result = new Dictionary<int, HashSet<int>>();

            foreach(int occupiedHourIndex in occupiedHourIndices)
            {
                int boundedIndex = Core.Query.BoundedIndex(24, occupiedHourIndex);
                int dayIndex = System.Convert.ToInt32(occupiedHourIndex / 24);

                if(!result.TryGetValue(dayIndex, out HashSet<int> dailyHourIndexes) || dailyHourIndexes == null)
                {
                    dailyHourIndexes = new HashSet<int>();
                    result[dayIndex] = dailyHourIndexes;
                }

                dailyHourIndexes.Add(boundedIndex);
            }

            return result;
        }

        private List<DailyWeightedExceedance> GetOccupiedDailyWeightedExceedances()
        {
            Dictionary<int, HashSet<int>> dailyHourIndicesDictionary = GetOccupiedDailyHourIndicesDictionary();
            if(dailyHourIndicesDictionary == null)
            {
                return null;
            }

            List<DailyWeightedExceedance> result = new List<DailyWeightedExceedance>();
            foreach (KeyValuePair<int, HashSet<int>> keyValuePair in dailyHourIndicesDictionary)
            {
                int dayIndex = keyValuePair.Key;

                IndexedDoubles temperatureDifferences = new IndexedDoubles();
                foreach (int hourIndex in keyValuePair.Value)
                {
                    double temperatureDifference = GetTemperatureDifference((dayIndex * 24) + hourIndex);
                    temperatureDifferences.Add(hourIndex, temperatureDifference);
                }

                result.Add(new DailyWeightedExceedance(dayIndex, temperatureDifferences));
            }

            return result;
        }

        public int GetOccupiedDailyWeightedExceedanceStartHourIndex()
        {
            GetOccupiedDailyWeightedExceedance(out DailyWeightedExceedance dailyWeightedExceedance);
            if(dailyWeightedExceedance == null)
            {
                return -1;
            }

            return dailyWeightedExceedance.StartHourIndex;
        }
        
        public double GetOccupiedDailyWeightedExceedance()
        {
            return GetOccupiedDailyWeightedExceedance(out DailyWeightedExceedance dailyWeightedExceedance);
        }

        public double GetOccupiedDailyWeightedExceedance(out DailyWeightedExceedance dailyWeightedExceedance)
        {
            dailyWeightedExceedance = null;

            List<DailyWeightedExceedance> dailyWeightedExceedances = GetOccupiedDailyWeightedExceedances();
            if(dailyWeightedExceedances == null)
            {
                return double.NaN;
            }

            if(dailyWeightedExceedances.Count == 0)
            {
                return 0;
            }

            double result = double.MinValue;
            foreach(DailyWeightedExceedance dailyWeightedExceedance_Temp in dailyWeightedExceedances)
            {
                if(dailyWeightedExceedance_Temp == null)
                {
                    continue;
                }

                double weightedExceedance = dailyWeightedExceedance_Temp.WeightedExceedance;
                if(weightedExceedance > result)
                {
                    result = weightedExceedance;
                    dailyWeightedExceedance = dailyWeightedExceedance_Temp;
                }
            }

            if(result == double.MinValue)
            {
                return 0;
            }

            return result;
        }

        public double GetOccupiedMaxTemperatureDifference()
        {
            int hourIndex = GetOccupiedMaxTemperatureDifferenceHourIndex();
            if(hourIndex == -1)
            {
                return double.NaN;
            }

            return GetTemperatureDifference(hourIndex);
        }

        public int GetOccupiedMaxTemperatureDifferenceHourIndex()
        {
            IndexedDoubles occupiedTemperatureDifferences = GetOccupiedTemperatureDifferences();
            if(occupiedTemperatureDifferences == null || occupiedTemperatureDifferences.Count == 0)
            {
                return -1;
            }

            return occupiedTemperatureDifferences.GetMaxValueIndex();

        }

        public int GetOccupiedHoursExceedingAbsoluteLimit()
        {
            IndexedDoubles occupiedTemperatureDifferences = GetOccupiedTemperatureDifferences();
            if(occupiedTemperatureDifferences == null)
            {
                return -1;
            }

            IEnumerable<double> values = occupiedTemperatureDifferences?.Values;
            if (values == null || values.Count() == 0)
            {
                return 0;
            }

            int result = 0;
            foreach (double value in values)
            {
                if (value >= 4)
                {
                    result++;
                }
            }

            return result;
        }

        public List<int> GetOccupiedHourIndicesExceedingAbsoluteLimit()
        {
            IndexedDoubles occupiedTemperatureDifferences = GetOccupiedTemperatureDifferences();

            IEnumerable<int> keys = occupiedTemperatureDifferences?.Keys;
            if(keys == null)
            {
                return null;
            }

            List<int> result = new List<int>();
            foreach(int key in keys)
            {
                double value = occupiedTemperatureDifferences[key];
                if(value >= 4)
                {
                    result.Add(key);
                }
            }
            return result;
        }

        public bool Criterion2
        {
            get
            {
                List<DailyWeightedExceedance> occupiedDailyWeightedExceedances = GetOccupiedDailyWeightedExceedances();
                if(occupiedDailyWeightedExceedances == null)
                {
                    return false;
                }

                if (occupiedDailyWeightedExceedances.Count == 0)
                {
                    return true;
                }

                return occupiedDailyWeightedExceedances.FindIndex(x => x.Exceed) == -1;
            }
        }

        public bool Criterion3
        {
            get
            {
                IndexedDoubles occupiedTemperatureDifferences = GetOccupiedTemperatureDifferences();
                if(occupiedTemperatureDifferences == null)
                {
                    return false;
                }

                IEnumerable<double> values = occupiedTemperatureDifferences.Values;
                if(values == null || values.Count() == 0)
                {
                    return true;
                }

                foreach (double value in values)
                {
                    if(value >= 4)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public override bool Pass
        {
            get
            {
                List<bool> passes = new List<bool>() { Criterion1, Criterion2, Criterion3 };
                passes.RemoveAll(x => !x);

                return passes.Count > 1;
            }
        }

        public TM52ExtendedResult(string name, string source, string reference, TM52BuildingCategory tM52BuildingCategory)
            : base(name, source, reference, tM52BuildingCategory)
        {

        }

        public TM52ExtendedResult(string name, string source, string reference, TM52BuildingCategory tM52BuildingCategory, HashSet<int> occupiedHourIndices, IndexedDoubles minAcceptableTemperatures, IndexedDoubles maxAcceptableTemperatures, IndexedDoubles operativeTemperatures)
            : base(name, source, reference, tM52BuildingCategory, occupiedHourIndices, minAcceptableTemperatures, maxAcceptableTemperatures, operativeTemperatures)
        {

        }

        public TM52ExtendedResult(Guid guid, string name, string source, string reference, TM52BuildingCategory tM52BuildingCategory)
            : base(guid, name, source, reference, tM52BuildingCategory)
        {

        }

        public TM52ExtendedResult(TM52ExtendedResult tM52ExtendedResult)
            : base(tM52ExtendedResult)
        {

        }

        public TM52ExtendedResult(TM52ExtendedResult tM52ExtendedResult, HashSet<int> occupiedHourIndices, IndexedDoubles minAcceptableTemperatures, IndexedDoubles maxAcceptableTemperatures, IndexedDoubles operativeTemperatures)
            : base(tM52ExtendedResult, occupiedHourIndices, minAcceptableTemperatures, maxAcceptableTemperatures, operativeTemperatures)
        {

        }

        public TM52ExtendedResult(JObject jObject)
            : base(jObject)
        {
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            return true;
        }

        public override JObject ToJObject()
        {
           JObject result = base.ToJObject();
            if (result == null)
            {
                return null;
            }

            return result;
        }
    }
}