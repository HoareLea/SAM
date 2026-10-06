// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public class TM59CorridorExtendedResult : TM59ExtendedResult
    {
        public TM59CorridorExtendedResult(string name, string source, string reference, TM52BuildingCategory tM52BuildingCategory, HashSet<int> occupiedHourIndices, IndexedDoubles minAcceptableTemperatures, IndexedDoubles maxAcceptableTemperatures, IndexedDoubles operativeTemperatures)
            : base(name, source, reference, tM52BuildingCategory, occupiedHourIndices, minAcceptableTemperatures, maxAcceptableTemperatures, operativeTemperatures, TM59SpaceApplication.Undefined)
        {

        }

        public TM59CorridorExtendedResult(TM59CorridorExtendedResult tM59CorridorExtendedResult)
            : base(tM59CorridorExtendedResult)
        {

        }

        public TM59CorridorExtendedResult(TM59CorridorExtendedResult tM59CorridorExtendedResult, HashSet<int> occupiedHourIndices, IndexedDoubles minAcceptableTemperatures, IndexedDoubles maxAcceptableTemperatures, IndexedDoubles operativeTemperatures)
            : base(tM59CorridorExtendedResult, occupiedHourIndices, minAcceptableTemperatures, maxAcceptableTemperatures, operativeTemperatures)
        {

        }
        public TM59CorridorExtendedResult(System.Text.Json.Nodes.JsonObject jsonObject)

            : base(jsonObject)

        {

        }

        //GetAnnualHours() is now on the TMExtendedResult base - every TM59 extended result shares the same
        //operative-temperature series, not only the corridor check.

        public int GetHoursNumberExceeding28()
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

            int count = 0;
            foreach (double operativeTemperature in operativeTemperatures)
            {
                if (operativeTemperature > 28)
                {
                    count++;
                }
            }

            return count;
        }

        public override int MaxExceedableHours
        {
            get
            {
                IndexedDoubles operativeTemperatures = OperativeTemperatures;
                if (operativeTemperatures == null)
                {
                    return -1;
                }

                if(operativeTemperatures.Count <= 0)
                {
                    return 0;
                }

                if (operativeTemperatures.GetMaxIndex() is not int maxIndex || operativeTemperatures.GetMinIndex() is not int minIndex)
                {
                    return 0;
                }

                int count = maxIndex - minIndex + 1;
                return System.Convert.ToInt32(System.Math.Truncate(count * ExceedanceFactor));
            }
        }

        public override bool Criterion1
        {
            get
            {
                return GetHoursNumberExceeding28() < MaxExceedableHours;
            }
        }

    }
}
