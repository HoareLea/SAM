using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateInfiltrationGains : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("dc6afc04-27b4-4cd6-b0fe-16592370cbe0");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

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
                result.Add(new GH_SAMParam(new GooProfileParam() { Name = "profile_", NickName = "profile_", Description = "SAM Analytical Profile", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add( new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "infiltrationACH_", NickName = "infiltrationACH_", Description = "Infiltration Air Changes Per Hour, ac/h", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                //result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "toExternalFacing_", NickName = "toExternalFacing_", Description = "If False then value for Infiltration Air Changes Per Hour will be applied to all spaces", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "airflowPerExtArea_", NickName = "airflowPerExtArea_", Description = "Airflow Per External Area", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_Boolean boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "toExternalFacing_", NickName = "toExternalFacing_", Description = "If False then value for Infiltration Air Changes Per Hour will be applied to all spaces", Access = GH_ParamAccess.item };
                boolean.SetPersistentData(true);
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
        public SAMAnalyticalUpdateInfiltrationGains()
          : base("SAMAnalytical.UpdateInfiltrationGains", "SAMAnalytical.UpdateInfiltrationGains",
              "Updates Infiltration in Internal Condition for Spaces",
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

            Profile profile = null;
            index = Params.IndexOfInputParam("profile_");
            if (index != -1)
                dataAccess.GetData(index, ref profile);

            double infiltrationAirChangesPerHour = double.NaN;
            index = Params.IndexOfInputParam("infiltrationACH_");
            if (index != -1)
                dataAccess.GetData(index, ref infiltrationAirChangesPerHour);

            bool toExternaFacing = true;
            index = Params.IndexOfInputParam("toExternalFacing_");
            if (index != -1)
                dataAccess.GetData(index, ref toExternaFacing);

            double airflowPerExtArea = double.NaN;
            index = Params.IndexOfInputParam("airflowPerExtArea_");
            if (index != -1)
                dataAccess.GetData(index, ref airflowPerExtArea);

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
                    internalCondition.SetValue(InternalConditionParameter.InfiltrationProfileName, profile.Name);

                if(!double.IsNaN(infiltrationAirChangesPerHour) || !double.IsNaN(airflowPerExtArea))
                {
                    double value = 0;

                    double area_External = adjacencyCluster.ExternalPanelsArea(space, true);
                    if (!double.IsNaN(airflowPerExtArea))
                    {
                        if (!double.IsNaN(area_External))
                            value = airflowPerExtArea * area_External;
                    }
                    else
                    {
                        bool apply = true;
                        if (toExternaFacing)
                        {
                            if (double.IsNaN(area_External) || area_External == 0)
                                apply = false;
                        }

                        if (apply)
                            value = infiltrationAirChangesPerHour;
                    }

                    internalCondition.SetValue(InternalConditionParameter.InfiltrationAirChangesPerHour, value);
                }

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