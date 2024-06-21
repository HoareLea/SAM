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
        public override string LatestComponentVersion => "1.0.1";

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

            index = Params.IndexOfOutputParam("internalCondition_");
            if(index != -1)
            {

                dataAccess.SetData(index, new GooInternalCondition(internalCondition));
            }
        }
    }
}