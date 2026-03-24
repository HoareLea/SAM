// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Newtonsoft.Json.Linq;
using SAM.Analytical.Classes;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static PartFData PartFData(string path)
        {
            PartFData result = new ();

            if(!string.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path))
            {
                string json = System.IO.File.ReadAllText(path);

                JObject jObject = JObject.Parse(json);

                if(jObject.ContainsKey("WholeDwellingRates_Lps"))
                {
                    JObject jObject_Temp = jObject.Value<JObject>("WholeDwellingRates_Lps");
                    foreach (JProperty property in jObject_Temp.Properties())
                    {
                        string name = property.Name;
                        JToken value = property.Value;

                        double value_Temp;

                        if(name == "IncrementAbove5" && Core.Query.TryConvert(value, out value_Temp))
                        {
                            result.IncrementAbove5 = value_Temp;
                            continue;
                        }
                        else if (name == "AreaRate_LpsPerM2" && Core.Query.TryConvert(value, out value_Temp))
                        {
                            result.AreaRate_LpsPerM2 = value_Temp;
                            continue;
                        }
                        else if(Core.Query.TryConvert<int>(name, out int @int) && Core.Query.TryConvert(value, out value_Temp))
                        {
                            result.WholeDwellingRates_Lps[@int] = value_Temp;
                        }

                    }
                }

                if(jObject.ContainsKey("Categories"))
                {
                    JArray jArray_Temp = jObject.Value<JArray>("Categories");
                    if(jArray_Temp is not null)
                    {
                        foreach(JObject jObject_Category in jArray_Temp)
                        {
                            if(jObject_Category.Value<string>("Category") is not string name || string.IsNullOrWhiteSpace(name))
                            {
                                continue;
                            }

                            Enums.PartFType partFType = Enums.PartFType.Habitable;
                            if (jObject_Category.Value<string>("PartFCategory") is string category && !string.IsNullOrWhiteSpace(category))
                            {
                                partFType = Core.Query.Enum<Enums.PartFType>(category);
                            }

                            Enums.PartFVentilationType partFVentilationType = Enums.PartFVentilationType.supply;
                            if (jObject_Category.Value<string>("VentilationType") is string ventilationType && !string.IsNullOrWhiteSpace(ventilationType))
                            {
                                partFVentilationType = Core.Query.Enum<Enums.PartFVentilationType>(ventilationType);
                            }

                            bool isBedroom = false;
                            if (jObject_Category.Value<bool>("IsBedroom") is bool isBedroom_Temp)
                            {
                                isBedroom = isBedroom_Temp;
                            }

                            double? minFlowRate_Lps = jObject_Category["MinFlowRate_Lps"]?.Value<double?>();

                            bool includeInFloorAreaCheck = false;
                            if (jObject_Category.Value<bool>("IncludeInFloorAreaCheck") is bool includeInFloorAreaCheck_Temp)
                            {
                                includeInFloorAreaCheck = includeInFloorAreaCheck_Temp;
                            }

                            bool isTerminalSpace = false;
                            if (jObject_Category.Value<bool>("IsTerminalSpace") is bool isTerminalSpace_Temp)
                            {
                                isTerminalSpace = isTerminalSpace_Temp;
                            }

                            bool scaleSupplyWithVolume = false;
                            if (jObject_Category.Value<bool>("ScaleSupplyWithVolume") is bool scaleSupplyWithVolume_Temp)
                            {
                                scaleSupplyWithVolume = scaleSupplyWithVolume_Temp;
                            }

                            bool scaleExtractAboveMinimum = false;
                            if (jObject_Category.Value<bool>("scaleExtractAboveMinimum") is bool scaleExtractAboveMinimum_Temp)
                            {
                                scaleExtractAboveMinimum = scaleExtractAboveMinimum_Temp;
                            }

                            string defaultFlowWeightBasis = null;
                            if (jObject_Category.Value<string>("DefaultFlowWeightBasis") is string defaultFlowWeightBasis_Temp)
                            {
                                defaultFlowWeightBasis = defaultFlowWeightBasis_Temp;
                            }

                            double? calculatedFlowRate_Lps = jObject_Category["CalculatedFlowRate_Lps"]?.Value<double?>();

                            List<string> synonyms = [];
                            if (jObject_Category.ContainsKey("Synonyms"))
                            {
                                if (jObject_Category.Value<JArray>("Synonyms") is JArray jArray)
                                {
                                    foreach(string synonym in jArray)
                                    {
                                        synonyms.Add(synonym);
                                    }
                                }
                            }

                            PartFCategory partFCategory = new PartFCategory(
                                name,
                                partFType,
                                partFVentilationType,
                                isBedroom,
                                minFlowRate_Lps,
                                includeInFloorAreaCheck,
                                isTerminalSpace,
                                scaleSupplyWithVolume,
                                scaleExtractAboveMinimum,
                                defaultFlowWeightBasis,
                                calculatedFlowRate_Lps, 
                                synonyms);

                            result.PartFCategories[partFCategory.Name] = partFCategory;
                        }
                    }
                }
            }

            return result;
        }
    }
}
