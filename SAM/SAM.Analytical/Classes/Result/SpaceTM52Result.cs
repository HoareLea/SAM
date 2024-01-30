using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public class SpaceTM52Result : Result, IAnalyticalObject
    {
        private double exceedanceFactor = 0.03;

        private HashSet<int> occupiedHourIndices;
        private IndexedDoubles maximumAcceptableTemperatures;
        private IndexedDoubles operativeTemperatures;


        public HashSet<int> OccupiedHourIndices
        {
            get
            {
                return occupiedHourIndices == null ? null : new HashSet<int>(occupiedHourIndices);
            }
        }

        public int GetMaxExceedableHours()
        {
            if (occupiedHourIndices == null)
            {
                return -1;
            }

            return System.Convert.ToInt32(System.Math.Round(occupiedHourIndices.Count * exceedanceFactor));
        }

        public double GetTemperatureDifference(int index)
        {
            if(maximumAcceptableTemperatures == null || operativeTemperatures == null)
            {
                return double.NaN;
            }

            double result = operativeTemperatures[index] - maximumAcceptableTemperatures[index];
            if(double.IsNaN(result))
            {
                return result;
            }

            if(result < 0.5)
            {
                return 0;
            }

            double fraction = result % 1;

            result = System.Math.Truncate(result);
            if(fraction >= 0.5)
            {
                result = System.Math.Truncate(result + 1.0);
            }

            return result;
        }

        public IndexedDoubles GetTemperatureDifferences()
        {
            if (maximumAcceptableTemperatures == null || operativeTemperatures == null || occupiedHourIndices == null)
            {
                return null;
            }

            IndexedDoubles result = new IndexedDoubles();
            foreach (int occupiedHourIndex in occupiedHourIndices)
            {
                double temperatureDifference = GetTemperatureDifference(occupiedHourIndex);
                result.Add(occupiedHourIndex, temperatureDifference);
            }

            return result;
        }

        public HashSet<int> GetHourIndicesExceedingComfortRange()
        {
            IndexedDoubles temperatureDifferences = GetTemperatureDifferences();
            if(temperatureDifferences == null)
            {
                return null;
            }

            IEnumerable<int> keys = temperatureDifferences.Keys;
            if(keys == null)
            {
                return null;
            }

            HashSet<int> result = new HashSet<int>();
            foreach (int key in keys)
            {
                if(temperatureDifferences[key] >= 1)
                {
                    result.Add(key);
                }
            }


            return result;
        }

        public int GetHoursExceedingComfortRange()
        {
            HashSet<int> indexes = GetHourIndicesExceedingComfortRange();
            if(indexes == null)
            {
                return -1;
            }

            return indexes.Count();
        }

        public Dictionary<int, HashSet<int>> GetDailyHourIndicesDictionary()
        {
            if(occupiedHourIndices == null)
            {
                return null;
            }

            Dictionary<int, HashSet<int>> result = new Dictionary<int, HashSet<int>>();

            foreach(int occupiedHourIndex in occupiedHourIndices)
            {
                int boundedIndex = Core.Query.BoundedIndex(23, occupiedHourIndex);
                int dayIndex = System.Convert.ToInt32(occupiedHourIndex / 23);

                if(!result.TryGetValue(dayIndex, out HashSet<int> dailyHourIndexes) || dailyHourIndexes == null)
                {
                    dailyHourIndexes = new HashSet<int>();
                    result[dayIndex] = dailyHourIndexes;
                }

                dailyHourIndexes.Add(boundedIndex);
            }

            return result;
        }

        public List<DailyWeightedExceedance> GetDailyWeightedExceedances()
        {
            Dictionary<int, HashSet<int>> dailyHourIndicesDictionary = GetDailyHourIndicesDictionary();
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
                    double temperatureDifference = GetTemperatureDifference(dayIndex + hourIndex);
                    temperatureDifferences.Add(hourIndex, temperatureDifference);
                }

                result.Add(new DailyWeightedExceedance(dayIndex, temperatureDifferences));
            }

            return result;
        }

        public bool Criterion1
        {
            get
            {
                int maxExceedableHours = GetMaxExceedableHours();
                if(maxExceedableHours == -1)
                {
                    return false;
                }

                int hoursExceedingComfortRange = GetHoursExceedingComfortRange();
                if(hoursExceedingComfortRange == -1)
                {
                    return false;
                }

                return hoursExceedingComfortRange < maxExceedableHours;
            }
        }

        public bool Criterion2
        {
            get
            {
                List<DailyWeightedExceedance> dailyWeightedExceedances = GetDailyWeightedExceedances();
                if(dailyWeightedExceedances == null || dailyWeightedExceedances.Count == 0)
                {
                    return false;
                }

                return dailyWeightedExceedances.FindIndex(x => x.Exceed) == -1;
            }
        }

        public bool Criterion3
        {
            get
            {
                IndexedDoubles temperatureDifferences = GetTemperatureDifferences();

                IEnumerable<double> values = temperatureDifferences?.Values;
                if(values == null || values.Count() == 0)
                {
                    return false;
                }

                foreach (double value in values)
                {
                    if(value > 4)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public bool Pass
        {
            get
            {
                List<bool> passes = new List<bool>() { Criterion1, Criterion2, Criterion3 };
                passes.RemoveAll(x => !x);

                return passes.Count > 1;
            }
        }

        public SpaceTM52Result(string name, string source, string reference)
            : base(name, source, reference)
        {

        }

        public SpaceTM52Result(string name, string source, string reference, HashSet<int> occupiedHourIndices, IndexedDoubles maximumAcceptableTemperatures, IndexedDoubles operativeTemperatures)
            : base(name, source, reference)
        {
            this.occupiedHourIndices = occupiedHourIndices == null ? null : new HashSet<int>(occupiedHourIndices);
            this.maximumAcceptableTemperatures = maximumAcceptableTemperatures == null ? null : new IndexedDoubles(maximumAcceptableTemperatures);
            this.operativeTemperatures = operativeTemperatures == null ? null : new IndexedDoubles(operativeTemperatures);
        }

        public SpaceTM52Result(Guid guid, string name, string source, string reference)
            : base(guid, name, source, reference)
        {

        }

        public SpaceTM52Result(SpaceTM52Result spaceTM52Result)
            : base(spaceTM52Result)
        {
            if(spaceTM52Result != null)
            {
                exceedanceFactor = spaceTM52Result.exceedanceFactor;

                occupiedHourIndices = spaceTM52Result.occupiedHourIndices == null ? null : new HashSet<int>(spaceTM52Result.occupiedHourIndices);
                maximumAcceptableTemperatures = spaceTM52Result.maximumAcceptableTemperatures == null ? null : new IndexedDoubles(spaceTM52Result.maximumAcceptableTemperatures);
                operativeTemperatures = spaceTM52Result.operativeTemperatures == null ? null : new IndexedDoubles(spaceTM52Result.operativeTemperatures);
            }
        }

        public SpaceTM52Result(JObject jObject)
            : base(jObject)
        {
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            return true;
        }

        public override JObject ToJObject()
        {
           JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            return jObject;
        }
    }
}