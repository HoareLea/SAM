using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;
using static SAM.Geometry.Rhino.ActiveSetting;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateInternalConditionList : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("01320998-9431-4d60-a1e2-4e1aed822f7f");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

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

                global::Grasshopper.Kernel.Parameters.Param_String @string = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "iCnames_", NickName = "iCnames_", Description = "New Internal Condition Names to allow name change", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(@string, ParamVisibility.Binding));

                GooProfileParam gooProfileParam = new GooProfileParam() { Name = "gainProfiles_", NickName = "gainProfiles_", Description = "SAM Analytical Profiles for gains, in not provided default 24h profile: 8to18 profile will be used fro all gains", Access = GH_ParamAccess.list, Optional = true};
                result.Add(new GH_SAMParam(gooProfileParam, ParamVisibility.Binding));

                GooDegreeOfActivityParam gooDegreeOfActivityParam = new GooDegreeOfActivityParam() { Name = "degreeOfActivities_", NickName = "degreeOfActivities_", Description = "SAM Analytical DegreeOfActivities, default: 'Moderate office work' Sens=75 & Lat=55 W/person", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(gooDegreeOfActivityParam, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number = null;

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "areasPerPerson_", NickName = "areasPerPerson_", Description = "Areas Per Person, default 10 m2/person", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "occupancies_", NickName = "occupancies_", Description = "Occupancies (Number of People) will overide _areaPerPerson_ ", Access = GH_ParamAccess.list, Optional = true};
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "lightingGainsPerArea_", NickName = "lightingGainsPerArea_", Description = "Lighting Gains Per Area at specified profiles, default 8 W/m2", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "lightingLevels_", NickName = "lightingLevels_", Description = "Lighting Levels", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "equipmentSensGainsPerArea_", NickName = "equipmentSensGainsPerArea_", Description = "Equipment Sensible Gains Per Area at specified profiles, default 15 W/m2", Access = GH_ParamAccess.list, Optional = true};
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "equipmentLatGainsPerAreas_", NickName = "equipmentLatGainsPerAreas_", Description = "Equipment Latent Gains Per Area at specified profiles, default 0 W/m2", Access = GH_ParamAccess.list, Optional = true};
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_GenericObject genericObject = null;

                genericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "heatingSetPoints_", NickName = "heatingSetPoints_", Description = "Heating SetPoints, default Constant 21 degC \n* Input as Values or Profiles", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(genericObject, ParamVisibility.Binding));

                genericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "coolingSetPoints_", NickName = "coolingSetPoints_", Description = "Cooling SetPoints, default Constant 24 degC\n* Input as Values or Profiles", Access = GH_ParamAccess.list, Optional = true};
                result.Add(new GH_SAMParam(genericObject, ParamVisibility.Binding));

                genericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "humidificationSetPoints_", NickName = "humidificationSetPoints_", Description = "Humidification SetPoints, default Constant 40%\n* Input as Values or Profiles", Access = GH_ParamAccess.list, Optional = true};
                result.Add(new GH_SAMParam(genericObject, ParamVisibility.Binding));

                genericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "dehumidificationSetPoints_", NickName = "dehumidificationSetPoints_", Description = "Dehumidification SetPoints, default Constant 60%\n* Input as Values or Profiles", Access = GH_ParamAccess.list, Optional = true};
                result.Add(new GH_SAMParam(genericObject, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "infiltrationsACH_", NickName = "infiltrationsACH_", Description = "Infiltrations, default Constant 0.2 ac/h for spaces with external Panels", Access = GH_ParamAccess.list, Optional = true};
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "pollutantGsPerHrPerPerson_", NickName = "pollutantGsPerHrPerPerson_", Description = "Pollutant Generations Per Hour Per Person using Profile, default 37.5 g/hr/person see https://www.irbnet.de/daten/iconda/CIB6974.pdf]", Access = GH_ParamAccess.list, Optional = true };
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() {Name = "analyticalModel", NickName = "analyticalModel", Description = "SAM Analytical Model with ", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooInternalConditionParam() { Name = "internalConditions", NickName = "internalConditions", Description = "SAM Analytical InternalConditions", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMAnalyticalUpdateInternalConditionList()
          : base("SAMAnalytical.UpdateInternalConditionList", "SAMAnalytical.UpdateInternalConditionList",
              "Updates InternalConditions (IC) Properties for Spaces or Add new IC if is not included. \nIf nothing connected - default type S37_OfficeCell will be assign.\n This includes 8to18 Profile for gains and Constrant profile for SetPoints",
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
                {
                    spaces = null;
                }
            }

            if (spaces == null)
            {
                spaces = analyticalModel.GetSpaces();
            }

            List<Profile> profiles_Gain = new List<Profile>(); 
            index = Params.IndexOfInputParam("gainProfiles_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, profiles_Gain);
            }

            List<DegreeOfActivity> degreeOfActivities = new List<DegreeOfActivity>();
            index = Params.IndexOfInputParam("degreeOfActivities_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, degreeOfActivities);
            }

            List<double> areasPerPerson = new List<double>();
            index = Params.IndexOfInputParam("areasPerPerson_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, areasPerPerson);
            }

            List<double> occupancies = new List<double>();
            index = Params.IndexOfInputParam("occupancies_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, occupancies);
            }

            List<double> lightingGainsPerArea = new List<double>();
            index = Params.IndexOfInputParam("lightingGainsPerArea_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, lightingGainsPerArea);
            }

            List<double> lightingLevels = new List<double>();
            index = Params.IndexOfInputParam("lightingLevels_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, lightingLevels);
            }

            List<double> equipmentSensGainsPerArea = new List<double>();
            index = Params.IndexOfInputParam("equipmentSensGainsPerArea_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, equipmentSensGainsPerArea);
            }

            List<double> equipmentLatGainsPerArea = new List<double>();
            index = Params.IndexOfInputParam("equipmentLatGainsPerAreas_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, equipmentLatGainsPerArea);
            }

            List<GH_ObjectWrapper> objectWrappers = null;

            objectWrappers = new List<GH_ObjectWrapper>();
            List<Profile> profiles_Heating = new List<Profile>();
            index = Params.IndexOfInputParam("heatingSetPoints_");
            if (index != -1 && dataAccess.GetDataList(index, objectWrappers))
            {
                foreach(GH_ObjectWrapper objectWrapper in objectWrappers)
                {
                    object value = objectWrapper.Value;
                    if (value is IGH_Goo)
                    {
                        value = (value as dynamic).Value;
                    }

                    Profile profile = null;

                    if (value is Profile)
                    {
                        profile = (Profile)value;
                    }
                    else if (Core.Query.TryConvert(value, out double @double))
                    {
                        profile = double.IsNaN(@double) ? null : new Profile(string.Format("HTG_1to24_{0}", @double), @double, ProfileGroup.Thermostat);
                    }

                    if (profile == null)
                    {
                        continue;
                    }

                    profiles_Heating.Add(profile);
                }
            }

            objectWrappers = new List<GH_ObjectWrapper>();
            List<Profile> profiles_Cooling = new List<Profile>();
            index = Params.IndexOfInputParam("coolingSetPoints_");
            if (index != -1 && dataAccess.GetDataList(index, objectWrappers))
            {
                foreach (GH_ObjectWrapper objectWrapper in objectWrappers)
                {
                    object value = objectWrapper.Value;
                    if (value is IGH_Goo)
                    {
                        value = (value as dynamic).Value;
                    }

                    Profile profile = null;

                    if (value is Profile)
                    {
                        profile = (Profile)value;
                    }
                    else if (Core.Query.TryConvert(value, out double @double))
                    {
                        profile = double.IsNaN(@double) ? null : new Profile(string.Format("CLG_1to24_{0}", @double), @double, ProfileGroup.Thermostat);
                    }

                    if (profile == null)
                    {
                        continue;
                    }

                    profiles_Cooling.Add(profile);
                }
            }

            objectWrappers = new List<GH_ObjectWrapper>();
            List<Profile> profiles_Humidification = new List<Profile>();
            index = Params.IndexOfInputParam("humidificationSetPoints_");
            if (index != -1 && dataAccess.GetDataList(index, objectWrappers))
            {
                foreach (GH_ObjectWrapper objectWrapper in objectWrappers)
                {
                    object value = objectWrapper.Value;
                    if (value is IGH_Goo)
                    {
                        value = (value as dynamic).Value;
                    }

                    Profile profile = null;

                    if (value is Profile)
                    {
                        profile = (Profile)value;
                    }
                    else if (Core.Query.TryConvert(value, out double @double))
                    {
                        profile = double.IsNaN(@double) ? null : new Profile(string.Format("HUM_1to24_{0}", @double), @double, ProfileGroup.Humidistat);
                    }

                    if (profile == null)
                    {
                        continue;
                    }

                    profiles_Humidification.Add(profile);
                }
            }

            objectWrappers = new List<GH_ObjectWrapper>();
            List<Profile> profiles_Dehumidification = new List<Profile>();
            index = Params.IndexOfInputParam("dehumidificationSetPoints_");
            if (index != -1 && dataAccess.GetDataList(index, objectWrappers))
            {
                foreach (GH_ObjectWrapper objectWrapper in objectWrappers)
                {
                    object value = objectWrapper.Value;
                    if (value is IGH_Goo)
                    {
                        value = (value as dynamic).Value;
                    }

                    Profile profile = null;

                    if (value is Profile)
                    {
                        profile = (Profile)value;
                    }
                    else if (Core.Query.TryConvert(value, out double @double))
                    {
                        profile = double.IsNaN(@double) ? null : new Profile(string.Format("DHU_1to24_{0}", @double), @double, ProfileGroup.Humidistat);
                    }

                    if (profile == null)
                    {
                        continue;
                    }

                    profiles_Dehumidification.Add(profile);
                }
            }

            List<double> infiltrations = new List<double>();
            index = Params.IndexOfInputParam("infiltrationsACH_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, infiltrations);
            }

            List<string> names = new List<string>();
            index = Params.IndexOfInputParam("iCnames_");
            if(index != -1)
            {
                dataAccess.GetDataList(index, names);
            }

            List<double> pollutantGenerationsPerPerson = new List<double>();
            index = Params.IndexOfInputParam("pollutantGsPerHrPerPerson_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, pollutantGenerationsPerPerson);
            }

            Profile profile_Infiltartion = null;
            if(infiltrations != null || infiltrations.Count != 0)
            {
                ProfileLibrary profileLibrary_Default = ActiveSetting.Setting.GetValue<ProfileLibrary>(AnalyticalSettingParameter.DefaultProfileLibrary);
                profile_Infiltartion = profileLibrary_Default.GetProfile("Constant", ProfileGroup.Gain, true);
            }
                           
            ProfileLibrary profileLibrary = analyticalModel.ProfileLibrary;

            if (profile_Infiltartion != null)
            {
                profileLibrary.Add(profile_Infiltartion);
            }

            if (profiles_Gain != null)
            {
                profiles_Gain.ForEach(x => profileLibrary.Add(x));
            }

            if (profiles_Heating != null)
            {
                profiles_Heating.ForEach(x => profileLibrary.Add(x));
            }

            if (profiles_Cooling != null)
            {
                profiles_Cooling.ForEach(x => profileLibrary.Add(x));
            }

            if (profiles_Humidification != null)
            {
                profiles_Humidification.ForEach(x => profileLibrary.Add(x));
            }

            if (profiles_Dehumidification != null)
            {
                profiles_Dehumidification.ForEach(x => profileLibrary.Add(x));
            }

            List<InternalCondition> internalConditions = new List<InternalCondition>();

            for (int i = 0; i < spaces.Count; i++)
            {
                Space space_Temp = spaces[i] == null ? null : adjacencyCluster.SAMObject<Space>(spaces[i].Guid);
                if (space_Temp == null)
                    continue;

                InternalCondition internalCondition = space_Temp.InternalCondition;
                if (internalCondition == null)
                {
                    internalCondition = new InternalCondition(space_Temp.Name);
                }

                if (names != null && names.Count != 0)
                {
                    internalCondition = new InternalCondition(names.Count > i ? names[i] : names.Last(), Guid.NewGuid(), internalCondition);
                }

                if (profiles_Gain != null && profiles_Gain.Count != 0)
                {
                    Profile profile = profiles_Gain.Count > i ? profiles_Gain[i] : profiles_Gain.Last();

                    internalCondition.SetValue(InternalConditionParameter.OccupancyProfileName, profile.Name);
                    internalCondition.SetValue(InternalConditionParameter.LightingProfileName, profile.Name);
                    internalCondition.SetValue(InternalConditionParameter.EquipmentSensibleProfileName, profile.Name);
                    internalCondition.SetValue(InternalConditionParameter.EquipmentLatentProfileName, profile.Name);
                }

                if (pollutantGenerationsPerPerson != null && pollutantGenerationsPerPerson.Count != 0)
                {
                    double value = pollutantGenerationsPerPerson.Count > i ? pollutantGenerationsPerPerson[i] : pollutantGenerationsPerPerson.Last();
                    string name = internalCondition.GetValue<string>(InternalConditionParameter.OccupancyProfileName);
                    if(string.IsNullOrWhiteSpace(name))
                    {
                        name = value.ToString();
                    }
                    else
                    {
                        name = string.Format("{0}", name);
                    }

                    Profile profile = profileLibrary.GetProfile(name, ProfileType.Pollutant, true);
                    if(profile == null)
                    {
                        profile = internalCondition.GetProfile(ProfileType.Occupancy, analyticalModel.ProfileLibrary, true);
                        if(profile != null)
                        {
                            profile = new Profile(Guid.NewGuid(), profile, name, ProfileType.Pollutant.ToString());
                        }
                        else
                        {
                            profile = new Profile(name, ProfileType.Pollutant, new double[] { 1 });
                        }

                        profileLibrary.Add(profile);
                    }

                    internalCondition.SetValue(InternalConditionParameter.PollutantGenerationPerPerson, value);
                    internalCondition.SetValue(InternalConditionParameter.PollutantProfileName, profile.Name);
                }

                if (profiles_Heating != null && profiles_Heating.Count != 0)
                {
                    Profile profile = profiles_Heating.Count > i ? profiles_Heating[i] : profiles_Heating.Last();

                    internalCondition.SetValue(InternalConditionParameter.HeatingProfileName, profile.Name);
                }

                if (profiles_Cooling != null && profiles_Cooling.Count != 0)
                {
                    Profile profile = profiles_Cooling.Count > i ? profiles_Cooling[i] : profiles_Cooling.Last();

                    internalCondition.SetValue(InternalConditionParameter.CoolingProfileName, profile.Name);
                }

                if (profiles_Humidification != null && profiles_Humidification.Count != 0)
                {
                    Profile profile = profiles_Humidification.Count > i ? profiles_Humidification[i] : profiles_Humidification.Last();

                    internalCondition.SetValue(InternalConditionParameter.HumidificationProfileName, profile.Name);
                }

                if (profiles_Dehumidification != null && profiles_Dehumidification.Count != 0)
                {
                    Profile profile = profiles_Dehumidification.Count > i ? profiles_Dehumidification[i] : profiles_Dehumidification.Last();

                    internalCondition.SetValue(InternalConditionParameter.DehumidificationProfileName, profile.Name);
                }

                if (profile_Infiltartion != null)
                {
                    internalCondition.SetValue(InternalConditionParameter.InfiltrationProfileName, profile_Infiltartion.Name);
                }

                if (degreeOfActivities != null && degreeOfActivities.Count != 0)
                {
                    DegreeOfActivity degreeOfActivity = degreeOfActivities.Count > i ? degreeOfActivities[i] : degreeOfActivities.Last();

                    internalCondition.SetValue(InternalConditionParameter.OccupancySensibleGainPerPerson, degreeOfActivity.Sensible);
                    internalCondition.SetValue(InternalConditionParameter.OccupancyLatentGainPerPerson, degreeOfActivity.Latent);
                }

                if (occupancies != null && occupancies.Count != 0)
                {
                    double value = occupancies.Count > i ? occupancies[i] : occupancies.Last();

                    space_Temp.SetValue(SpaceParameter.Occupancy, value);
                    double area;
                    if (space_Temp.TryGetValue(SpaceParameter.Area, out area) && double.IsNaN(area) && value != 0)
                    {
                        internalCondition.SetValue(InternalConditionParameter.AreaPerPerson, area / value);
                    }
                }
                else
                {
                    if (areasPerPerson != null && areasPerPerson.Count != 0)
                    {
                        double value = areasPerPerson.Count > i ? areasPerPerson[i] : areasPerPerson.Last();

                        internalCondition.SetValue(InternalConditionParameter.AreaPerPerson, value);
                        double area;
                        if (space_Temp.TryGetValue(SpaceParameter.Area, out area) && double.IsNaN(area) && value != 0)
                        {
                            space_Temp.SetValue(SpaceParameter.Occupancy, area / value);
                        }
                    }
                }

                if (lightingGainsPerArea != null && lightingGainsPerArea.Count != 0)
                {
                    internalCondition.SetValue(InternalConditionParameter.LightingGainPerArea, lightingGainsPerArea.Count > i ? lightingGainsPerArea[i] : lightingGainsPerArea.Last());
                }

                if (lightingLevels != null && lightingLevels.Count != 0)
                {
                    internalCondition.SetValue(InternalConditionParameter.LightingLevel, lightingLevels.Count > i ? lightingLevels[i] : lightingLevels.Last());
                }

                if (equipmentLatGainsPerArea != null && equipmentLatGainsPerArea.Count != 0)
                {
                    internalCondition.SetValue(InternalConditionParameter.EquipmentLatentGainPerArea, equipmentLatGainsPerArea.Count > i ? equipmentLatGainsPerArea[i] : equipmentLatGainsPerArea.Last());
                }

                if (equipmentSensGainsPerArea != null && equipmentSensGainsPerArea.Count != 0)
                {
                    internalCondition.SetValue(InternalConditionParameter.EquipmentSensibleGainPerArea, equipmentSensGainsPerArea.Count > i ? equipmentSensGainsPerArea[i] : equipmentSensGainsPerArea.Last());
                }

                if (infiltrations != null && infiltrations.Count != 0)
                {
                    internalCondition.SetValue(InternalConditionParameter.InfiltrationAirChangesPerHour, infiltrations.Count > i ? infiltrations[i] : infiltrations.Last());
                }

                if (pollutantGenerationsPerPerson != null && pollutantGenerationsPerPerson.Count != 0)
                {
                    internalCondition.SetValue(InternalConditionParameter.PollutantGenerationPerPerson, pollutantGenerationsPerPerson.Count > i ? pollutantGenerationsPerPerson[i] : pollutantGenerationsPerPerson.Last());
                }

                space_Temp.InternalCondition = internalCondition;
                internalConditions.Add(internalCondition);
                adjacencyCluster.AddObject(space_Temp);
            }

            index = Params.IndexOfOutputParam("analyticalModel");
            if(index != -1)
            {
                analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster, analyticalModel.MaterialLibrary, profileLibrary);
                dataAccess.SetData(index, new GooAnalyticalModel(analyticalModel));
            }

            index = Params.IndexOfOutputParam("internalConditions");
            if (index != -1)
                dataAccess.SetDataList(index, internalConditions?.ConvertAll(x => new GooInternalCondition(x)));
        }
    }
}