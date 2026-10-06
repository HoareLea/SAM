// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

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
                if (occupiedHourIndices == null)
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
                return System.Convert.ToInt32(System.Math.Truncate(OccupiedHours * exceedanceFactor));
            }
        }

        /// <summary>
        /// The number of hours in the full annual series this result was evaluated over - read from the
        /// actual operative-temperature series every TM59 extended result carries (all three collect
        /// candidate hours across the whole year, occupancy included or not), not assumed to be 8760.
        /// <c>-1</c> where no series was ever set; <c>0</c> where it was set but is empty.
        /// </summary>
        public int GetAnnualHours()
        {
            IndexedDoubles operativeTemperatures = OperativeTemperatures;
            if (operativeTemperatures == null)
            {
                return -1;
            }

            if (operativeTemperatures.Count <= 0)
            {
                return 0;
            }

            if (operativeTemperatures.GetMaxIndex() is not int maxIndex || operativeTemperatures.GetMinIndex() is not int minIndex)
            {
                return 0;
            }

            return maxIndex - minIndex + 1;
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
            if (maxAcceptableTemperatures == null || operativeTemperatures == null)
            {
                return double.NaN;
            }

            double result = operativeTemperatures[index] - maxAcceptableTemperatures[index];
            if (double.IsNaN(result))
            {
                return result;
            }

            if (result < 0.5)
            {
                return 0;
            }

            double fraction = result % 1;

            result = System.Math.Truncate(result);
            if (fraction >= 0.5)
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
                if (double.IsNaN(temperatureDifference))
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
                for (int i = minIndex; i <= maxIndex; i++)
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

                if (hoursExceedingComfortRange == 0)
                {
                    return true;
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

        public TMExtendedResult(Guid guid, string name, string source, string reference, TM52BuildingCategory tM52BuildingCategory)
            : base(guid, name, source, reference, tM52BuildingCategory)
        {

        }

        public TMExtendedResult(TMExtendedResult tMExtendedResult)
            : base(tMExtendedResult)
        {
            if (tMExtendedResult != null)
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
        public TMExtendedResult(System.Text.Json.Nodes.JsonObject jsonObject)

            : base(jsonObject)

        {

        }

        public override bool FromJsonObject(JsonObject jsonObject)
        {
            if (!base.FromJsonObject(jsonObject))
            {
                return false;
            }

            if (jsonObject.ContainsKey("ExceedanceFactor"))
            {
                exceedanceFactor = jsonObject["ExceedanceFactor"]?.GetValue<double>() ?? double.NaN;
            }

            if (jsonObject["OccupiedHourIndices"] is JsonArray occupiedHourIndicesArray)
            {
                occupiedHourIndices = new HashSet<int>();
                foreach (JsonNode node in occupiedHourIndicesArray)
                {
                    occupiedHourIndices.Add(node?.GetValue<int>() ?? 0);
                }
            }

            if (jsonObject["MinAcceptableTemperatures"] is JsonObject minAcceptableTemperaturesJson)
            {
                minAcceptableTemperatures = new IndexedDoubles((JsonObject)minAcceptableTemperaturesJson.DeepClone());
            }

            if (jsonObject["MaxAcceptableTemperatures"] is JsonObject maxAcceptableTemperaturesJson)
            {
                maxAcceptableTemperatures = new IndexedDoubles((JsonObject)maxAcceptableTemperaturesJson.DeepClone());
            }

            if (jsonObject["OperativeTemperatures"] is JsonObject operativeTemperaturesJson)
            {
                operativeTemperatures = new IndexedDoubles((JsonObject)operativeTemperaturesJson.DeepClone());
            }

            return true;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject result = base.ToJsonObject();
            if (result == null)
            {
                return null;
            }

            if (!double.IsNaN(exceedanceFactor))
            {
                result["ExceedanceFactor"] = exceedanceFactor;
            }

            if (occupiedHourIndices != null)
            {
                JsonArray occupiedHourIndicesArray = new JsonArray();
                foreach (int occupiedHourIndex in occupiedHourIndices)
                {
                    occupiedHourIndicesArray.Add(occupiedHourIndex);
                }

                result["OccupiedHourIndices"] = occupiedHourIndicesArray;
            }

            if (minAcceptableTemperatures?.ToJsonObject() is JsonObject minAcceptableTemperaturesJson)
            {
                result["MinAcceptableTemperatures"] = minAcceptableTemperaturesJson.DeepClone();
            }

            if (maxAcceptableTemperatures?.ToJsonObject() is JsonObject maxAcceptableTemperaturesJson)
            {
                result["MaxAcceptableTemperatures"] = maxAcceptableTemperaturesJson.DeepClone();
            }

            if (operativeTemperatures?.ToJsonObject() is JsonObject operativeTemperaturesJson)
            {
                result["OperativeTemperatures"] = operativeTemperaturesJson.DeepClone();
            }

            return result;
        }
    }
}
