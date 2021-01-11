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
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM Analytical AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "_spaces_", NickName = "_spaces_", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list, Optional = true}, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooProfileParam() { Name = "_profileSens_", NickName = "_profileSens_", Description = "SAM Analytical Profile for Equipment Sensible Gains", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooProfileParam() { Name = "_profileLat_", NickName = "_profileLat_", Description = "SAM Analytical Profile for Equipment Latent Gains", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add( new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_equipmentSensGainPerArea_", NickName = "_equipmentSensGainPerArea_", Description = "Equipment Sensible Gain Per Area", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_equipmentLatGainPerArea_", NickName = "_equipmentLatGainPerArea_", Description = "Equipment Latent Gain Per Area", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_equipmentSensGain_", NickName = "_equipmentSensGain_", Description = "Equipment Sensible Gain", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_equipmentLatGain_", NickName = "_equipmentLatGain_", Description = "Equipment Latent Gain", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
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
              "Updates Equipment Gains Properties for Spaces",
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

            Profile profile_Sensible = null;
            index = Params.IndexOfInputParam("_profileSens_");
            if (index != -1)
                dataAccess.GetData(index, ref profile_Sensible);

            Profile profile_Latent = null;
            index = Params.IndexOfInputParam("_profileLat_");
            if (index != -1)
                dataAccess.GetData(index, ref profile_Latent);

            double equipmentSensGainPerArea = double.NaN;
            index = Params.IndexOfInputParam("_equipmentSensGainPerArea_");
            if (index != -1)
                dataAccess.GetData(index, ref equipmentSensGainPerArea);

            double equipmentLatGainPerArea = double.NaN;
            index = Params.IndexOfInputParam("_equipmentLatGainPerArea_");
            if (index != -1)
                dataAccess.GetData(index, ref equipmentLatGainPerArea);

            double equipmentSensGain = double.NaN;
            index = Params.IndexOfInputParam("_equipmentSensGain_");
            if (index != -1)
                dataAccess.GetData(index, ref equipmentSensGain);

            double equipmentLatGain = double.NaN;
            index = Params.IndexOfInputParam("_equipmentLatGain_");
            if (index != -1)
                dataAccess.GetData(index, ref equipmentLatGain);

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

                InternalCondition internalCondition = space.InternalCondition;
                if(internalCondition == null)
                    internalCondition = new InternalCondition(space.Name);

                if (profile_Sensible != null)
                    internalCondition.SetValue(InternalConditionParameter.EquipmentSensibleProfileName, profile_Sensible.Name);

                if (profile_Latent != null)
                    internalCondition.SetValue(InternalConditionParameter.EquipmentLatentProfileName, profile_Latent.Name);

                if (!double.IsNaN(equipmentSensGainPerArea))
                    internalCondition.SetValue(InternalConditionParameter.EquipmentSensibleGainPerArea, equipmentSensGainPerArea);

                if (!double.IsNaN(equipmentLatGainPerArea))
                    internalCondition.SetValue(InternalConditionParameter.EquipmentLatentGainPerArea, equipmentLatGainPerArea);

                if (!double.IsNaN(equipmentSensGain))
                    internalCondition.SetValue(InternalConditionParameter.EquipmentSensibleGain, equipmentSensGain);

                if (!double.IsNaN(equipmentLatGain))
                    internalCondition.SetValue(InternalConditionParameter.EquipmentLatentGain, equipmentLatGain);

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