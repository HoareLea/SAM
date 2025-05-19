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
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateZonesByValues()
          : base("SAMAnalytical.CreateZonesByValues", "SAMAnalytical.CreateZonesByValues",
              "Create Zones By Values",
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam { Name = "_spaces", NickName = "_spaces", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject { Name = "_values", NickName = "_values", Description = "Values", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String param_String = null;

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String { Name = "_zoneType", NickName = "_zoneType", Description = "SAM Analytical ZoneType", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String { Name = "zoneCategoryName_", NickName = "zoneCategoryName_", Description = "Zone Category Name. ZoneType parameter will be ignored when zoneCategory name applied", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean boolean = null;
                boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean { Name = "cleanSpacesByZoneCategory_", NickName = "cleanSpacesByZoneCategory_", Description = "Clean Spaces By Zone Category", Access = GH_ParamAccess.item, Optional = true };
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "analytical", NickName = "analytical", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "zones", NickName = "zones", Description = "SAM GuidCollection representing Zones", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
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
            if(index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
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

            index = Params.IndexOfInputParam("_values");
            List<object> values = new List<object>();
            if (index == -1 || !dataAccess.GetDataList(index, values) || values == null || values.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            for(int i =0; i < values.Count; i++)
            {
                if (values[i] is IGH_Goo)
                {
                    values[i] = (values[i] as dynamic).Value;
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

            if(zoneType == ZoneType.Undefined && zoneCategory ==null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            if(sAMObject is AnalyticalModel)
            {
                AnalyticalModel analyticalModel = new AnalyticalModel((AnalyticalModel)sAMObject);
                adjacencyCluster = analyticalModel.AdjacencyCluster;
            }
            else if(sAMObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);
            }

            if(adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<string> values_String = values.ConvertAll(x => x?.ToString().Trim());
            
            List<string> names = values_String.Distinct().ToList();
            names.RemoveAll(x => string.IsNullOrWhiteSpace(x));

            if(zoneType != ZoneType.Undefined && string.IsNullOrWhiteSpace(zoneCategory))
            {
                zoneCategory = zoneType.Text();
            }

            List<Zone> zones_Result = new List<Zone>();
            foreach (string name_Temp in names)
            {
                if(cleanSpacesByZoneCategory)
                {
                    Zone zone_Temp = adjacencyCluster.GetZones()?.Find(x => x?.Name == name_Temp && x.GetValue<string>(ZoneParameter.ZoneCategory) == zoneCategory);
                    if(zone_Temp != null)
                    {
                        adjacencyCluster.RemoveObject<Zone>(zone_Temp.Guid);
                    }
                }
                
                List<int> indexes = values_String.IndexesOf(name_Temp);
                List<Space> spaces_Temp = indexes.ConvertAll(x => spaces[x]);
                Zone zone = Analytical.Modify.UpdateZone(adjacencyCluster, name_Temp, zoneCategory, spaces_Temp.ToArray());
                if(zone != null)
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