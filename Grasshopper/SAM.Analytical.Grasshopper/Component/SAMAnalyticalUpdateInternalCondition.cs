﻿using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateInternalCondition : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("79f69cd4-33f7-4f8c-bfe9-9f0f83faf990");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;

        protected override GH_SAMParam[] Inputs
        {
            get
            {

                ProfileLibrary profileLibrary = ActiveSetting.Setting.GetValue<ProfileLibrary>(AnalyticalSettingParameter.DefaultProfileLibrary);
                DegreeOfActivityLibrary degreeOfActivityLibrary = ActiveSetting.Setting.GetValue<DegreeOfActivityLibrary>(AnalyticalSettingParameter.DefaultDegreeOfActivityLibrary);

                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM Analytical AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                GooSpaceParam gooSpaceParam = new GooSpaceParam() { Name = "_spaces_", NickName = "_spaces_", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list};
                result.Add(new GH_SAMParam(gooSpaceParam, ParamVisibility.Binding));

                GooProfileParam gooProfileParam = new GooProfileParam() { Name = "_profile_", NickName = "_profile_", Description = "SAM Analytical Profile", Access = GH_ParamAccess.item};
                gooProfileParam.SetPersistentData(new GooProfile(profileLibrary.GetProfile("8to18", ProfileGroup.Gain, true)));
                result.Add(new GH_SAMParam(gooProfileParam, ParamVisibility.Binding));

                GooDegreeOfActivityParam gooDegreeOfActivityParam = new GooDegreeOfActivityParam() { Name = "_degreeOfActivity_", NickName = "_degreeOfActivity_", Description = "SAM Analytical DegreeOfActivity", Access = GH_ParamAccess.item};
                gooDegreeOfActivityParam.SetPersistentData(new GooDegreeOfActivity(degreeOfActivityLibrary.GetDegreeOfActivities("Moderate office work").FirstOrDefault()));
                result.Add(new GH_SAMParam(gooDegreeOfActivityParam, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number = null;

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_areaPerPerson_", NickName = "_areaPerPerson_", Description = "Area Per Person", Access = GH_ParamAccess.item};
                number.SetPersistentData(10);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_occupancy_", NickName = "_occupancy_", Description = "Occupancy", Access = GH_ParamAccess.item, Optional = true};
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_lightingGainPerArea_", NickName = "_lightingGainPerArea_", Description = "Lighting Gain Per Area", Access = GH_ParamAccess.item, Optional = true };
                number.SetPersistentData(8);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_lightingLevel_", NickName = "_lightingLevel_", Description = "Lighting Level", Access = GH_ParamAccess.item};
                number.SetPersistentData(500);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_equipmentSensGainPerArea_", NickName = "_equipmentSensGainPerArea_", Description = "Equipment Sensible Gain Per Area", Access = GH_ParamAccess.item};
                number.SetPersistentData(15);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_equipmentLatGainPerArea_", NickName = "_equipmentLatGainPerArea_", Description = "Equipment Latent Gain Per Area", Access = GH_ParamAccess.item};
                number.SetPersistentData(0);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_heatingSetPoint_", NickName = "_heatingSetPoint_", Description = "Heating SetPoint", Access = GH_ParamAccess.item };
                number.SetPersistentData(21);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_coolingSetPoint_", NickName = "_coolingSetPoint_", Description = "Cooling SetPoint", Access = GH_ParamAccess.item};
                number.SetPersistentData(24);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_humidificationSetPoint_", NickName = "_humidificationSetPoint_", Description = "Humidification SetPoint", Access = GH_ParamAccess.item};
                number.SetPersistentData(0);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_dehumidificationSetPoint_", NickName = "_dehumidificationSetPoint_", Description = "Dehumidification SetPoint", Access = GH_ParamAccess.item};
                number.SetPersistentData(100);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() {Name = "AnalyticalModel", NickName = "AnalyticalModel", Description = "SAM Analytical AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooInternalConditionParam() { Name = "InternalConditions", NickName = "InternalConditions", Description = "SAM Analytical InternalConditions", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMAnalyticalUpdateInternalCondition()
          : base("SAMAnalytical.UpdateInternalCondition", "SAMAnalytical.UpdateInternalCondition",
              "Updates InternalCondition Properties for Spaces",
              "SAM", "Analytical")
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
            index = Params.IndexOfInputParam("_spaces_");
            if(index != -1)
            {
                spaces = new List<Space>();
                dataAccess.GetDataList(index, spaces);
                if (spaces != null && spaces.Count == 0)
                    spaces = null;
            }

            if (spaces == null)
                spaces = analyticalModel.GetSpaces();

            Profile profile = null;
            index = Params.IndexOfInputParam("_profile_");
            if (index != -1)
                dataAccess.GetData(index, ref profile);

            DegreeOfActivity degreeOfActivity = null;
            index = Params.IndexOfInputParam("_degreeOfActivity_");
            if (index != -1)
                dataAccess.GetData(index, ref degreeOfActivity);

            double areaPerPerson = double.NaN;
            index = Params.IndexOfInputParam("_areaPerPerson_");
            if (index != -1)
                dataAccess.GetData(index, ref areaPerPerson);

            double occupancy = double.NaN;
            index = Params.IndexOfInputParam("_occupancy_");
            if (index != -1)
                dataAccess.GetData(index, ref occupancy);

            double lightingGainPerArea = double.NaN;
            index = Params.IndexOfInputParam("_lightingGainPerArea_");
            if (index != -1)
                dataAccess.GetData(index, ref lightingGainPerArea);

            double lightingLevel = double.NaN;
            index = Params.IndexOfInputParam("_lightingLevel_");
            if (index != -1)
                dataAccess.GetData(index, ref lightingLevel);

            double equipmentSensGainPerArea = double.NaN;
            index = Params.IndexOfInputParam("_equipmentSensGainPerArea_");
            if (index != -1)
                dataAccess.GetData(index, ref equipmentSensGainPerArea);

            double equipmentLatGainPerArea = double.NaN;
            index = Params.IndexOfInputParam("_equipmentLatGainPerArea_");
            if (index != -1)
                dataAccess.GetData(index, ref equipmentLatGainPerArea);

            double heatingSetPoint = double.NaN;
            index = Params.IndexOfInputParam("_heatingSetPoint_");
            if (index != -1)
                dataAccess.GetData(index, ref heatingSetPoint);

            double coolingSetPoint = double.NaN;
            index = Params.IndexOfInputParam("_coolingSetPoint_");
            if (index != -1)
                dataAccess.GetData(index, ref coolingSetPoint);

            double humidificationSetPoint = double.NaN;
            index = Params.IndexOfInputParam("_humidificationSetPoint_");
            if (index != -1)
                dataAccess.GetData(index, ref humidificationSetPoint);

            double dehumidificationSetPoint = double.NaN;
            index = Params.IndexOfInputParam("_dehumidificationSetPoint_");
            if (index != -1)
                dataAccess.GetData(index, ref dehumidificationSetPoint);


            ProfileLibrary profileLibrary = analyticalModel.ProfileLibrary;

            if (profile != null)
                profileLibrary.Add(profile);

            List<InternalCondition> internalConditions = new List<InternalCondition>();

            foreach(Space space in spaces)
            {
                if (space == null)
                    continue;

                Space space_Temp = adjacencyCluster.SAMObject<Space>(space.Guid);
                if (space_Temp == null)
                    continue;

                space_Temp = new Space(space_Temp);

                InternalCondition internalCondition = space_Temp.InternalCondition;
                if(internalCondition == null)
                    internalCondition = new InternalCondition(space_Temp.Name);

                if (profile != null)
                {
                    internalCondition.SetValue(InternalConditionParameter.OccupancyProfileName, profile.Name);
                    internalCondition.SetValue(InternalConditionParameter.LightingProfileName, profile.Name);
                    internalCondition.SetValue(InternalConditionParameter.EquipmentSensibleProfileName, profile.Name);
                    internalCondition.SetValue(InternalConditionParameter.EquipmentLatentProfileName, profile.Name);
                    internalCondition.SetValue(InternalConditionParameter.HeatingProfileName, profile.Name);
                    
                }

                if(degreeOfActivity != null)
                {
                    internalCondition.SetValue(InternalConditionParameter.OccupancySensibleGainPerPerson, degreeOfActivity.Sensible);
                    internalCondition.SetValue(InternalConditionParameter.OccupancyLatentGainPerPerson, degreeOfActivity.Latent);
                }

                if (double.IsNaN(occupancy))
                {
                    if (!double.IsNaN(areaPerPerson))
                    {
                        internalCondition.SetValue(InternalConditionParameter.AreaPerPerson, areaPerPerson);
                        double area;
                        if (space_Temp.TryGetValue(SpaceParameter.Area, out area) && double.IsNaN(area) && areaPerPerson != 0)
                            space_Temp.SetValue(SpaceParameter.Occupancy, area / areaPerPerson);
                    }
                }
                else
                {
                    space_Temp.SetValue(SpaceParameter.Occupancy, occupancy);
                    double area;
                    if (space_Temp.TryGetValue(SpaceParameter.Area, out area) && double.IsNaN(area) && occupancy != 0)
                        internalCondition.SetValue(InternalConditionParameter.AreaPerPerson, area / occupancy);
                }

                if (!double.IsNaN(lightingGainPerArea))
                    internalCondition.SetValue(InternalConditionParameter.LightingGainPerArea, lightingGainPerArea);

                if (!double.IsNaN(lightingLevel))
                    internalCondition.SetValue(InternalConditionParameter.LightingLevel, lightingLevel);

                if (!double.IsNaN(equipmentSensGainPerArea))
                    internalCondition.SetValue(InternalConditionParameter.EquipmentSensibleGainPerArea, equipmentSensGainPerArea);

                if (!double.IsNaN(equipmentLatGainPerArea))
                    internalCondition.SetValue(InternalConditionParameter.EquipmentLatentGainPerArea, equipmentLatGainPerArea);


                space_Temp.InternalCondition = internalCondition;
                adjacencyCluster.AddObject(space_Temp);
            }

            index = Params.IndexOfOutputParam("AnalyticalModel");
            if(index != -1)
            {
                analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster, analyticalModel.MaterialLibrary, profileLibrary);
                dataAccess.SetData(index, new GooAnalyticalModel(analyticalModel));
            }

            index = Params.IndexOfOutputParam("InternalConditions");
            if (index != -1)
                dataAccess.SetDataList(index, internalConditions?.ConvertAll(x => new GooInternalCondition(x)));
        }
    }
}