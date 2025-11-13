using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Convert
    {
        public static List<string> ToDesignExplorer(this IEnumerable<AnalyticalModel> analyticalModels)
        {
            if (analyticalModels == null)
            {
                return null;
            }

            List<List<Tuple<string, object>>> data = [];

            foreach (AnalyticalModel analyticalModel in analyticalModels)
            {
                if (ToDesignExplorer(analyticalModel) is List<Tuple<string, object>> tuples)
                {
                    data.Add(tuples);
                }
            }

            if(data == null || data.Count == 0)
            {
                return null;
            }

            List<string> header = [];

            List<Dictionary<int, object>> values = [];

            foreach (List<Tuple<string, object>> tuples in data)
            {
                HashSet<int> usedIndexes = [];

                Dictionary<int, object> dictionary = [];

                foreach (Tuple<string, object> tuple in tuples)
                {
                    string name = tuple.Item1;

                    int index = header.IndexOf(name);
                    if (index == -1)
                    {
                        index = header.Count;
                        usedIndexes.Add(index);
                        header.Add(name);
                    }
                    else if (usedIndexes.Contains(index))
                    {
                        do
                        {
                            index = header.IndexOf(name, index);
                        }
                        while (index == -1 || !usedIndexes.Contains(index));

                        if (index == -1)
                        {
                            index = header.Count;
                            usedIndexes.Add(index);
                            header.Add(name);
                        }
                        else
                        {
                            usedIndexes.Add(index);
                        }
                    }
                    else
                    {
                        usedIndexes.Add(index);
                    }

                    dictionary[index] = tuple.Item2;
                }

                values.Add(dictionary);
            }


            for (int i = 0; i < header.Count; i++)
            {
                string name = header[i];

                List<int> indexes = [];

                for (int j = i + 1; j < header.Count; j++)
                {
                    if(name == header[j])
                    {
                        indexes.Add(j);
                    }
                }

                if(indexes.Count == 0)
                {
                    continue;
                }

                indexes.Insert(0, i);

                name = name.Trim();
                string unit = string.Empty;
                if(name.LastIndexOf(']') == name.Length - 1 && name.LastIndexOf('[') > 0)
                {
                    int index = name.LastIndexOf('[');
                    name = name.Substring(0, index);
                    unit = name.Substring(index);
                }

                for(int x = 0; x < indexes.Count; x++)
                {
                    header[indexes[x]] = string.Format("{0} {1} {2}", name, x + 1, unit).Trim();
                }
            }

            List<string> result = [string.Join(",", header)];

            foreach(Dictionary<int, object> dictionary in values)
            {
                List<string> line = Enumerable.Repeat(string.Empty, header.Count).ToList();
                foreach(KeyValuePair<int, object> keyValuePair in dictionary)
                {
                    line[keyValuePair.Key] = keyValuePair.Value?.ToString() ?? string.Empty;
                }
                result.Add(string.Join(",", line));
            }

            return result;
        }

        public static List<Tuple<string, object>> ToDesignExplorer(this AnalyticalModel analyticalModel)
        {
            if (analyticalModel == null)
            {
                return null;
            }

            List<Tuple<string, object>> result = [];

            if (analyticalModel.TryGetValue(AnalyticalModelParameter.CaseDataCollection, out CaseDataCollection caseDataCollection) && caseDataCollection != null)
            {
                foreach (CaseData caseData in caseDataCollection)
                {
                    if (caseData is ApertureConstructionCaseData apertureConstructionCaseData)
                    {
                        if (apertureConstructionCaseData.ApertureConstruction.Name is string name)
                        {
                            result.Add(new Tuple<string, object>("in:ApertureCONSTR", name));
                        }
                    }
                    else if (caseData is OpeningCaseData openingCaseData)
                    {
                        if (openingCaseData.OpeningAngle is double openingAngle && !double.IsNaN(openingAngle))
                        {
                            result.Add(new Tuple<string, object>("in:OpeningAngle[°]", openingAngle));
                        }
                    }
                    else if (caseData is ShadeCaseData shadeCaseData)
                    {
                        if (shadeCaseData.OverhangDepth is double overhangDepth && !double.IsNaN(overhangDepth))
                        {
                            result.Add(new Tuple<string, object>("in:OverhangDepth[m]", overhangDepth));
                        }

                        if (shadeCaseData.LeftFinDepth is double leftFinDepth && !double.IsNaN(leftFinDepth))
                        {
                            result.Add(new Tuple<string, object>("in:LFinDepth[m]", leftFinDepth));
                        }

                        if (shadeCaseData.RightFinDepth is double rightFinDepth && !double.IsNaN(rightFinDepth))
                        {
                            result.Add(new Tuple<string, object>("in:RFinDepth[m]", rightFinDepth));
                        }
                    }
                    else if (caseData is VentilationCaseData ventilationCaseData)
                    {
                        if (ventilationCaseData.ACH is double aCH && !double.IsNaN(aCH))
                        {
                            result.Add(new Tuple<string, object>("in:ACH[ac/h]", aCH));
                        }
                    }
                    else if (caseData is WeatherCaseData weatherCaseData)
                    {
                        if (weatherCaseData.WeatherDataName is string name)
                        {
                            result.Add(new Tuple<string, object>("in:WeatherData", name));
                        }
                    }
                    else if (caseData is WindowSizeCaseData windowSizeCaseData)
                    {
                        if (windowSizeCaseData.ApertureScaleFactor is double apertureScaleFactor && !double.IsNaN(apertureScaleFactor))
                        {
                            result.Add(new Tuple<string, object>("in:ApertureScale[-]", apertureScaleFactor));
                        }
                    }
                }
            }

            AnalyticalModelSimulationResult analyticalModelSimulationResult = analyticalModel.GetAnalyticalModelSimulationResults()?.FirstOrDefault();

            if (analyticalModelSimulationResult != null)
            {
                double consumptionHeating = analyticalModelSimulationResult.GetValue<double>(AnalyticalModelSimulationResultParameter.ConsumptionHeating);
                double peakHeatingLoad = analyticalModelSimulationResult.GetValue<double>(AnalyticalModelSimulationResultParameter.PeakHeatingLoad);
                double peakHeatingHour = analyticalModelSimulationResult.GetValue<double>(AnalyticalModelSimulationResultParameter.PeakHeatingHour);

                double consumptionCooling = analyticalModelSimulationResult.GetValue<double>(AnalyticalModelSimulationResultParameter.ConsumptionCooling);
                double peakCoolingLoad = analyticalModelSimulationResult.GetValue<double>(AnalyticalModelSimulationResultParameter.PeakCoolingLoad);
                double peakCoolingHour = analyticalModelSimulationResult.GetValue<double>(AnalyticalModelSimulationResultParameter.PeakCoolingHour);

                result.Add(new Tuple<string, object>("out:ConsumptionHTG[kWh]", double.IsNaN(consumptionHeating) ? 0 : Core.Query.Round(consumptionHeating / 1000, 0.01)));
                result.Add(new Tuple<string, object>("out:PeakHTGLoad[kW]", double.IsNaN(peakHeatingLoad) ? 0 : Core.Query.Round(peakHeatingLoad / 1000, 0.01)));
                result.Add(new Tuple<string, object>("out:PeakHeatingHr", peakHeatingHour == -1 ? 0 : peakHeatingHour));

                result.Add(new Tuple<string, object>("out:ConsumptionCLG[kWh]", double.IsNaN(consumptionCooling) ? 0 : Core.Query.Round(consumptionCooling / 1000, 0.01)));
                result.Add(new Tuple<string, object>("out:PeakCLGLoad[kW]", double.IsNaN(peakCoolingLoad) ? 0 : Core.Query.Round(peakCoolingLoad / 1000, 0.01)));
                result.Add(new Tuple<string, object>("out:PeakCoolingHr", peakCoolingHour == -1 ? 0 : peakCoolingHour));
            }

            List<AdjacencyClusterSimulationResult> adjacencyClusterSimulationResults = analyticalModel.GetResults<AdjacencyClusterSimulationResult>();
            if (adjacencyClusterSimulationResults != null && adjacencyClusterSimulationResults.Count != 0)
            {
                int unmetHours_Cooling = -1;
                int unmetHours_Heating = -1;
                int unmetHours = -1;

                AdjacencyClusterSimulationResult adjacencyClusterSimulationResult_Cooling = adjacencyClusterSimulationResults.Find(x => x.GetValue<LoadType>(AdjacencyClusterSimulationResultParameter.LoadType) == LoadType.Cooling);
                if (adjacencyClusterSimulationResult_Cooling != null)
                {
                    unmetHours_Cooling = adjacencyClusterSimulationResult_Cooling.GetValue<int>(AdjacencyClusterSimulationResultParameter.UnmetHours);
                    result.Add(new Tuple<string, object>("out:UnmetCLGHrs", unmetHours_Cooling == -1 ? 0 : unmetHours_Cooling));
                }

                AdjacencyClusterSimulationResult adjacencyClusterSimulationResult_Heating = adjacencyClusterSimulationResults.Find(x => x.GetValue<LoadType>(AdjacencyClusterSimulationResultParameter.LoadType) == LoadType.Heating);
                if (adjacencyClusterSimulationResult_Heating != null)
                {
                    unmetHours_Heating = adjacencyClusterSimulationResult_Heating.GetValue<int>(AdjacencyClusterSimulationResultParameter.UnmetHours);
                    result.Add(new Tuple<string, object>("out:UnmetHTGHrs", unmetHours_Heating == -1 ? 0 : unmetHours_Heating));
                }

                if (unmetHours_Cooling != -1 || unmetHours_Heating != -1)
                {
                    if (unmetHours_Cooling == -1)
                    {
                        unmetHours = unmetHours_Heating;
                    }
                    else if (unmetHours_Heating == -1)
                    {
                        unmetHours = unmetHours_Cooling;
                    }
                    else
                    {
                        unmetHours = unmetHours_Heating + unmetHours_Cooling;
                    }

                    result.Add(new Tuple<string, object>("out:Unmet Hours", unmetHours == -1 ? 0 : unmetHours));
                }
            }

            return result;
        }
    }
}