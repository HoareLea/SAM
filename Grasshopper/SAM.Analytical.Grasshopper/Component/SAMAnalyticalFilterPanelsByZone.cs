// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalFilterPanelsByZone : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6f2d8c7a-9b3e-4c1a-9e2f-3a7c5d0b8e4f");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAMAnalyticalFilterPanelsByZone class.
        /// </summary>
        public SAMAnalyticalFilterPanelsByZone()
          : base("SAMAnalytical.FilterPanelsByZone", "SAMAnalytical.FilterPanelsByZone",
              "SUMMARY\nFilters Panels associated with Zones selected either by their Zone Category Name or by connecting specific Zones directly.\nThe component separates Panels into external Panels, internal Panels between Spaces in the same Zone, and internal Panels leading to Spaces outside that Zone.\nThis can be used to identify external elements, partitions within Flats, party walls between Flats, and Panels between Flats and Corridors.\n\n" +
              "INPUTS\n_analyticalModel: SAM AnalyticalModel containing the Zones, Spaces and Panels to query.\nzoneCategoryName_: One or more Zone Category Names used to select Zones, specific SAM Zones connected directly, or a mixture of both. Zones sharing the same category name remain separate Zones.\n\n" +
              "OUTPUTS\nexternalPanels: External Panels associated with Spaces in the selected Zones.\ninternalPanelsWithinZone: Internal Panels between Spaces belonging to the same selected Zone.\ninternalPanelsToSpacesOutsideZone: Internal Panels between a Space in a selected Zone and a Space outside that same Zone. The other Space may belong to another Zone or may not be assigned to a Zone.\n\n" +
              "NOTES\nThe component uses individual Zone identity when classifying Panels.\nTwo different Zones may share the same Zone Category Name - a Panel between those Zones is returned in internalPanelsToSpacesOutsideZone.\nPanels are returned only once in each output.\nThe supplied AnalyticalModel remains unchanged.\n\n" +
              "EXAMPLE\nAnalyticalModel -> Filter Panels By Zone, with zoneCategoryName_ = \"Flat\", gives externalPanels as the external walls, roofs or exposed Panels associated with Flats, internalPanelsWithinZone as the partitions between Spaces inside the same Flat, and internalPanelsToSpacesOutsideZone as the party walls between Flats and the Panels between Flats and Corridors.",
              "SAM", "Analytical02")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM AnalyticalModel containing the Zones, Spaces and Panels to query.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_GenericObject() { Name = "zoneCategoryName_", NickName = "zoneCategoryName_", Description = "Zone Category Names used to select Zones, specific SAM Zones connected directly, or a mixture of both. Different Zones sharing the same category name remain separate.", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "externalPanels", NickName = "externalPanels", Description = "External Panels associated with Spaces belonging to the selected Zones.", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "internalPanelsWithinZone", NickName = "internalPanelsWithinZone", Description = "Internal Panels between Spaces belonging to the same selected Zone.", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "internalPanelsToSpacesOutsideZone", NickName = "internalPanelsToSpacesOutsideZone", Description = "Internal Panels between a Space in a selected Zone and a Space outside that same Zone. The other Space may belong to another Zone or may have no Zone assignment.", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = Params.IndexOfInputParam("_analyticalModel");
            AnalyticalModel analyticalModel = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Please supply a valid SAM AnalyticalModel.");
                return;
            }

            index = Params.IndexOfInputParam("zoneCategoryName_");
            List<GH_ObjectWrapper> zoneCategoryName_ObjectWrappers = new List<GH_ObjectWrapper>();
            if (index != -1)
            {
                dataAccess.GetDataList(index, zoneCategoryName_ObjectWrappers);
            }

            List<Zone> zones_Explicit = new List<Zone>();
            List<string> zoneCategoryNames_Raw = new List<string>();
            foreach (GH_ObjectWrapper zoneCategoryName_ObjectWrapper in zoneCategoryName_ObjectWrappers)
            {
                object value = zoneCategoryName_ObjectWrapper?.Value;
                if (value is IGH_Goo)
                {
                    value = (value as dynamic).Value;
                }

                if (value is Zone zone)
                {
                    zones_Explicit.Add(zone);
                }
                else if (value is string zoneCategoryName_Raw)
                {
                    zoneCategoryNames_Raw.Add(zoneCategoryName_Raw);
                }
            }

            List<string> zoneCategoryNames = new List<string>();
            foreach (string zoneCategoryName_Raw in zoneCategoryNames_Raw)
            {
                if (zoneCategoryName_Raw == null)
                {
                    continue;
                }

                string zoneCategoryName_Trimmed = zoneCategoryName_Raw.Trim();
                if (zoneCategoryName_Trimmed.Length == 0)
                {
                    continue;
                }

                if (!zoneCategoryNames.Any(x => x.Equals(zoneCategoryName_Trimmed, StringComparison.OrdinalIgnoreCase)))
                {
                    zoneCategoryNames.Add(zoneCategoryName_Trimmed);
                }
            }

            if (zones_Explicit.Count == 0 && zoneCategoryNames.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Please supply at least one Zone Category Name or Zone.");
                return;
            }

            List<Panel> externalPanels = new List<Panel>();
            List<Panel> internalPanelsWithinZone = new List<Panel>();
            List<Panel> internalPanelsToSpacesOutsideZone = new List<Panel>();

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            if (adjacencyCluster != null)
            {
                List<Zone> zones = new List<Zone>(zones_Explicit);

                List<Zone> zones_ByCategoryName = zoneCategoryNames.Count == 0 ? null : adjacencyCluster.ZonesByCategoryName(zoneCategoryNames);
                if (zones_ByCategoryName != null)
                {
                    zones.AddRange(zones_ByCategoryName);
                }

                if (zones.Count == 0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No Zones were found for the supplied Zone Category Name.");
                }
                else if (!zones.Any(x => (adjacencyCluster.GetSpaces(x)?.Count ?? 0) > 0))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The matched Zones do not contain any Spaces.");
                }
                else
                {
                    externalPanels = adjacencyCluster.FilterPanelsByZone(zones, out internalPanelsWithinZone, out internalPanelsToSpacesOutsideZone);

                    if (externalPanels.Count == 0 && internalPanelsWithinZone.Count == 0 && internalPanelsToSpacesOutsideZone.Count == 0)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No Panels were found for the matched Zones.");
                    }
                }
            }

            index = Params.IndexOfOutputParam("externalPanels");
            if (index != -1)
            {
                dataAccess.SetDataList(index, externalPanels.ConvertAll(x => new GooPanel(x)));
            }

            index = Params.IndexOfOutputParam("internalPanelsWithinZone");
            if (index != -1)
            {
                dataAccess.SetDataList(index, internalPanelsWithinZone.ConvertAll(x => new GooPanel(x)));
            }

            index = Params.IndexOfOutputParam("internalPanelsToSpacesOutsideZone");
            if (index != -1)
            {
                dataAccess.SetDataList(index, internalPanelsToSpacesOutsideZone.ConvertAll(x => new GooPanel(x)));
            }
        }
    }
}
