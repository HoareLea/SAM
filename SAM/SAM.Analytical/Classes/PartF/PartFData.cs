// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public class PartFData : SAMObject
    {
        public Dictionary<string, PartFCategory> PartFCategories { get; set; } = [];

        public Dictionary<int, double> WholeDwellingRates_Lps { get; set; } = [];


        private TextMap textMap = null;

        public double IncrementAbove5 { get; set; }

        /// <summary>
        /// AreaRate [l/(s*m2)]
        /// </summary>
        public double AreaRate_LpsPerM2 { get; set; }

        public PartFData()
        {



        }

        public double GetWholeDwellingRates_Lps(int count)
        {
            if(WholeDwellingRates_Lps is null || WholeDwellingRates_Lps.Count == 0)
            {
                return double.NaN;
            }

            if(WholeDwellingRates_Lps.TryGetValue(count, out double result))
            {
                return result;
            }

            result = 19 + (count - 1) * 6;

            return result;
        }

        public PartFCategory GetPartFCategory(string name)
        {
            if(PartFCategories is null || PartFCategories.Count == 0)
            {
                return null;
            }

            if(textMap is null)
            {
                textMap = Core.Create.TextMap("PartF");
                foreach (PartFCategory partFCategory in PartFCategories.Values)
                {
                    if (partFCategory?.Name is not string name_Temp)
                    {
                        continue;
                    }

                    List<string> synonyms = partFCategory.Synonyms;
                    if (synonyms is null || synonyms.Count == 0)
                    {
                        synonyms = new List<string>() { name_Temp };
                    }

                    textMap.Add(name_Temp, synonyms.ToArray());
                }
            }

            HashSet<string> keys = textMap.GetSortedKeys(name);
            if(keys is null || keys.Count == 0)
            {
                return null;
            }

            return PartFCategories[keys.ElementAt(0)];
        }
    }
}
