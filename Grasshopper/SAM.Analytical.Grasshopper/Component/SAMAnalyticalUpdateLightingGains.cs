using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateLightingGains : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c32754f9-4e24-4297-ba08-a31fc0f9ef3d");

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
                result.Add(new GH_SAMParam(new GooProfileParam() { Name = "_profile_", NickName = "_profile_", Description = "SAM Analytical Profile", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add( new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_lightingGainPerArea_", NickName = "_lightingGainPerArea_", Description = "Lighting Gain Per Area", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_lightingGain_", NickName = "_lightingGain_", Description = "Lighting Gain", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_lightingLevel_", NickName = "_lightingLevel_", Description = "Lighting Level", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_lightingEfficiency_", NickName = "_lightingEfficiency_", Description = "Lighting Efficiency", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooProfileParam() {Name = "AnalyticalModel", NickName = "AnalyticalModel", Description = "SAM Analytical AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooProfileParam() { Name = "InternalConditions", NickName = "InternalConditions", Description = "SAM Analytical InternalConditions", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMAnalyticalUpdateLightingGains()
          : base("SAMAnalytical.UpdateLightingGains", "SAMAnalytical.UpdateLightingGains",
              "Updates Lighting Gains Properties for Spaces",
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

            double lightingGainPerArea = double.NaN;
            index = Params.IndexOfInputParam("_lightingGainPerArea_");
            if (index != -1)
                dataAccess.GetData(index, ref lightingGainPerArea);

            double lightingGain = double.NaN;
            index = Params.IndexOfInputParam("_lightingGain_");
            if (index != -1)
                dataAccess.GetData(index, ref lightingGain);

            double lightingLevel = double.NaN;
            index = Params.IndexOfInputParam("_lightingLevel_");
            if (index != -1)
                dataAccess.GetData(index, ref lightingLevel);

            double lightingEfficiency = double.NaN;
            index = Params.IndexOfInputParam("_lightingEfficiency_");
            if (index != -1)
                dataAccess.GetData(index, ref lightingEfficiency);

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

                InternalCondition internalCondition = space.InternalCondition;
                if(internalCondition == null)
                    internalCondition = new InternalCondition(space.Name);

                if (profile != null)
                    internalCondition.SetValue(InternalConditionParameter.LightingProfileName, profile.Name);

                if(!double.IsNaN(lightingGainPerArea))
                    internalCondition.SetValue(InternalConditionParameter.LightingGainPerArea, lightingGainPerArea);

                if (!double.IsNaN(lightingGain))
                    internalCondition.SetValue(InternalConditionParameter.LightingGain, lightingGain);

                if (!double.IsNaN(lightingLevel))
                    internalCondition.SetValue(InternalConditionParameter.LightingLevel, lightingLevel);

                if (!double.IsNaN(lightingEfficiency))
                    internalCondition.SetValue(InternalConditionParameter.LightingEfficiency, lightingEfficiency);

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