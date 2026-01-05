// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double OccupancyLatentGain(this Space space)
        {
            if (space == null)
                return double.NaN;

            InternalCondition internalCondition = space.InternalCondition;
            if (internalCondition == null)
                return double.NaN;

            double result = double.NaN;
            if (internalCondition.TryGetValue(Analytical.InternalConditionParameter.OccupancyLatentGainPerPerson, out result) && !double.IsNaN(result))
            {
                double occupancy = CalculatedOccupancy(space);
                if (double.IsNaN(occupancy))
                    return double.NaN;

                result = result * occupancy;
            }

            return result;
        }
    }
}
