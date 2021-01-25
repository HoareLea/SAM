using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateSetPoint : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b57f85bc-78c4-45ee-9f75-85e3a34804cc");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

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
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "_spaces_", NickName = "_spaces_", Description = "SAM Analytical Spaces, if nothing connected all spaces from AnalyticalModel will be used", Access = GH_ParamAccess.list, Optional = true}, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooProfileParam() { Name = "profileHeating_", NickName = "profileHeating_", Description = "SAM Analytical Profile for Heating", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add( new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "emitterHTGRadianProportion_", NickName = "emitterHTGRadianProportion_", Description = "Heating Emitter Radiant Proportion", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "emitterHTGViewCoefficient_", NickName = "emitterHTGViewCoefficient_", Description = "Heating Emitter View Coefficient", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new GooProfileParam() { Name = "profileCooling_", NickName = "profileCooling_", Description = "SAM Analytical Profile for Cooling", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "emitterCLGRadianProportion_", NickName = "emitterCLGRadianProportion_", Description = "Cooling Emitter Radiant Proportion", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "emitterCLGViewCoefficient_", NickName = "emitterHCLGViewCoefficient_", Description = "Cooling Emitter View Coefficient", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new GooProfileParam() { Name = "profileHumidification_", NickName = "profileHumidification_", Description = "SAM Analytical Profile for Humidification", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooProfileParam() { Name = "profileDehumidification_", NickName = "profileDehumidification_", Description = "SAM Analytical Profile for Dehumidification", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
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
        public SAMAnalyticalUpdateSetPoint()
          : base("SAMAnalytical.UpdateSetPoint", "SAMAnalytical.UpdateSetPoint",
              "Updates SetPoint (Cooling, Heating, Humidification and Dehumidification ) Properties for Spaces \nIf nothing connect orignal Analytical Model will be outputed \nFor reference see https://edsl.myzen.co.uk/manuals/Building%20Simulator/",
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

            Profile profile_Heating = null;
            index = Params.IndexOfInputParam("profileHeating_");
            if (index != -1)
                dataAccess.GetData(index, ref profile_Heating);

            double heatingEmitterRadiantProportion = double.NaN;
            index = Params.IndexOfInputParam("emitterHTGRadianProportion_");
            if (index != -1)
                dataAccess.GetData(index, ref heatingEmitterRadiantProportion);

            double heatingEmitterCoefficient = double.NaN;
            index = Params.IndexOfInputParam("emitterHTGViewCoefficient_");
            if (index != -1)
                dataAccess.GetData(index, ref heatingEmitterCoefficient);

            Profile profile_Cooling = null;
            index = Params.IndexOfInputParam("profileCooling_");
            if (index != -1)
                dataAccess.GetData(index, ref profile_Cooling);

            double coolingEmitterRadiantProportion = double.NaN;
            index = Params.IndexOfInputParam("emitterCLGRadianProportion_");
            if (index != -1)
                dataAccess.GetData(index, ref coolingEmitterRadiantProportion);

            double coolingEmitterCoefficient = double.NaN;
            index = Params.IndexOfInputParam("emitterCLGViewCoefficient_");
            if (index != -1)
                dataAccess.GetData(index, ref coolingEmitterCoefficient);

            Profile profile_Humidification = null;
            index = Params.IndexOfInputParam("profileHumidification_");
            if (index != -1)
                dataAccess.GetData(index, ref profile_Humidification);

            Profile profile_Dehumidification = null;
            index = Params.IndexOfInputParam("profileDehumidification_");
            if (index != -1)
                dataAccess.GetData(index, ref profile_Dehumidification);

            ProfileLibrary profileLibrary = analyticalModel.ProfileLibrary;

            if (profile_Heating != null)
                profileLibrary.Add(profile_Heating);

            if (profile_Cooling != null)
                profileLibrary.Add(profile_Cooling);

            if (profile_Humidification != null)
                profileLibrary.Add(profile_Humidification);

            if (profile_Dehumidification != null)
                profileLibrary.Add(profile_Dehumidification);

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

                if (profile_Heating != null)
                    internalCondition.SetValue(InternalConditionParameter.HeatingProfileName, profile_Heating.Name);

                if(!double.IsNaN(heatingEmitterRadiantProportion))
                    internalCondition.SetValue(InternalConditionParameter.HeatingEmitterRadiantProportion, heatingEmitterRadiantProportion);

                if (!double.IsNaN(heatingEmitterCoefficient))
                    internalCondition.SetValue(InternalConditionParameter.HeatingEmitterCoefficient, heatingEmitterCoefficient);

                if (profile_Cooling != null)
                    internalCondition.SetValue(InternalConditionParameter.CoolingProfileName, profile_Cooling.Name);

                if (!double.IsNaN(coolingEmitterRadiantProportion))
                    internalCondition.SetValue(InternalConditionParameter.CoolingEmitterRadiantProportion, coolingEmitterRadiantProportion);

                if (!double.IsNaN(coolingEmitterCoefficient))
                    internalCondition.SetValue(InternalConditionParameter.CoolingEmitterCoefficient, coolingEmitterCoefficient);

                if (profile_Humidification != null)
                    internalCondition.SetValue(InternalConditionParameter.HumidificationProfileName, profile_Humidification.Name);

                if (profile_Dehumidification != null)
                    internalCondition.SetValue(InternalConditionParameter.DehumidificationProfileName, profile_Dehumidification.Name);

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