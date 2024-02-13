using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public abstract class TMExtendedResult : TMResult
    {
        private double exceedanceFactor = 0.03;

        private HashSet<int> occupiedHourIndices;
        private IndexedDoubles minAcceptableTemperatures;
        private IndexedDoubles maxAcceptableTemperatures;
        private IndexedDoubles operativeTemperatures;

        public HashSet<int> OccupiedHourIndices
        {
            get
            {
                return occupiedHourIndices == null ? null : new HashSet<int>(occupiedHourIndices);
            }
        }

        public override int OccupiedHours
        {
            get
            {
                if(occupiedHourIndices == null)
                {
                    return 0;
                }

                return occupiedHourIndices.Count;
            }
        }

        public double ExceedanceFactor
        {
            get
            {
                return exceedanceFactor;
            }
        }

        public override int MaxExceedableHours
        {
            get
            {
                return System.Convert.ToInt32(System.Math.Round(OccupiedHours * exceedanceFactor));
            }
        }

        public HashSet<int> GetOccupiedHourIndicesExceedingComfortRange()
        {
            IndexedDoubles occupiedTemperatureDifferences = GetOccupiedTemperatureDifferences();
            if (occupiedTemperatureDifferences == null)
            {
                return null;
            }

            HashSet<int> result = new HashSet<int>();

            IEnumerable<int> keys = occupiedTemperatureDifferences.Keys;
            if (keys == null)
            {
                return result;
            }

            foreach (int key in keys)
            {
                if (occupiedTemperatureDifferences[key] >= 1)
                {
                    result.Add(key);
                }
            }

            return result;
        }

        public int GetOccupiedHoursExceedingComfortRange()
        {
            HashSet<int> indexes = GetOccupiedHourIndicesExceedingComfortRange();
            if (indexes == null)
            {
                return -1;
            }

            return indexes.Count();
        }

        public double GetTemperatureDifference(int index)
        {
            if(maxAcceptableTemperatures == null || operativeTemperatures == null)
            {
                return double.NaN;
            }

            double result = operativeTemperatures[index] - maxAcceptableTemperatures[index];
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

        public IndexedDoubles GetOccupiedTemperatureDifferences()
        {
            if (maxAcceptableTemperatures == null || operativeTemperatures == null || occupiedHourIndices == null)
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

        public IndexedDoubles GetTemperatureDifferences()
        {
            if (maxAcceptableTemperatures == null || operativeTemperatures == null || occupiedHourIndices == null)
            {
                return null;
            }

            IndexedDoubles result = new IndexedDoubles();

            int maxIndex = System.Math.Max(maxAcceptableTemperatures.GetMaxIndex().Value, maxAcceptableTemperatures.GetMaxIndex().Value);
            int minIndex = System.Math.Min(operativeTemperatures.GetMinIndex().Value, operativeTemperatures.GetMinIndex().Value);
            for (int i = minIndex; i <= maxIndex; i++)
            {
                double temperatureDifference = GetTemperatureDifference(i);
                if(double.IsNaN(temperatureDifference))
                {
                    continue;
                }

                result.Add(i, temperatureDifference);
            }

            return result;
        }

        public IndexedDoubles MaxAcceptableTemperatures
        {
            get
            {
                return maxAcceptableTemperatures == null ? null : new IndexedDoubles(maxAcceptableTemperatures);
            }
        }

        public IndexedDoubles MinAcceptableTemperatures
        {
            get
            {
                return minAcceptableTemperatures == null ? null : new IndexedDoubles(minAcceptableTemperatures);
            }
        }

        public IndexedDoubles OperativeTemperatures
        {
            get
            {
                return operativeTemperatures == null ? null : new IndexedDoubles(operativeTemperatures);
            }
        }

        public IndexedDoubles Occupancies
        {
            get
            {
                if (maxAcceptableTemperatures == null || operativeTemperatures == null || occupiedHourIndices == null)
                {
                    return null;
                }

                IndexedDoubles result = new IndexedDoubles();

                int maxIndex = System.Math.Max(maxAcceptableTemperatures.GetMaxIndex().Value, maxAcceptableTemperatures.GetMaxIndex().Value);
                int minIndex = System.Math.Min(operativeTemperatures.GetMinIndex().Value, operativeTemperatures.GetMinIndex().Value);
                for(int i = minIndex; i <= maxIndex; i++)
                {
                    result.Add(i, occupiedHourIndices.Contains(i) ? 1 : 0);
                }

                return result;
            }
        }

        public virtual bool Criterion1
        {
            get
            {
                int maxExceedableHours = MaxExceedableHours;
                if (maxExceedableHours == -1)
                {
                    return false;
                }

                int hoursExceedingComfortRange = GetOccupiedHoursExceedingComfortRange();
                if (hoursExceedingComfortRange == -1)
                {
                    return false;
                }

                return hoursExceedingComfortRange < maxExceedableHours;
            }
        }

        public override bool Pass
        {
            get
            {
                return Criterion1;
            }
        }

        public TMExtendedResult(string name, string source, string reference, TM52BuildingCategory tM52BuildingCategory)
            : base(name, source, reference, tM52BuildingCategory)
        {

        }

        public TMExtendedResult(string name, string source, string reference, TM52BuildingCategory tM52BuildingCategory, HashSet<int> occupiedHourIndices, IndexedDoubles minAcceptableTemperatures, IndexedDoubles maxAcceptableTemperatures, IndexedDoubles operativeTemperatures)
            : base(name, source, reference, tM52BuildingCategory)
        {
            this.occupiedHourIndices = occupiedHourIndices == null ? null : new HashSet<int>(occupiedHourIndices);
            this.minAcceptableTemperatures = minAcceptableTemperatures == null ? null : new IndexedDoubles(minAcceptableTemperatures);
            this.maxAcceptableTemperatures = maxAcceptableTemperatures == null ? null : new IndexedDoubles(maxAcceptableTemperatures);
            this.operativeTemperatures = operativeTemperatures == null ? null : new IndexedDoubles(operativeTemperatures);
        }

        public TMExtendedResult(Guid guid, string name, string source, string reference,TM52BuildingCategory tM52BuildingCategory)
            : base(guid, name, source, reference, tM52BuildingCategory)
        {

        }

        public TMExtendedResult(TMExtendedResult tMExtendedResult)
            : base(tMExtendedResult)
        {
            if(tMExtendedResult != null)
            {
                exceedanceFactor = tMExtendedResult.exceedanceFactor;

                occupiedHourIndices = tMExtendedResult.occupiedHourIndices == null ? null : new HashSet<int>(tMExtendedResult.occupiedHourIndices);
                minAcceptableTemperatures = tMExtendedResult.minAcceptableTemperatures == null ? null : new IndexedDoubles(tMExtendedResult.minAcceptableTemperatures);
                maxAcceptableTemperatures = tMExtendedResult.maxAcceptableTemperatures == null ? null : new IndexedDoubles(tMExtendedResult.maxAcceptableTemperatures);
                operativeTemperatures = tMExtendedResult.operativeTemperatures == null ? null : new IndexedDoubles(tMExtendedResult.operativeTemperatures);
            }
        }

        public TMExtendedResult(TMExtendedResult tMExtendedResult, HashSet<int> occupiedHourIndices, IndexedDoubles minAcceptableTemperatures, IndexedDoubles maxAcceptableTemperatures, IndexedDoubles operativeTemperatures)
            : base(Guid.NewGuid(), tMExtendedResult)
        {
            if (tMExtendedResult != null)
            {
                exceedanceFactor = tMExtendedResult.exceedanceFactor;
            }

            this.occupiedHourIndices = occupiedHourIndices == null ? null : new HashSet<int>(occupiedHourIndices);
            this.minAcceptableTemperatures = minAcceptableTemperatures == null ? null : new IndexedDoubles(minAcceptableTemperatures);
            this.maxAcceptableTemperatures = maxAcceptableTemperatures == null ? null : new IndexedDoubles(maxAcceptableTemperatures);
            this.operativeTemperatures = operativeTemperatures == null ? null : new IndexedDoubles(operativeTemperatures);
        }

        public TMExtendedResult(JObject jObject)
            : base(jObject)
        {
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
            {
                return false;
            }

            if (jObject.ContainsKey("ExceedanceFactor"))
            {
                exceedanceFactor = jObject.Value<double>("ExceedanceFactor");
            }

            if (jObject.ContainsKey("OccupiedHourIndices"))
            {
                JArray jArray = jObject.Value<JArray>("OccupiedHourIndices");
                if (jArray != null)
                {
                    occupiedHourIndices = new HashSet<int>();
                    foreach (int occupiedHourIndex in jArray)
                    {
                        occupiedHourIndices.Add(occupiedHourIndex);
                    }
                }
            }

            if (jObject.ContainsKey("MinAcceptableTemperatures"))
            {
                minAcceptableTemperatures = new IndexedDoubles(jObject.Value<JObject>("MinAcceptableTemperatures"));
            }

            if (jObject.ContainsKey("MaxAcceptableTemperatures"))
            {
                maxAcceptableTemperatures = new IndexedDoubles(jObject.Value<JObject>("MaxAcceptableTemperatures"));
            }

            if (jObject.ContainsKey("OperativeTemperatures"))
            {
                operativeTemperatures = new IndexedDoubles(jObject.Value<JObject>("OperativeTemperatures"));
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

            if (!double.IsNaN(exceedanceFactor))
            {
                result.Add("ExceedanceFactor", exceedanceFactor);
            }

            if (occupiedHourIndices != null)
            {
                JArray jArray = new JArray();
                foreach(int occupiedHourIndex in occupiedHourIndices)
                {
                    jArray.Add(occupiedHourIndex);
                }

                result.Add("OccupiedHourIndices", jArray);
            }

            if(minAcceptableTemperatures != null)
            {
                result.Add("MinAcceptableTemperatures", minAcceptableTemperatures.ToJObject());
            }

            if (maxAcceptableTemperatures != null)
            {
                result.Add("MaxAcceptableTemperatures", maxAcceptableTemperatures.ToJObject());
            }

            if (operativeTemperatures != null)
            {
                result.Add("OperativeTemperatures", operativeTemperatures.ToJObject());
            }

            return result;
        }
    }
}