using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateNCM : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e0cc7bad-fcad-4bc4-b2ce-1604ec78fe6a");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM Analytical AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_spaces_", NickName = "_spaces_", Description = "SAM Analytical Spaces or Zones, if nothing connected all spaces from AnalyticalModel will be used", Access = GH_ParamAccess.list, Optional = true}, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "name_", NickName = "name_", Description = "NCM Name\n *Type under inspect of NCM Data in Internal Condition", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "systemType_", NickName = "systemType_", Description = "System Type", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "lightingOccupancyControls_", NickName = "lightingOccupancyControls_", Description = "Lighting Occupancy Controls", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "lightingPhotoelectricControls_", NickName = "lightingPhotoelectricControls_", Description = "Lighting Photoelectric Controls", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "lightingPhotoelectricBackSpaceSensor_", NickName = "lightingPhotoelectricBackSpaceSensor_", Description = "Lighting Photoelectric Back Space Sensor", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "lightingPhotoelectricControlsTimeSwitch_", NickName = "lightingPhotoelectricControlsTimeSwitch_", Description = "Lighting Photoelectric Controls Time Switch", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "lightingDaylightFactorMethod_", NickName = "lightingDaylightFactorMethod_", Description = "Lighting Daylight Factor Method", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "isMainsGasAvailable_", NickName = "isMainsGasAvailable_", Description = "Is Mains Gas Available", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "lightingPhotoelectricParasiticPower_", NickName = "lightingPhotoelectricParasiticPower_", Description = "Lighting Photoelectric Parasitic Power", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "airPermeability_", NickName = "airPermeability_", Description = "Air Permeability", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_description_", NickName = "_description_", Description = "Description", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));

                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() {Name = "analyticalModel", NickName = "analyticalModel", Description = "SAM Analytical AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "spaces", NickName = "spaces", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMAnalyticalUpdateNCM()
          : base("SAMAnalytical.UpdateNCM", "SAMAnalytical.UpdateNCM",
              "Updates NCM Data",
              "SAM", "SAM_IC")
        {
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            index = Params.IndexOfInputParam("_analyticalModel");
            if(index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AnalyticalModel analyticalModel = null;
            if (!dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;

            List<Space> spaces = null;

            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            index = Params.IndexOfInputParam("_spaces_");
            if(index != -1)
            {
                spaces = new List<Space>();
                dataAccess.GetDataList(index, objectWrappers);
                if(objectWrappers != null && objectWrappers.Count != 0)
                {
                    foreach(GH_ObjectWrapper objectWrapper in objectWrappers)
                    {
                        if(objectWrapper.Value is Space)
                        {
                            spaces.Add((Space)objectWrapper.Value);
                        }
                        else if(objectWrapper.Value is Zone)
                        {
                            List<Space> spaces_Temp = adjacencyCluster.GetRelatedObjects<Space>((Zone)objectWrapper.Value);
                            if(spaces_Temp != null && spaces_Temp.Count != 0)
                            {
                                spaces.AddRange(spaces_Temp);
                            }
                        }
                    }
                }
                if (spaces != null && spaces.Count == 0)
                {
                    spaces = null;
                }
            }

            if (spaces == null)
            {
                spaces = analyticalModel.GetSpaces();
            }

            string name = null;
            index = Params.IndexOfInputParam("name_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref name);
            }

            NCMSystemType? nCMSystemType = null;
            index = Params.IndexOfInputParam("systemType_");
            if (index != -1)
            {
                string systemTypeName = null;
                if (dataAccess.GetData(index, ref systemTypeName) && !string.IsNullOrWhiteSpace(systemTypeName))
                {
                    if (Core.Query.TryGetEnum(systemTypeName, out NCMSystemType nCMSystemType_Temp))
                    {
                        nCMSystemType = nCMSystemType_Temp;
                    }
                }
            }

            LightingOccupancyControls? lightingOccupancyControls = null;
            index = Params.IndexOfInputParam("lightingOccupancyControls_");
            if (index != -1)
            {
                string lightingOccupancyControlsName = null;
                if (dataAccess.GetData(index, ref lightingOccupancyControlsName) && !string.IsNullOrWhiteSpace(lightingOccupancyControlsName))
                {
                    if (Core.Query.TryGetEnum(lightingOccupancyControlsName, out LightingOccupancyControls lightingOccupancyControls_Temp))
                    {
                        lightingOccupancyControls = lightingOccupancyControls_Temp;
                    }
                }
            }

            LightingPhotoelectricControls? lightingPhotoelectricControls = null;
            index = Params.IndexOfInputParam("lightingPhotoelectricControls_");
            if (index != -1)
            {
                string lightingPhotoelectricControlsName = null;
                if (dataAccess.GetData(index, ref lightingPhotoelectricControlsName) && !string.IsNullOrWhiteSpace(lightingPhotoelectricControlsName))
                {
                    if (Core.Query.TryGetEnum(lightingPhotoelectricControlsName, out LightingPhotoelectricControls lightingPhotoelectricControls_Temp))
                    {
                        lightingPhotoelectricControls = lightingPhotoelectricControls_Temp;
                    }
                }
            }

            bool? lightingPhotoelectricBackSpaceSensor = null;
            index = Params.IndexOfInputParam("lightingPhotoelectricBackSpaceSensor_");
            if (index != -1)
            {
                bool lightingPhotoelectricBackSpaceSensor_Temp = false;
                if (dataAccess.GetData(index, ref lightingPhotoelectricBackSpaceSensor_Temp))
                {
                    lightingPhotoelectricBackSpaceSensor = lightingPhotoelectricBackSpaceSensor_Temp;
                }
            }

            bool? lightingPhotoelectricControlsTimeSwitch = null;
            index = Params.IndexOfInputParam("lightingPhotoelectricControlsTimeSwitch_");
            if (index != -1)
            {
                bool lightingPhotoelectricControlsTimeSwitch_Temp = false;
                if (dataAccess.GetData(index, ref lightingPhotoelectricControlsTimeSwitch_Temp))
                {
                    lightingPhotoelectricControlsTimeSwitch = lightingPhotoelectricControlsTimeSwitch_Temp;
                }
            }

            bool? lightingDaylightFactorMethod = null;
            index = Params.IndexOfInputParam("lightingDaylightFactorMethod_");
            if (index != -1)
            {
                bool lightingDaylightFactorMethod_Temp = false;
                if (dataAccess.GetData(index, ref lightingDaylightFactorMethod_Temp))
                {
                    lightingDaylightFactorMethod = lightingDaylightFactorMethod_Temp;
                }
            }

            bool? isMainsGasAvailable = null;
            index = Params.IndexOfInputParam("isMainsGasAvailable_");
            if (index != -1)
            {
                bool isMainsGasAvailable_Temp = false;
                if (dataAccess.GetData(index, ref isMainsGasAvailable_Temp))
                {
                    isMainsGasAvailable = isMainsGasAvailable_Temp;
                }
            }

            double? lightingPhotoelectricParasiticPower = null;
            index = Params.IndexOfInputParam("lightingPhotoelectricParasiticPower_");
            if (index != -1)
            {
                double lightingPhotoelectricParasiticPower_Temp = double.NaN;
                if (dataAccess.GetData(index, ref lightingPhotoelectricParasiticPower_Temp))
                {
                    lightingPhotoelectricParasiticPower = lightingPhotoelectricParasiticPower_Temp;
                }
            }

            double? airPermeability = null;
            index = Params.IndexOfInputParam("airPermeability_");
            if (index != -1)
            {
                double airPermeability_Temp = double.NaN;
                if (dataAccess.GetData(index, ref airPermeability_Temp))
                {
                    airPermeability = airPermeability_Temp;
                }
            }


            string description = null;
            index = Params.IndexOfInputParam("_description_");
            if (index != -1)
            {
                string description_Temp = null;
                if (dataAccess.GetData(index, ref description_Temp))
                {
                    description = description_Temp;
                }
            }

            List<Space> spaces_Result = new List<Space>();
            foreach(Space space in spaces)
            {
                if(space == null)
                {
                    continue;
                }

                Space space_Temp = new Space(space);

                InternalCondition internalCondition = space_Temp.InternalCondition;
                if(internalCondition == null)
                {
                    continue;
                }

                internalCondition = new InternalCondition(internalCondition);

                if (!internalCondition.TryGetValue(InternalConditionParameter.NCMData, out NCMData nCMData) || nCMData == null)
                {
                    continue;
                }

                nCMData = new NCMData(nCMData);

                if (!string.IsNullOrWhiteSpace(name))
                {
                    nCMData.Name = name;
                }

                if(nCMSystemType != null && nCMSystemType.HasValue)
                {
                    nCMData.SystemType = nCMSystemType.Value;
                }

                if (lightingOccupancyControls != null && lightingOccupancyControls.HasValue)
                {
                    nCMData.LightingOccupancyControls = lightingOccupancyControls.Value;
                }

                if (lightingOccupancyControls != null && lightingOccupancyControls.HasValue)
                {
                    nCMData.LightingOccupancyControls = lightingOccupancyControls.Value;
                }

                if(lightingPhotoelectricBackSpaceSensor != null && lightingPhotoelectricBackSpaceSensor.HasValue)
                {
                    nCMData.LightingPhotoelectricBackSpaceSensor = lightingPhotoelectricBackSpaceSensor.Value;
                }

                if (lightingPhotoelectricControlsTimeSwitch != null && lightingPhotoelectricControlsTimeSwitch.HasValue)
                {
                    nCMData.LightingPhotoelectricControlsTimeSwitch = lightingPhotoelectricControlsTimeSwitch.Value;
                }

                if (lightingDaylightFactorMethod != null && lightingDaylightFactorMethod.HasValue)
                {
                    nCMData.LightingDaylightFactorMethod = lightingDaylightFactorMethod.Value;
                }

                if (isMainsGasAvailable != null && isMainsGasAvailable.HasValue)
                {
                    nCMData.IsMainsGasAvailable = isMainsGasAvailable.Value;
                }

                if (lightingPhotoelectricParasiticPower != null && lightingPhotoelectricParasiticPower.HasValue)
                {
                    nCMData.LightingPhotoelectricParasiticPower = lightingPhotoelectricParasiticPower.Value;
                }

                if (airPermeability != null && airPermeability.HasValue)
                {
                    nCMData.AirPermeability = airPermeability.Value;
                }

                if(description != null)
                {
                    nCMData.Description = description;
                }

                internalCondition.SetValue(InternalConditionParameter.NCMData, nCMData);

                space_Temp.InternalCondition = internalCondition;

                adjacencyCluster.AddObject(space_Temp);
                spaces_Result.Add(space_Temp);
            }

            index = Params.IndexOfOutputParam("analyticalModel");
            if(index != -1)
            {
                analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster, analyticalModel.MaterialLibrary, analyticalModel.ProfileLibrary);
                dataAccess.SetData(index, new GooAnalyticalModel(analyticalModel));
            }

            index = Params.IndexOfOutputParam("spaces");
            if (index != -1)
                dataAccess.SetDataList(index, spaces_Result?.ConvertAll(x => new GooSpace(x)));
        }
    }
}