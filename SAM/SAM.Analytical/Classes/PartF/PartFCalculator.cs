// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public class PartFCalculator
    {
        private readonly PartFData partFData;

        private List<Tuple<Guid, PartFCategory, double>> results = [];
        
        public PartFCalculator(PartFData partFData)
        {
            this.partFData = partFData;
        }

        public AnalyticalModel AnalyticalModel { get; set; }
        
        public bool Calculate()
        {
            if(partFData is null)
            {
                return false;
            }

            AdjacencyCluster adjacencyCluster = AnalyticalModel?.AdjacencyCluster;
            if(adjacencyCluster is null)
            {
                return false;
            }

            adjacencyCluster = new AdjacencyCluster(adjacencyCluster, deepClone: true);

            List<Space> spaces = adjacencyCluster.GetSpaces();
            List<double> ls = [];
            if (spaces is not null && spaces.Count != 0)
            {
                List<Tuple<PartFCategory, Space>> tuples = [];
                foreach(Space space in spaces)
                {
                    if(partFData.GetPartFCategory(space.Name) is not PartFCategory partFCategory)
                    {
                        continue;
                    }

                    space.SetValue(SpaceParameter.PartFCategory, partFCategory);
                    adjacencyCluster.AddObject(space);

                    tuples.Add(new Tuple<PartFCategory, Space>(partFCategory, space));
                }

                int bedroomCount = tuples.FindAll(x => x.Item1.IsBedroom).Count;

                double bedroomBaseRate = partFData.GetWholeDwellingRates_Lps(bedroomCount);

                double internalFloorAreaExcludingVoids = tuples.FindAll(x => x.Item1.IncludeInFloorAreaCheck).ConvertAll(x => x.Item2.GetValue<double>(SpaceParameter.Area)).Sum();

                double wholeDwelingRate = System.Math.Max(bedroomBaseRate, 0.3 * internalFloorAreaExcludingVoids);

                double extractMinTotal = tuples.FindAll(x => x.Item1.MinFlowRate_Lps is not null).ConvertAll(x => x.Item1.MinFlowRate_Lps.Value).Sum();

                double finalSystemRate = System.Math.Max(wholeDwelingRate, extractMinTotal);

                double totalSupply = finalSystemRate;
                double totalExtract = finalSystemRate;

                List<Tuple<PartFCategory, Space>> tuples_Temp;

                tuples_Temp = tuples.FindAll(x => 
                    x.Item1.PartFVentilationType == Enums.PartFVentilationType.supply &&
                    x.Item1.ScaleSupplyWithVolume && 
                    x.Item1.IsTerminalSpace);

                double totalSupplyWeight = tuples_Temp.ConvertAll(x => x.Item2.GetValue<double>(SpaceParameter.Volume)).Sum();
                foreach (Tuple<PartFCategory, Space> tuple in tuples_Temp)
                {
                    double flowRate = finalSystemRate * (tuple.Item2.GetValue<double>(SpaceParameter.Volume) / totalSupplyWeight);
                    results.Add(new Tuple<Guid, PartFCategory, double>(tuple.Item2.Guid, tuple.Item1, flowRate));
                }


                tuples_Temp = tuples.FindAll(x =>
                    x.Item1.PartFVentilationType == Enums.PartFVentilationType.extract &&
                    x.Item1.IsTerminalSpace);

                double extraExtractNeeded = finalSystemRate - tuples_Temp.FindAll(x => x.Item1.MinFlowRate_Lps is not null).ConvertAll(x => x.Item1.MinFlowRate_Lps.Value).Sum();

                tuples_Temp = tuples_Temp.FindAll(x => x.Item1.ScaleExtractAboveMinimum);

                double totalExtractWeight = tuples_Temp.ConvertAll(x => x.Item2.GetValue<double>(SpaceParameter.Volume)).Sum();
                foreach (Tuple<PartFCategory, Space> tuple in tuples_Temp)
                {
                    double flowRate = extraExtractNeeded * (tuple.Item2.GetValue<double>(SpaceParameter.Volume) / totalExtractWeight);
                    results.Add(new Tuple<Guid, PartFCategory, double>(tuple.Item2.Guid, tuple.Item1, flowRate + tuple.Item1.MinFlowRate_Lps ?? 0));
                }

            }

            AnalyticalModel = new AnalyticalModel(AnalyticalModel, adjacencyCluster);

            return true;
        }

        public double GetFlowRate(Space space)
        {
            if(space is null)
            {
                return double.NaN;
            }

            Tuple<Guid, PartFCategory, double> tuple = results.Find(x => space.Guid == x.Item1);

            if (tuple is null)
            {
                return 0;
            }

            return tuple.Item3;
        }
    }
}
