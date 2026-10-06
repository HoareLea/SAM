// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalModifyZonesCategoryName : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("be852598-6bd0-4581-9bdd-a14eaf0ba091");

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
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalModifyZonesCategoryName()
          : base("SAMAnalytical.ModifyZone", "SAMAnalytical.ModifyZone",
              DescriptionLong,
              "SAM", "Analytical02")
        {
        }

        private const string DescriptionLong =
@"Assign a custom Zone Category name to selected zones, such as Bedroom, Office or Corridor.

SUMMARY
Assigns a user-defined category name to zones within an AdjacencyCluster or AnalyticalModel.
Makes a copy of the supplied object; the original stays unchanged.

INPUTS
_analytical  (AdjacencyCluster | AnalyticalModel, required)
AnalyticalModel or AdjacencyCluster containing the zones to classify.

_zones  (Zone[], optional)
Zones to classify. When omitted, all zones in the analytical object are selected.
Zones not found in the object are ignored.

zoneCategoryName_  (String, optional)
Custom Zone Category name to assign, e.g. ""Bedroom"", ""Office"" or ""Corridor"".
The name does not need to exist beforehand. When omitted, no zones are changed.

OUTPUTS
analytical  (AdjacencyCluster | AnalyticalModel)
Copy of the supplied analytical object with the assigned zone categories.

zones  (Zone[])
Zones selected from the copied object. When a category name is supplied,
these contain the newly assigned Zone Category.

NOTES
Only zones already present in the analytical object are processed.
The component stores the text value on each zone; no new SAM objects are created.

EXAMPLE
AnalyticalModel → _analytical
Selected zones → _zones
""Office"" → zoneCategoryName_
analytical → next SAM component";

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam { Name = "_analytical", NickName = "_analytical", Description = "AnalyticalModel or AdjacencyCluster containing the zones to classify.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooGroupParam() { Name = "_zones", NickName = "_zones", Description = "Zones to classify. When omitted, all zones in the analytical object are selected.", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String param_String = null;

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String { Name = "zoneCategoryName_", NickName = "zoneCategoryName_", Description = "Custom Zone Category name to assign, such as 'Bedroom' or 'Office'. The name does not need to exist beforehand.", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Voluntary));

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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "analytical", NickName = "analytical", Description = "Copy of the supplied analytical object with the assigned zone categories. The original stays unchanged.", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooGroupParam() { Name = "zones", NickName = "zones", Description = "Selected Zone objects. When a category name is supplied, these contain the assigned Zone Category.", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
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
            int index;

            index = Params.IndexOfInputParam("_analytical");
            IAnalyticalObject analyticalObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("zoneCategoryName_");
            string zoneCategory = null;
            if (index == -1 || !dataAccess.GetData(index, ref zoneCategory))
            {
                zoneCategory = null;
            }

            AdjacencyCluster adjacencyCluster = null;

            if (analyticalObject is AnalyticalModel)
            {
                AnalyticalModel analyticalModel = new AnalyticalModel((AnalyticalModel)analyticalObject);
                adjacencyCluster = analyticalModel.AdjacencyCluster;
            }
            else if (analyticalObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)analyticalObject);
            }

            index = Params.IndexOfInputParam("_zones");
            List<Zone> zones = new List<Zone>();
            if (index == -1 || !dataAccess.GetDataList(index, zones))
            {
                zones = adjacencyCluster.GetZones();
            }

            if (zones != null && zones.Count != 0 && zoneCategory != null)
            {
                HashSet<Guid> guids = new HashSet<Guid>(zones.ConvertAll(x => x.Guid));

                zones = adjacencyCluster.GetZones()?.FindAll(x => x != null && guids.Contains(x.Guid));

                for (int i = 0; i < zones.Count; i++)
                {
                    Zone zone = new Zone(zones[i]);

                    zone.SetValue(ZoneParameter.ZoneCategory, zoneCategory);

                    adjacencyCluster.AddObject(zone);
                }
            }

            if (analyticalObject is AnalyticalModel)
            {
                analyticalObject = new AnalyticalModel((AnalyticalModel)analyticalObject, adjacencyCluster);
            }
            else if (analyticalObject is AdjacencyCluster)
            {
                analyticalObject = new AdjacencyCluster(adjacencyCluster);
            }


            index = Params.IndexOfOutputParam("analytical");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalObject);
            }

            index = Params.IndexOfOutputParam("zones");
            if (index != -1)
            {
                dataAccess.SetDataList(index, zones);
            }

        }
    }
}
