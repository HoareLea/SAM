using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateEquipmentGains : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b120b400-4e6a-4dec-be1c-2a2068433e3e");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM Analytical AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "spaces_", NickName = "spaces_", Description = "SAM Analytical Space or Spaces. Default all spaces from Analytcial Model", Access = GH_ParamAccess.list, Optional = true}, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooProfileParam() { Name = "profileSens_", NickName = "profileSens_", Description = "SAM Analytical Profile for Equipment Sensible Gains", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooProfileParam() { Name = "profileLat_", NickName = "profileLat_", Description = "SAM Analytical Profile for Equipment Latent Gains", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add( new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "equipmentSensGainPerArea_", NickName = "equipmentSensGainPerArea_", Description = "Equipment Sensible Gain Per Area, W/m2", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "equipmentLatGainPerArea_", NickName = "equipmentLatGainPerArea_", Description = "Equipment Latent Gain Per Area, W/m2", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "equipmentSensGain_", NickName = "equipmentSensGain_", Description = "Equipment Sensible Gain, W", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "equipmentLatGain_", NickName = "equipmentLatGain_", Description = "Equipment Latent Gain, W", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));

                //set default values 
                global::Grasshopper.Kernel.Parameters.Param_Boolean boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_multipleSenisbleGainPerPerson_", NickName = "_multipleSenisbleGainPerPerson_", Description = "Multiple Sensible Gain Per Person, Default = false", Access = GH_ParamAccess.item};
                boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(boolean, ParamVisibility.Voluntary));

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
        public SAMAnalyticalUpdateEquipmentGains()
          : base("SAMAnalytical.UpdateEquipmentGains", "SAMAnalytical.UpdateEquipmentGains",
              "Updates Equipment Gains Properties in Internal Condition for Spaces. If both connected in W/2 and W sum will be calculated. There is option to set bool=True to muliply W per person. ie laptop per person. Connect selected Spaces or Space, default all Spaces.",
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
            index = Params.IndexOfInputParam("spaces_");
            if(index != -1)
            {
                spaces = new List<Space>();
                dataAccess.GetDataList(index, spaces);
                if (spaces != null && spaces.Count == 0)
                    spaces = null;
            }

            if (spaces == null)
                spaces = analyticalModel.GetSpaces();

            Profile profile_Sensible = null;
            index = Params.IndexOfInputParam("profileSens_");
            if (index != -1)
                dataAccess.GetData(index, ref profile_Sensible);

            Profile profile_Latent = null;
            index = Params.IndexOfInputParam("profileLat_");
            if (index != -1)
                dataAccess.GetData(index, ref profile_Latent);

            double equipmentSensGainPerArea = double.NaN;
            index = Params.IndexOfInputParam("equipmentSensGainPerArea_");
            if (index != -1)
                dataAccess.GetData(index, ref equipmentSensGainPerArea);

            double equipmentLatGainPerArea = double.NaN;
            index = Params.IndexOfInputParam("equipmentLatGainPerArea_");
            if (index != -1)
                dataAccess.GetData(index, ref equipmentLatGainPerArea);

            double equipmentSensGain = double.NaN;
            index = Params.IndexOfInputParam("equipmentSensGain_");
            if (index != -1)
                dataAccess.GetData(index, ref equipmentSensGain);

            double equipmentLatGain = double.NaN;
            index = Params.IndexOfInputParam("equipmentLatGain_");
            if (index != -1)
                dataAccess.GetData(index, ref equipmentLatGain);

            bool multipleSenisbleGainPerPerson = false;
            index = Params.IndexOfInputParam("_multipleSenisbleGainPerPerson_");
            if (index != -1)
                dataAccess.GetData(index, ref multipleSenisbleGainPerPerson);

            ProfileLibrary profileLibrary = analyticalModel.ProfileLibrary;

            if (profile_Sensible != null)
                profileLibrary.Add(profile_Sensible);

            if (profile_Latent != null)
                profileLibrary.Add(profile_Latent);

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

                if (profile_Sensible != null)
                    internalCondition.SetValue(InternalConditionParameter.EquipmentSensibleProfileName, profile_Sensible.Name);

                if (profile_Latent != null)
                    internalCondition.SetValue(InternalConditionParameter.EquipmentLatentProfileName, profile_Latent.Name);

                if (!double.IsNaN(equipmentSensGainPerArea))
                    internalCondition.SetValue(InternalConditionParameter.EquipmentSensibleGainPerArea, equipmentSensGainPerArea);

                if (!double.IsNaN(equipmentLatGainPerArea))
                    internalCondition.SetValue(InternalConditionParameter.EquipmentLatentGainPerArea, equipmentLatGainPerArea);

                if (!double.IsNaN(equipmentSensGain))
                {
                    double equipmentSensGain_Temp = equipmentSensGain;
                    if (multipleSenisbleGainPerPerson)
                    {
                        double occupancy = space.CalculatedOccupancy();
                        if(!double.IsNaN(occupancy))
                            equipmentSensGain_Temp = equipmentSensGain_Temp * occupancy;
                        else
                            equipmentSensGain_Temp = 0;
                    }

                    internalCondition.SetValue(InternalConditionParameter.EquipmentSensibleGain, equipmentSensGain_Temp);
                }

                if (!double.IsNaN(equipmentLatGain))
                    internalCondition.SetValue(InternalConditionParameter.EquipmentLatentGain, equipmentLatGain);

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