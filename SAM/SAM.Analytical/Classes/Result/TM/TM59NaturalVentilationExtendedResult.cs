// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class TM59NaturalVentilationExtendedResult : TM59ExtendedResult
    {
        public TM59NaturalVentilationExtendedResult(string name, string source, string reference, TM52BuildingCategory tM52BuildingCategory, HashSet<int> occupiedHourIndices, IndexedDoubles minAcceptableTemperatures, IndexedDoubles maxAcceptableTemperatures, IndexedDoubles operativeTemperatures, params TM59SpaceApplication[] tM59SpaceApplications)
            : base(name, source, reference, tM52BuildingCategory, occupiedHourIndices, minAcceptableTemperatures, maxAcceptableTemperatures, operativeTemperatures, tM59SpaceApplications)
        {

        }

        public TM59NaturalVentilationExtendedResult(TM59NaturalVentilationExtendedResult tM59NaturalVentilationExtendedResult)
            : base(tM59NaturalVentilationExtendedResult)
        {

        }

        public TM59NaturalVentilationExtendedResult(TM59NaturalVentilationExtendedResult tM59NaturalVentilationExtendedResult, HashSet<int> occupiedHourIndices, IndexedDoubles minAcceptableTemperatures, IndexedDoubles maxAcceptableTemperatures, IndexedDoubles operativeTemperatures)
            : base(tM59NaturalVentilationExtendedResult, occupiedHourIndices, minAcceptableTemperatures, maxAcceptableTemperatures, operativeTemperatures)
        {

        }
        public TM59NaturalVentilationExtendedResult(System.Text.Json.Nodes.JsonObject jsonObject)

            : base(jsonObject)

        {

        }

        public HashSet<int> GetSummerOccupiedHourIndices()
        {
            HashSet<int> occupiedHourIndices = OccupiedHourIndices;
            if (occupiedHourIndices == null)
            {
                return null;
            }

            HashSet<int> result = new HashSet<int>();
            foreach (int occupiedHourIndex in occupiedHourIndices)
            {
                if (occupiedHourIndex >= HourOfYear.SummerStartIndex && occupiedHourIndex <= HourOfYear.SummerEndIndex)
                {
                    result.Add(occupiedHourIndex);
                }
            }

            return result;
        }

        public int GetSummerOccupiedHours()
        {
            return GetSummerOccupiedHourIndices().Count;
        }

        public int GetSummerMaxExceedableHours()
        {
            return System.Convert.ToInt32(System.Math.Truncate(GetSummerOccupiedHours() * 0.03));
        }

        public HashSet<int> GetSummerOccupiedHourIndicesExceedingComfortRange()
        {
            HashSet<int> occupiedHourIndicesExceedingComfortRange = GetOccupiedHourIndicesExceedingComfortRange();
            HashSet<int> summerOccupiedHourIndices = GetSummerOccupiedHourIndices();
            if (occupiedHourIndicesExceedingComfortRange == null || summerOccupiedHourIndices == null)
            {
                return null;
            }

            occupiedHourIndicesExceedingComfortRange.IntersectWith(summerOccupiedHourIndices);
            return occupiedHourIndicesExceedingComfortRange;
        }

        public int GetSummerOccupiedHoursExceedingComfortRange()
        {
            HashSet<int> summerOccupiedHourIndicesExceedingComfortRange = GetSummerOccupiedHourIndicesExceedingComfortRange();
            if (summerOccupiedHourIndicesExceedingComfortRange == null)
            {
                return -1;
            }

            return summerOccupiedHourIndicesExceedingComfortRange.Count;
        }

        /// <summary>
        /// TM59:2017 Criterion 1 is a May-September test for naturally-ventilated spaces - both the exceedance
        /// count and the 3% limit it is measured against are counted over summer occupied hours only, not the
        /// whole year <see cref="TMExtendedResult.Criterion1"/> uses. Mirrors that base implementation exactly,
        /// on the summer-restricted figures <see cref="TM59AssessmentReport"/> already displays instead.
        /// </summary>
        public override bool Criterion1
        {
            get
            {
                int maxExceedableHours = GetSummerMaxExceedableHours();

                int hoursExceedingComfortRange = GetSummerOccupiedHoursExceedingComfortRange();
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

    }
}
