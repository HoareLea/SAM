using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateInternalConditionByProfiles : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("79f69cd4-33f7-4f8c-bfe9-9f0f83faf990");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override GH_SAMParam[] Inputs
        {
            get
            {

                ProfileLibrary profileLibrary = ActiveSetting.Setting.GetValue<ProfileLibrary>(AnalyticalSettingParameter.DefaultProfileLibrary);
                DegreeOfActivityLibrary degreeOfActivityLibrary = ActiveSetting.Setting.GetValue<DegreeOfActivityLibrary>(AnalyticalSettingParameter.DefaultDegreeOfActivityLibrary);

                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM Analytical AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                GooSpaceParam gooSpaceParam = new GooSpaceParam() { Name = "_spaces", NickName = "_spaces", Description = "SAM Analytical Spaces. If not provided all Spaces from Analytical Model will be used and modified", Access = GH_ParamAccess.list};
                result.Add(new GH_SAMParam(gooSpaceParam, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String @string = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "ICname_", NickName = "ICname_", Description = "new Internal Condition Name to allow name change", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(@string, ParamVisibility.Binding));

                GooProfileParam gooProfileParam = new GooProfileParam() { Name = "_gainProfile_", NickName = "_gainProfile_", Description = "SAM Analytical Profile for gains, in not provided default 24h profile: 8to18 profile will be used fro all gains", Access = GH_ParamAccess.item};
                gooProfileParam.SetPersistentData(new GooProfile(profileLibrary.GetProfile("8to18", ProfileGroup.Gain, true)));
                result.Add(new GH_SAMParam(gooProfileParam, ParamVisibility.Binding));

                GooDegreeOfActivityParam gooDegreeOfActivityParam = new GooDegreeOfActivityParam() { Name = "_degreeOfActivity_", NickName = "_degreeOfActivity_", Description = "SAM Analytical DegreeOfActivity, default: 'Moderate office work' Sens=75 & Lat=55 W/person", Access = GH_ParamAccess.item};
                gooDegreeOfActivityParam.SetPersistentData(new GooDegreeOfActivity(degreeOfActivityLibrary.GetDegreeOfActivities("Moderate office work").FirstOrDefault()));
                result.Add(new GH_SAMParam(gooDegreeOfActivityParam, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number = null;

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_areaPerPerson_", NickName = "_areaPerPerson_", Description = "Area Per Person, default 10 m2/person", Access = GH_ParamAccess.item};
                number.SetPersistentData(10);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_occupancy_", NickName = "_occupancy_", Description = "Occupancy(Number of People) will overide _areaPerPerson_ ", Access = GH_ParamAccess.item, Optional = true};
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_lightingGainPerArea_", NickName = "_lightingGainPerArea_", Description = "Lighting Gain Per Area at specified profile, default 8 W/m2", Access = GH_ParamAccess.item, Optional = true };
                number.SetPersistentData(8);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_lightingLevel_", NickName = "_lightingLevel_", Description = "Lighting Level", Access = GH_ParamAccess.item};
                number.SetPersistentData(500);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_equipmentSensGainPerArea_", NickName = "_equipmentSensGainPerArea_", Description = "Equipment Sensible Gain Per Area at specified profile, default 15 W/m2", Access = GH_ParamAccess.item};
                number.SetPersistentData(15);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_equipmentLatGainPerArea_", NickName = "_equipmentLatGainPerArea_", Description = "Equipment Latent Gain Per Area at specified profile, default 0 W/m2", Access = GH_ParamAccess.item};
                number.SetPersistentData(0);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_heatingSetPoint_", NickName = "_heatingSetPoint_", Description = "Heating SetPoint, default Constant 21 degC", Access = GH_ParamAccess.item };
                number.SetPersistentData(21);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_coolingSetPoint_", NickName = "_coolingSetPoint_", Description = "Cooling SetPoint, default Constant 24 degC", Access = GH_ParamAccess.item};
                number.SetPersistentData(24);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_humidificationSetPoint_", NickName = "_humidificationSetPoint_", Description = "Humidification SetPoint, default Constant 40%", Access = GH_ParamAccess.item};
                number.SetPersistentData(40);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_dehumidificationSetPoint_", NickName = "_dehumidificationSetPoint_", Description = "Dehumidification SetPoint, default Constant 60%", Access = GH_ParamAccess.item};
                number.SetPersistentData(60);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "infiltrationACH_", NickName = "_infiltrationACH_", Description = "Infiltration, default Constant 0.2 ac/h for spaces with external Panels", Access = GH_ParamAccess.item, Optional = true};
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_pollutantGPerHrPerPerson_", NickName = "_pollutantGPerHrPerPerson_", Description = "Pollutant Generation Per Hour Per Person using Profile, default 37.5 g/hr/person see https://www.irbnet.de/daten/iconda/CIB6974.pdf]", Access = GH_ParamAccess.item };
                number.SetPersistentData(37.5);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() {Name = "AnalyticalModel", NickName = "AnalyticalModel", Description = "SAM Analytical Model with ", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooInternalConditionParam() { Name = "InternalConditions", NickName = "InternalConditions", Description = "SAM Analytical InternalConditions", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMAnalyticalCreateInternalConditionByProfiles()
          : base("SAMAnalytical.CreateInternalConditionByProfiles", "SAMAnalytical.CreateInternalConditionByProfiles",
              "Creates InternalCondition(IC) Properties for Spaces or Add new IC if is not included. \nIf nothing connected - default type S37_OfficeCell will be assign.\n This includes 8to18 Profile for gains and Constrant profile for SetPoints",
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
            index = Params.IndexOfInputParam("_spaces");
            if(index != -1)
            {
                spaces = new List<Space>();
                dataAccess.GetDataList(index, spaces);
                if (spaces != null && spaces.Count == 0)
                    spaces = null;
            }

            if (spaces == null)
                spaces = analyticalModel.GetSpaces();

            Profile profile_Gain = null;
            index = Params.IndexOfInputParam("_gainProfile_");
            if (index != -1)
                dataAccess.GetData(index, ref profile_Gain);

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

            double infiltration = double.NaN;
            index = Params.IndexOfInputParam("infiltrationACH_");
            if (index != -1)
                dataAccess.GetData(index, ref infiltration);

            string name = null;
            index = Params.IndexOfInputParam("ICname_");
            if(index != -1)
                dataAccess.GetData(index, ref name);

            double pollutantGenerationPerPerson = double.NaN;
            index = Params.IndexOfInputParam("_pollutantGPerHrPerPerson_");
            if (index != -1)
                dataAccess.GetData(index, ref pollutantGenerationPerPerson);

            if (double.IsNaN(infiltration))
            {
                infiltration = 0;

                List<Panel> panels = adjacencyCluster.GetPanels();
                if(panels != null && panels.Count != 0)
                {
                    foreach(Panel panel in panels)
                    {
                        List<Space> spaces_Adjacent = adjacencyCluster.GetSpaces(panel);
                        if(spaces_Adjacent != null && spaces_Adjacent.Count == 1)
                        {
                            infiltration = 0.2;
                            break;
                        }
                    }
                }
            }

            Profile profile_Heating = new Profile(string.Format("HTG_1to24_{0}", heatingSetPoint), heatingSetPoint, ProfileGroup.Thermostat);
            Profile profile_Cooling = new Profile(string.Format("CLG_1to24_{0}", coolingSetPoint), coolingSetPoint, ProfileGroup.Thermostat);

            Profile profile_Humidification = new Profile(string.Format("HUM_1to24_{0}", humidificationSetPoint), humidificationSetPoint, ProfileGroup.Humidistat);
            Profile profile_Dehumidification = new Profile(string.Format("DHU_1to24_{0}", dehumidificationSetPoint), dehumidificationSetPoint, ProfileGroup.Humidistat);

            Profile profile_Infiltartion = null;
            if(!double.IsNaN(infiltration) && infiltration != 0)
            {
                ProfileLibrary profileLibrary_Default = ActiveSetting.Setting.GetValue<ProfileLibrary>(AnalyticalSettingParameter.DefaultProfileLibrary);
                profile_Infiltartion = profileLibrary_Default.GetProfile("Constant", ProfileGroup.Gain, true);
            }
                           
            ProfileLibrary profileLibrary = analyticalModel.ProfileLibrary;

            if (profile_Gain != null)
                profileLibrary.Add(profile_Gain);

            if (profile_Heating != null)
                profileLibrary.Add(profile_Heating);

            if (profile_Cooling != null)
                profileLibrary.Add(profile_Cooling);

            if (profile_Humidification != null)
                profileLibrary.Add(profile_Humidification);

            if (profile_Dehumidification != null)
                profileLibrary.Add(profile_Dehumidification);

            if (profile_Infiltartion != null)
                profileLibrary.Add(profile_Infiltartion);

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

                if (!string.IsNullOrEmpty(name))
                    internalCondition = new InternalCondition(name, Guid.NewGuid(), internalCondition);

                if (profile_Gain != null)
                {
                    internalCondition.SetValue(InternalConditionParameter.OccupancyProfileName, profile_Gain.Name);
                    internalCondition.SetValue(InternalConditionParameter.LightingProfileName, profile_Gain.Name);
                    internalCondition.SetValue(InternalConditionParameter.EquipmentSensibleProfileName, profile_Gain.Name);
                    internalCondition.SetValue(InternalConditionParameter.EquipmentLatentProfileName, profile_Gain.Name);

                    if(!double.IsNaN(pollutantGenerationPerPerson))
                        internalCondition.SetValue(InternalConditionParameter.PollutantProfileName, profile_Gain.Name);
                }

                if(profile_Heating != null)
                    internalCondition.SetValue(InternalConditionParameter.HeatingProfileName, profile_Heating.Name);

                if (profile_Cooling != null)
                    internalCondition.SetValue(InternalConditionParameter.CoolingProfileName, profile_Cooling.Name);

                if (profile_Humidification != null)
                    internalCondition.SetValue(InternalConditionParameter.HumidificationProfileName, profile_Humidification.Name);

                if (profile_Dehumidification != null)
                    internalCondition.SetValue(InternalConditionParameter.DehumidificationProfileName, profile_Dehumidification.Name);

                if(profile_Infiltartion != null)
                    internalCondition.SetValue(InternalConditionParameter.InfiltrationProfileName, profile_Infiltartion.Name);

                if (degreeOfActivity != null)
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

                if (!double.IsNaN(infiltration))
                    internalCondition.SetValue(InternalConditionParameter.InfiltrationAirChangesPerHour, infiltration);

                if (!double.IsNaN(pollutantGenerationPerPerson))
                    internalCondition.SetValue(InternalConditionParameter.PollutantGenerationPerPerson, pollutantGenerationPerPerson);

                space_Temp.InternalCondition = internalCondition;
                internalConditions.Add(internalCondition);
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