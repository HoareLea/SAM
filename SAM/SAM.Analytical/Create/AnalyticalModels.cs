// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Analytical.Classes;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static List<AnalyticalModel> AnalyticalModels(this AnalyticalModel analyticalModel, Cases cases)
        {
            if (analyticalModel == null || cases == null)
            {
                return null;
            }

            List<AnalyticalModel> result = [];
            foreach (Case @case in cases)
            {
                AnalyticalModel analyticalModel_Temp = AnalyticalModel(analyticalModel, @case);
                if (analyticalModel_Temp is null)
                {
                    continue;
                }

                result.Add(analyticalModel_Temp);
            }

            return result;
        }

        public static List<AnalyticalModel> AnalyticalModels(this AnalyticalModel analyticalModel, IEnumerable<Cases> cases)
        {
            if (analyticalModel == null || cases == null)
            {
                return null;
            }

            List<AnalyticalModel> result = [];
            foreach (Cases cases_Temp in cases)
            {
                if (AnalyticalModels(analyticalModel, cases_Temp) is not List<AnalyticalModel> analyticalModels)
                {
                    continue;
                }

                result.AddRange(analyticalModels);
            }

            return result;
        }

    }
}
