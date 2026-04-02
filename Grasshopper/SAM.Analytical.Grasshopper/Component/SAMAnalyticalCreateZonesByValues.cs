// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateZonesByValues : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("619aff3e-54e2-4a72-a809-62cb029e1e00");

        /// <summary>
        /// The latest version of this component.
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initialises a new instance of the SAMAnalytical.CreateZonesByValues component.
        /// </summary>
        public SAMAnalyticalCreateZonesByValues()
          : base("SAMAnalytical.CreateZonesByValues", "SAMAnalytical.CreateZonesByValues",
              "Creates zones by grouping spaces using matching zone names.\n\nEach item in _zoneNames is matched by index to a space in _spaces. Spaces with the same zone name are grouped into one zone.\n\nThe component makes a copy of the input analytical object, so the original stays unchanged.",
              "SAM", "Analytical01")
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

                result.Add(new GH_SAMParam(
                    new global::Grasshopper.Kernel.Parameters.Param_GenericObject()
                    {
                        Name = "_analytical",
                        NickName = "_analytical",
                        Description = "SAM Analytical object to update.\nAccepts an AdjacencyCluster or an AnalyticalModel.\nThe component makes a copy; the original stays unchanged.",
                        Access = GH_ParamAccess.item
                    },
                    ParamVisibility.Binding));

                result.Add(new GH_SAMParam(
                    new GooSpaceParam
                    {
                        Name = "_spaces",
                        NickName = "_spaces",
                        Description = "Spaces to group into zones.\nEach space is matched by index with one item in _zoneNames.",
                        Access = GH_ParamAccess.list
                    },
                    ParamVisibility.Binding));

                result.Add(new GH_SAMParam(
                    new global::Grasshopper.Kernel.Parameters.Param_GenericObject
                    {
                        Name = "_zoneNames",
                        NickName = "_zoneNames",
                        Description = "Zone names for the spaces.\nProvide one zone name for each space in _spaces, in the same order.\nSpaces with the same zone name are grouped into one zone.",
                        Access = GH_ParamAccess.list
                    },
                    ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String param_String = null;

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String
                {
                    Name = "_zoneType",
                    NickName = "_zoneType",
                    Description = "Zone type to assign to the created zones.\nUsed only when zoneCategoryName_ is not supplied.",
                    Access = GH_ParamAccess.item,
                    Optional = true
                };
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String
                {
                    Name = "zoneCategoryName_",
                    NickName = "zoneCategoryName_",
                    Description = "Zone category name for the created zones.\nIf supplied, this overrides _zoneType.",
                    Access = GH_ParamAccess.item,
                    Optional = true
                };
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean boolean = null;
                boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean
                {
                    Name = "cleanSpacesByZoneCategory_",
                    NickName = "cleanSpacesByZoneCategory_",
                    Description = "If true, removes an existing zone with the same name and zone category before creating the new one.\nDefault is false.",
                    Access = GH_ParamAccess.item,
                    Optional = true
                };
                boolean.SetPersistentData(false);

                result.Add(new GH_SAMParam(boolean, ParamVisibility.Binding));

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

                result.Add(new GH_SAMParam(
                    new global::Grasshopper.Kernel.Parameters.Param_GenericObject()
                    {
                        Name = "analytical",
                        NickName = "analytical",
                        Description = "Updated SAM Analytical object with the created or updated zones.",
                        Access = GH_ParamAccess.item
                    },
                    ParamVisibility.Binding));

                result.Add(new GH_SAMParam(
                    new global::Grasshopper.Kernel.Parameters.Param_GenericObject()
                    {
                        Name = "zones",
                        NickName = "zones",
                        Description = "Zones created or updated from the supplied zone names.",
                        Access = GH_ParamAccess.list
                    },
                    ParamVisibility.Voluntary));

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
            SAMObject sAMObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_spaces");
            List<Space> spaces = new List<Space>();
            if (index == -1 || !dataAccess.GetDataList(index, spaces) || spaces == null || spaces.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_zoneNames");
            List<object> zoneNames = new List<object>();
            if (index == -1 || !dataAccess.GetDataList(index, zoneNames) || zoneNames == null || zoneNames.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            for (int i = 0; i < zoneNames.Count; i++)
            {
                if (zoneNames[i] is IGH_Goo)
                {
                    zoneNames[i] = (zoneNames[i] as dynamic).Value;
                }
            }

            index = Params.IndexOfInputParam("_zoneType");
            string zoneCategory = null;
            if (index == -1 || !dataAccess.GetData(index, ref zoneCategory) || string.IsNullOrWhiteSpace(zoneCategory))
            {
                zoneCategory = null;
            }

            index = Params.IndexOfInputParam("cleanSpacesByZoneCategory_");
            bool cleanSpacesByZoneCategory = false;
            if (index == -1 || !dataAccess.GetData(index, ref cleanSpacesByZoneCategory))
            {
                cleanSpacesByZoneCategory = false;
            }

            if (!Enum.TryParse(zoneCategory, out ZoneType zoneType))
            {
                zoneType = ZoneType.Undefined;
            }

            index = Params.IndexOfInputParam("zoneCategoryName_");
            zoneCategory = null;
            if (index == -1 || !dataAccess.GetData(index, ref zoneCategory))
            {
                zoneCategory = null;
            }

            if (zoneType == ZoneType.Undefined && zoneCategory == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            if (sAMObject is AnalyticalModel)
            {
                AnalyticalModel analyticalModel = new AnalyticalModel((AnalyticalModel)sAMObject);
                adjacencyCluster = analyticalModel.AdjacencyCluster;
            }
            else if (sAMObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);
            }

            if (adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<string> zoneNames_String = zoneNames.ConvertAll(x => x?.ToString().Trim());

            List<string> names = zoneNames_String.Distinct().ToList();
            names.RemoveAll(x => string.IsNullOrWhiteSpace(x));

            if (zoneType != ZoneType.Undefined && string.IsNullOrWhiteSpace(zoneCategory))
            {
                zoneCategory = zoneType.Text();
            }

            List<Zone> zones_Result = new List<Zone>();
            foreach (string name_Temp in names)
            {
                if (cleanSpacesByZoneCategory)
                {
                    Zone zone_Temp = adjacencyCluster.GetZones()?.Find(x => x?.Name == name_Temp && x.GetValue<string>(ZoneParameter.ZoneCategory) == zoneCategory);
                    if (zone_Temp != null)
                    {
                        adjacencyCluster.RemoveObject<Zone>(zone_Temp.Guid);
                    }
                }

                List<int> indexes = zoneNames_String.IndexesOf(name_Temp);
                List<Space> spaces_Temp = indexes.ConvertAll(x => spaces[x]);
                Zone zone = Analytical.Modify.UpdateZone(adjacencyCluster, name_Temp, zoneCategory, spaces_Temp.ToArray());
                if (zone != null)
                {
                    zones_Result.Add(zone);
                }
            }

            if (sAMObject is AnalyticalModel)
            {
                sAMObject = new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster);
            }
            else if (sAMObject is AdjacencyCluster)
            {
                sAMObject = new AdjacencyCluster(adjacencyCluster);
            }

            index = Params.IndexOfOutputParam("analytical");
            if (index != -1)
                dataAccess.SetData(index, sAMObject);

            index = Params.IndexOfOutputParam("zones");
            if (index != -1)
                dataAccess.SetDataList(index, zones_Result);
        }
    }
}