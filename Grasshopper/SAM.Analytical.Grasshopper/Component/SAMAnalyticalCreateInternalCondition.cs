using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateInternalCondition : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("29bd4dfe-fa7c-4c7d-a976-3fae5206fa6f");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.3";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateInternalCondition()
          : base("SAMAnalytical.CreateInternalCondition", "SAMAnalytical.CreateInternalCondition",
              "Create InternalCondition",
              "SAM", "Analytical")
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

                result.Add(new GH_SAMParam(new GooInternalConditionParam() { Name = "internalCondition_", NickName = "internalCondition_", Description = "Source SAM Analytical InternalCondition", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "name_", NickName = "name_", Description = "Internal Condition Name", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "areaPerPerson_", NickName = "areaPerPerson_", Description = "Area Per Person, default 10 m2/person", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "occupancyProfile_", NickName = "occupancyProfile_", Description = "Occupancy Profile", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "occupancySensibleGainPerPerson_", NickName = "occupancySensibleGainPerPerson_", Description = "Occupancy Sensible Gain Per Person [W/p]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "occupancyLatentGainPerPerson_", NickName = "occupancyLatentGainPerPerson_", Description = "Occupancy Latent Gain Per Person [W/p]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "equipmentSensibleProfile_", NickName = "equipmentSensibleProfile_", Description = "Equipment Sensible Profile Name", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "equipmentSensibleGain_", NickName = "equipmentSensibleGain_", Description = "Equipment Sensible Gain [W]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "equipmentSensibleGainPerArea_", NickName = "equipmentSensibleGainPerArea_", Description = "Equipment Sensible Gain Per Area [W/m2]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "equipmentSensibleGainPerPerson_", NickName = "equipmentSensibleGainPerPerson_", Description = "Equipment Sensible Gain Per Person [W/p]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "equipmentLatentProfile_", NickName = "equipmentLatentProfile_", Description = "Equipment Latent Profile Name", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "equipmentLatentGain_", NickName = "equipmentLatentGain_", Description = "Equipment Latent Gain [W]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "equipmentLatentGainPerArea_", NickName = "equipmentLatentGainPerArea_", Description = "Equipment Latent Gain Per Area [W/m2]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "lightingProfile_", NickName = "lightingProfile_", Description = "Lighting Profile Name", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "lightingGain_", NickName = "lightingGain_", Description = "Lighting Gain [W]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "lightingGainPerArea_", NickName = "lightingGainPerArea_", Description = "Lighting Gain Per Area [W/m2]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "lightingGainPerPerson_", NickName = "lightingGainPerPerson_", Description = "Lighting Gain Per Person [W/p]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "lightingLevel_", NickName = "lightingLevel_", Description = "Lighting Level [lux]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "lightingEfficiency_", NickName = "lightingEfficiency_", Description = "Lighting Efficiency [W/m²/100lux]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "infiltrationProfile_", NickName = "infiltrationProfile_", Description = "Infiltration Profile", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "infiltrationAirChangesPerHour_", NickName = "infiltrationAirChangesPerHour_", Description = "Infiltration Air Changes Per Hour [ACH]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "pollutantProfile_", NickName = "pollutantProfile_", Description = "Pollutant Profile", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "pollutantGenerationPerArea_", NickName = "pollutantGenerationPerArea_", Description = "Pollutant Generation Per Area [g/h/m2]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "pollutantGenerationPerPerson_", NickName = "pollutantGenerationPerPerson_", Description = "Pollutant Generation Per Person [g/h/p]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "heatingProfile_", NickName = "heatingProfile_", Description = "Heating Profile", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "heatingEmitterCoefficient_", NickName = "heatingEmitterCoefficient_", Description = "Heating Emitter Coefficient [0-1]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "heatingEmitterRadiantProportion_", NickName = "heatingEmitterRadiantProportion_", Description = "Heating Emitter Radiant Proportion [0-1]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "coolingProfile_", NickName = "coolingProfile_", Description = "Cooling Profile", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "coolingEmitterCoefficient_", NickName = "coolingEmitterCoefficient_", Description = "Cooling Emitter Coefficient [0-1]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "coolingEmitterRadiantProportion_", NickName = "coolingEmitterRadiantProportion_", Description = "Cooling Emitter Radiant Proportion [0-1]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "humidificationProfile_", NickName = "humidificationProfile_", Description = "Humidification Profile", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "dehumidificationProfile_", NickName = "dehumidificationProfile_", Description = "Dehumidification Profile", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "ventilationSystemTypeName_", NickName = "ventilationSystemTypeName_", Description = "Ventilation System Type Name", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "coolingSystemTypeName_", NickName = "coolingSystemTypeName_", Description = "Cooling System Type Name", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "heatingSystemTypeName_", NickName = "heatingSystemTypeName_", Description = "Heating System Type Name", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "ventilationProfile_", NickName = "ventilationProfile_", Description = "Ventilation Profile", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "supplyAirFlowPerPerson_", NickName = "supplyAirFlowPerPerson_", Description = "Supply Air Flow Per Person [m3/s/p]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "exhaustAirFlowPerPerson_", NickName = "exhaustAirFlowPerPerson_", Description = "Exhaust Air Flow Per Person [m3/s/p]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "supplyAirChangesPerHour_", NickName = "supplyAirChangesPerHour_", Description = "Supply Air Changes Per Hour [ACH]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "exhaustAirChangesPerHour_", NickName = "exhaustAirChangesPerHour_", Description = "Exhaust Air Changes Per Hour [ACH]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "supplyAirFlowPerArea_", NickName = "supplyAirFlowPerArea_", Description = "Supply Air Flow Per Area [m3/s/m2]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "exhaustAirFlowPerArea_", NickName = "exhaustAirFlowPerArea_", Description = "Exhaust Air Flow Per Area [m3/s/m2]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "supplyAirFlow_", NickName = "supplyAirFlow_", Description = "Supply Air Flow [m3/s]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "exhaustAirFlow_", NickName = "exhaustAirFlow_", Description = "Exhaust Air Flow [m3/s]]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "lightingRadiantProportion_", NickName = "lightingRadiantProportion_", Description = "Lighting Radiant Proportion [0-1]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "occupancyRadiantProportion_", NickName = "occupancyRadiantProportion_", Description = "Occupancy Radiant Proportion [0-1]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "equipmentRadiantProportion_", NickName = "equipmentRadiantProportion_", Description = "Equipment Radiant Proportion [0-1]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "lightingViewCoefficient_", NickName = "lightingViewCoefficient_", Description = "Lighting View Coefficient [0-1]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "occupancyViewCoefficient_", NickName = "occupancyViewCoefficient_", Description = "Occupancy View Coefficient [0-1]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "equipmentViewCoefficient_", NickName = "equipmentViewCoefficient_", Description = "Equipment View Coefficient [0-1]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "lightingControlFunction_", NickName = "lightingControlFunction_", Description = "Lighting Control Function", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "ventilationFunction_", NickName = "ventilationFunction_", Description = "Ventilation Function", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "nCMData_", NickName = "nCMData_", Description = "National Calculation Method (NCM) Data", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "description_", NickName = "description_", Description = "Description", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));


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
                result.Add(new GH_SAMParam(new GooInternalConditionParam() { Name = "internalCondition", NickName = "internalCondition", Description = "SAM Analytical InternalCondition", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            InternalCondition internalCondition = null;
            index = Params.IndexOfInputParam("internalCondition_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref internalCondition);
            }

            if (internalCondition == null)
            {
                internalCondition = new InternalCondition("Default Internal Condition");
            }
            else
            {
                internalCondition = new InternalCondition(Guid.NewGuid(), internalCondition);
            }

            string name = null;
            index = Params.IndexOfInputParam("name_");
            if (index != -1 && dataAccess.GetData(index, ref name) && name != null)
            {
                internalCondition = new InternalCondition(name, internalCondition);
            }

            double areaPerPerson = double.NaN;
            index = Params.IndexOfInputParam("areaPerPerson_");
            if (index != -1 && dataAccess.GetData(index, ref areaPerPerson) && !double.IsNaN(areaPerPerson))
            {
                internalCondition.SetValue(InternalConditionParameter.AreaPerPerson, areaPerPerson);
            }

            index = Params.IndexOfInputParam("occupancyProfile_");
            if(index != -1)
            {
                Profile profile = null;
                string profileName = null;
                if(dataAccess.GetData(index, ref profile))
                {
                    profileName = profile.Name;
                }
                else
                {
                    dataAccess.GetData(index, ref profileName);
                }

                if(!string.IsNullOrEmpty(profileName))
                {
                    internalCondition.SetValue(InternalConditionParameter.OccupancyProfileName, profileName);
                }
            }

            double occupancySensibleGainPerPerson = double.NaN;
            index = Params.IndexOfInputParam("occupancySensibleGainPerPerson_");
            if (index != -1 && dataAccess.GetData(index, ref occupancySensibleGainPerPerson) && !double.IsNaN(occupancySensibleGainPerPerson))
            {
                internalCondition.SetValue(InternalConditionParameter.OccupancySensibleGainPerPerson, occupancySensibleGainPerPerson);
            }

            double occupancyLatentGainPerPerson = double.NaN;
            index = Params.IndexOfInputParam("occupancyLatentGainPerPerson_");
            if (index != -1 && dataAccess.GetData(index, ref occupancyLatentGainPerPerson) && !double.IsNaN(occupancyLatentGainPerPerson))
            {
                internalCondition.SetValue(InternalConditionParameter.OccupancyLatentGainPerPerson, occupancyLatentGainPerPerson);
            }

            index = Params.IndexOfInputParam("equipmentSensibleProfile_");
            if (index != -1)
            {
                Profile profile = null;
                string profileName = null;
                if (dataAccess.GetData(index, ref profile))
                {
                    profileName = profile.Name;
                }
                else
                {
                    dataAccess.GetData(index, ref profileName);
                }

                if (!string.IsNullOrEmpty(profileName))
                {
                    internalCondition.SetValue(InternalConditionParameter.EquipmentSensibleProfileName, profileName);
                }
            }

            double equipmentSensibleGain = double.NaN;
            index = Params.IndexOfInputParam("equipmentSensibleGain_");
            if (index != -1 && dataAccess.GetData(index, ref equipmentSensibleGain) && !double.IsNaN(equipmentSensibleGain))
            {
                internalCondition.SetValue(InternalConditionParameter.EquipmentSensibleGain, equipmentSensibleGain);
            }

            double equipmentSensibleGainPerArea = double.NaN;
            index = Params.IndexOfInputParam("equipmentSensibleGainPerArea_");
            if (index != -1 && dataAccess.GetData(index, ref equipmentSensibleGainPerArea) && !double.IsNaN(equipmentSensibleGainPerArea))
            {
                internalCondition.SetValue(InternalConditionParameter.EquipmentSensibleGainPerArea, equipmentSensibleGainPerArea);
            }

            double equipmentSensibleGainPerPerson = double.NaN;
            index = Params.IndexOfInputParam("equipmentSensibleGainPerPerson_");
            if (index != -1 && dataAccess.GetData(index, ref equipmentSensibleGainPerPerson) && !double.IsNaN(equipmentSensibleGainPerPerson))
            {
                internalCondition.SetValue(InternalConditionParameter.EquipmentSensibleGainPerPerson, equipmentSensibleGainPerPerson);
            }

            index = Params.IndexOfInputParam("equipmentLatentProfile_");
            if (index != -1)
            {
                Profile profile = null;
                string profileName = null;
                if (dataAccess.GetData(index, ref profile))
                {
                    profileName = profile.Name;
                }
                else
                {
                    dataAccess.GetData(index, ref profileName);
                }

                if (!string.IsNullOrEmpty(profileName))
                {
                    internalCondition.SetValue(InternalConditionParameter.EquipmentLatentProfileName, profileName);
                }
            }

            double lightingGain = double.NaN;
            index = Params.IndexOfInputParam("lightingGain_");
            if (index != -1 && dataAccess.GetData(index, ref lightingGain) && !double.IsNaN(lightingGain))
            {
                internalCondition.SetValue(InternalConditionParameter.LightingGain, lightingGain);
            }

            double equipmentLatentGain = double.NaN;
            index = Params.IndexOfInputParam("equipmentLatentGain_");
            if (index != -1 && dataAccess.GetData(index, ref equipmentLatentGain) && !double.IsNaN(equipmentLatentGain))
            {
                internalCondition.SetValue(InternalConditionParameter.EquipmentLatentGain, equipmentLatentGain);
            }

            double equipmentLatentGainPerArea = double.NaN;
            index = Params.IndexOfInputParam("equipmentLatentGainPerArea_");
            if (index != -1 && dataAccess.GetData(index, ref equipmentLatentGainPerArea) && !double.IsNaN(equipmentLatentGainPerArea))
            {
                internalCondition.SetValue(InternalConditionParameter.EquipmentLatentGainPerArea, equipmentLatentGainPerArea);
            }

            double lightingLevel = double.NaN;
            index = Params.IndexOfInputParam("lightingLevel_");
            if (index != -1 && dataAccess.GetData(index, ref lightingLevel) && !double.IsNaN(lightingLevel))
            {
                internalCondition.SetValue(InternalConditionParameter.LightingLevel, lightingLevel);
            }

            double lightingEfficiency = double.NaN;
            index = Params.IndexOfInputParam("lightingEfficiency_");
            if (index != -1 && dataAccess.GetData(index, ref lightingEfficiency) && !double.IsNaN(lightingEfficiency))
            {
                internalCondition.SetValue(InternalConditionParameter.LightingEfficiency, lightingEfficiency);
            }

            index = Params.IndexOfInputParam("infiltrationProfile_");
            if (index != -1)
            {
                Profile profile = null;
                string profileName = null;
                if (dataAccess.GetData(index, ref profile))
                {
                    profileName = profile.Name;
                }
                else
                {
                    dataAccess.GetData(index, ref profileName);
                }

                if (!string.IsNullOrEmpty(profileName))
                {
                    internalCondition.SetValue(InternalConditionParameter.InfiltrationProfileName, profileName);
                }
            }

            double infiltrationAirChangesPerHour = double.NaN;
            index = Params.IndexOfInputParam("infiltrationAirChangesPerHour_");
            if (index != -1 && dataAccess.GetData(index, ref infiltrationAirChangesPerHour) && !double.IsNaN(infiltrationAirChangesPerHour))
            {
                internalCondition.SetValue(InternalConditionParameter.InfiltrationAirChangesPerHour, infiltrationAirChangesPerHour);
            }

            index = Params.IndexOfInputParam("pollutantProfile_");
            if (index != -1)
            {
                Profile profile = null;
                string profileName = null;
                if (dataAccess.GetData(index, ref profile))
                {
                    profileName = profile.Name;
                }
                else
                {
                    dataAccess.GetData(index, ref profileName);
                }

                if (!string.IsNullOrEmpty(profileName))
                {
                    internalCondition.SetValue(InternalConditionParameter.PollutantProfileName, profileName);
                }
            }

            double pollutantGenerationPerArea = double.NaN;
            index = Params.IndexOfInputParam("pollutantGenerationPerArea_");
            if (index != -1 && dataAccess.GetData(index, ref pollutantGenerationPerArea) && !double.IsNaN(pollutantGenerationPerArea))
            {
                internalCondition.SetValue(InternalConditionParameter.PollutantGenerationPerArea, pollutantGenerationPerArea);
            }

            double pollutantGenerationPerPerson = double.NaN;
            index = Params.IndexOfInputParam("pollutantGenerationPerPerson_");
            if (index != -1 && dataAccess.GetData(index, ref pollutantGenerationPerPerson) && !double.IsNaN(pollutantGenerationPerPerson))
            {
                internalCondition.SetValue(InternalConditionParameter.PollutantGenerationPerPerson, pollutantGenerationPerPerson);
            }

            index = Params.IndexOfInputParam("heatingProfile_");
            if (index != -1)
            {
                Profile profile = null;
                string profileName = null;
                if (dataAccess.GetData(index, ref profile))
                {
                    profileName = profile.Name;
                }
                else
                {
                    dataAccess.GetData(index, ref profileName);
                }

                if (!string.IsNullOrEmpty(profileName))
                {
                    internalCondition.SetValue(InternalConditionParameter.HeatingProfileName, profileName);
                }
            }

            double heatingEmitterCoefficient = double.NaN;
            index = Params.IndexOfInputParam("heatingEmitterCoefficient_");
            if (index != -1 && dataAccess.GetData(index, ref heatingEmitterCoefficient) && !double.IsNaN(heatingEmitterCoefficient))
            {
                internalCondition.SetValue(InternalConditionParameter.HeatingEmitterCoefficient, heatingEmitterCoefficient);
            }

            double heatingEmitterRadiantProportion = double.NaN;
            index = Params.IndexOfInputParam("heatingEmitterRadiantProportion_");
            if (index != -1 && dataAccess.GetData(index, ref heatingEmitterRadiantProportion) && !double.IsNaN(heatingEmitterRadiantProportion))
            {
                internalCondition.SetValue(InternalConditionParameter.HeatingEmitterRadiantProportion, heatingEmitterRadiantProportion);
            }

            index = Params.IndexOfInputParam("coolingProfile_");
            if (index != -1)
            {
                Profile profile = null;
                string profileName = null;
                if (dataAccess.GetData(index, ref profile))
                {
                    profileName = profile.Name;
                }
                else
                {
                    dataAccess.GetData(index, ref profileName);
                }

                if (!string.IsNullOrEmpty(profileName))
                {
                    internalCondition.SetValue(InternalConditionParameter.CoolingProfileName, profileName);
                }
            }

            double coolingEmitterCoefficient = double.NaN;
            index = Params.IndexOfInputParam("coolingEmitterCoefficient_");
            if (index != -1 && dataAccess.GetData(index, ref coolingEmitterCoefficient) && !double.IsNaN(coolingEmitterCoefficient))
            {
                internalCondition.SetValue(InternalConditionParameter.CoolingEmitterCoefficient, coolingEmitterCoefficient);
            }

            double coolingEmitterRadiantProportion = double.NaN;
            index = Params.IndexOfInputParam("coolingEmitterRadiantProportion_");
            if (index != -1 && dataAccess.GetData(index, ref coolingEmitterRadiantProportion) && !double.IsNaN(coolingEmitterRadiantProportion))
            {
                internalCondition.SetValue(InternalConditionParameter.CoolingEmitterRadiantProportion, coolingEmitterRadiantProportion);
            }

            index = Params.IndexOfInputParam("humidificationProfile_");
            if (index != -1)
            {
                Profile profile = null;
                string profileName = null;
                if (dataAccess.GetData(index, ref profile))
                {
                    profileName = profile.Name;
                }
                else
                {
                    dataAccess.GetData(index, ref profileName);
                }

                if (!string.IsNullOrEmpty(profileName))
                {
                    internalCondition.SetValue(InternalConditionParameter.HumidificationProfileName, profileName);
                }
            }

            index = Params.IndexOfInputParam("dehumidificationProfile_");
            if (index != -1)
            {
                Profile profile = null;
                string profileName = null;
                if (dataAccess.GetData(index, ref profile))
                {
                    profileName = profile.Name;
                }
                else
                {
                    dataAccess.GetData(index, ref profileName);
                }

                if (!string.IsNullOrEmpty(profileName))
                {
                    internalCondition.SetValue(InternalConditionParameter.DehumidificationProfileName, profileName);
                }
            }

            string ventilationSystemTypeName = null;
            index = Params.IndexOfInputParam("ventilationSystemTypeName_");
            if (index != -1 && dataAccess.GetData(index, ref ventilationSystemTypeName) && !string.IsNullOrEmpty(ventilationSystemTypeName))
            {
                internalCondition.SetValue(InternalConditionParameter.VentilationSystemTypeName, ventilationSystemTypeName);
            }

            string coolingSystemTypeName = null;
            index = Params.IndexOfInputParam("coolingSystemTypeName_");
            if (index != -1 && dataAccess.GetData(index, ref coolingSystemTypeName) && !string.IsNullOrEmpty(coolingSystemTypeName))
            {
                internalCondition.SetValue(InternalConditionParameter.CoolingSystemTypeName, coolingSystemTypeName);
            }

            string heatingSystemTypeName = null;
            index = Params.IndexOfInputParam("heatingSystemTypeName_");
            if (index != -1 && dataAccess.GetData(index, ref heatingSystemTypeName) && !string.IsNullOrEmpty(heatingSystemTypeName))
            {
                internalCondition.SetValue(InternalConditionParameter.HeatingSystemTypeName, heatingSystemTypeName);
            }

            index = Params.IndexOfInputParam("ventilationProfile_");
            if (index != -1)
            {
                Profile profile = null;
                string profileName = null;
                if (dataAccess.GetData(index, ref profile))
                {
                    profileName = profile.Name;
                }
                else
                {
                    dataAccess.GetData(index, ref profileName);
                }

                if (!string.IsNullOrEmpty(profileName))
                {
                    internalCondition.SetValue(InternalConditionParameter.VentilationProfileName, profileName);
                }
            }

            double supplyAirFlowPerPerson = double.NaN;
            index = Params.IndexOfInputParam("supplyAirFlowPerPerson_");
            if (index != -1 && dataAccess.GetData(index, ref supplyAirFlowPerPerson) && !double.IsNaN(supplyAirFlowPerPerson))
            {
                internalCondition.SetValue(InternalConditionParameter.SupplyAirFlowPerPerson, supplyAirFlowPerPerson);
            }

            double exhaustAirFlowPerPerson = double.NaN;
            index = Params.IndexOfInputParam("exhaustAirFlowPerPerson_");
            if (index != -1 && dataAccess.GetData(index, ref exhaustAirFlowPerPerson) && !double.IsNaN(exhaustAirFlowPerPerson))
            {
                internalCondition.SetValue(InternalConditionParameter.ExhaustAirFlowPerPerson, exhaustAirFlowPerPerson);
            }

            double supplyAirChangesPerHour = double.NaN;
            index = Params.IndexOfInputParam("supplyAirChangesPerHour_");
            if (index != -1 && dataAccess.GetData(index, ref supplyAirChangesPerHour) && !double.IsNaN(supplyAirChangesPerHour))
            {
                internalCondition.SetValue(InternalConditionParameter.SupplyAirChangesPerHour, supplyAirChangesPerHour);
            }

            double exhaustAirChangesPerHour = double.NaN;
            index = Params.IndexOfInputParam("exhaustAirChangesPerHour_");
            if (index != -1 && dataAccess.GetData(index, ref exhaustAirChangesPerHour) && !double.IsNaN(exhaustAirChangesPerHour))
            {
                internalCondition.SetValue(InternalConditionParameter.ExhaustAirChangesPerHour, exhaustAirChangesPerHour);
            }

            double supplyAirFlowPerArea = double.NaN;
            index = Params.IndexOfInputParam("supplyAirFlowPerArea_");
            if (index != -1 && dataAccess.GetData(index, ref supplyAirFlowPerArea) && !double.IsNaN(supplyAirFlowPerArea))
            {
                internalCondition.SetValue(InternalConditionParameter.SupplyAirFlowPerArea, supplyAirFlowPerArea);
            }

            double exhaustAirFlowPerArea = double.NaN;
            index = Params.IndexOfInputParam("exhaustAirFlowPerArea_");
            if (index != -1 && dataAccess.GetData(index, ref exhaustAirFlowPerArea) && !double.IsNaN(exhaustAirFlowPerArea))
            {
                internalCondition.SetValue(InternalConditionParameter.ExhaustAirFlowPerArea, exhaustAirFlowPerArea);
            }

            double supplyAirFlow = double.NaN;
            index = Params.IndexOfInputParam("supplyAirFlow_");
            if (index != -1 && dataAccess.GetData(index, ref supplyAirFlow) && !double.IsNaN(supplyAirFlow))
            {
                internalCondition.SetValue(InternalConditionParameter.SupplyAirFlow, supplyAirFlow);
            }

            double exhaustAirFlow = double.NaN;
            index = Params.IndexOfInputParam("exhaustAirFlow_");
            if (index != -1 && dataAccess.GetData(index, ref exhaustAirFlow) && !double.IsNaN(exhaustAirFlow))
            {
                internalCondition.SetValue(InternalConditionParameter.ExhaustAirFlow, exhaustAirFlow);
            }

            double lightingRadiantProportion = double.NaN;
            index = Params.IndexOfInputParam("lightingRadiantProportion_");
            if (index != -1 && dataAccess.GetData(index, ref lightingRadiantProportion) && !double.IsNaN(lightingRadiantProportion))
            {
                internalCondition.SetValue(InternalConditionParameter.LightingRadiantProportion, lightingRadiantProportion);
            }

            double occupancyRadiantProportion = double.NaN;
            index = Params.IndexOfInputParam("occupancyRadiantProportion_");
            if (index != -1 && dataAccess.GetData(index, ref occupancyRadiantProportion) && !double.IsNaN(occupancyRadiantProportion))
            {
                internalCondition.SetValue(InternalConditionParameter.OccupancyRadiantProportion, occupancyRadiantProportion);
            }

            double equipmentRadiantProportion = double.NaN;
            index = Params.IndexOfInputParam("equipmentRadiantProportion_");
            if (index != -1 && dataAccess.GetData(index, ref equipmentRadiantProportion) && !double.IsNaN(equipmentRadiantProportion))
            {
                internalCondition.SetValue(InternalConditionParameter.EquipmentRadiantProportion, equipmentRadiantProportion);
            }

            double lightingViewCoefficient = double.NaN;
            index = Params.IndexOfInputParam("lightingViewCoefficient_");
            if (index != -1 && dataAccess.GetData(index, ref lightingViewCoefficient) && !double.IsNaN(lightingViewCoefficient))
            {
                internalCondition.SetValue(InternalConditionParameter.LightingViewCoefficient, lightingViewCoefficient);
            }

            double occupancyViewCoefficient = double.NaN;
            index = Params.IndexOfInputParam("occupancyViewCoefficient_");
            if (index != -1 && dataAccess.GetData(index, ref occupancyViewCoefficient) && !double.IsNaN(occupancyViewCoefficient))
            {
                internalCondition.SetValue(InternalConditionParameter.OccupancyViewCoefficient, occupancyViewCoefficient);
            }

            double equipmentViewCoefficient = double.NaN;
            index = Params.IndexOfInputParam("equipmentViewCoefficient_");
            if (index != -1 && dataAccess.GetData(index, ref equipmentViewCoefficient) && !double.IsNaN(equipmentViewCoefficient))
            {
                internalCondition.SetValue(InternalConditionParameter.EquipmentViewCoefficient, equipmentViewCoefficient);
            }

            string lightingControlFunction = null;
            index = Params.IndexOfInputParam("lightingControlFunction_");
            if (index != -1 && dataAccess.GetData(index, ref lightingControlFunction) && !string.IsNullOrEmpty(lightingControlFunction))
            {
                internalCondition.SetValue(InternalConditionParameter.LightingControlFunction, lightingControlFunction);
            }

            string ventilationFunction = null;
            index = Params.IndexOfInputParam("ventilationFunction_");
            if (index != -1 && dataAccess.GetData(index, ref ventilationFunction) && !string.IsNullOrEmpty(ventilationFunction))
            {
                internalCondition.SetValue(InternalConditionParameter.VentilationFunction, ventilationFunction);
            }

            index = Params.IndexOfInputParam("nCMData_");
            if (index != -1)
            {
                IAnalyticalObject analyticalObject = null;
                if (dataAccess.GetData(index, ref analyticalObject))
                {
                    NCMData nCMData = analyticalObject as NCMData;
                    if(nCMData != null)
                    {
                        internalCondition.SetValue(InternalConditionParameter.NCMData, nCMData);
                    }

                }
            }

            string description = null;
            index = Params.IndexOfInputParam("description_");
            if (index != -1 && dataAccess.GetData(index, ref description) && !string.IsNullOrEmpty(description))
            {
                internalCondition.SetValue(InternalConditionParameter.Description, description);
            }


            index = Params.IndexOfOutputParam("internalCondition");
            if(index != -1)
            {

                dataAccess.SetData(index, new GooInternalCondition(internalCondition));
            }
        }
    }
}