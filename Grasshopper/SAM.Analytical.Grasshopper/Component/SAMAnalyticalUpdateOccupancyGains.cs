using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateOccupancyGains : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("7553d77e-69c6-4ec2-86fb-baf5a2637fc2");

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
                result.Add(new GH_SAMParam(new GooProfileParam() { Name = "profile_", NickName = "profile_", Description = "SAM Analytical Profile", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooDegreeOfActivityParam() { Name = "degreeOfActivity_", NickName = "degreeOfActivity_", Description = "SAM Analytical DegreeOfActivity", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add( new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "occupancySensibleGainPerPerson_", NickName = "occupancySensibleGainPerPerson_", Description = "Occupancy Sensible Gain Per Person", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "occupancyLatentGainPerPerson_", NickName = "occupancyLatentGainPerPerson_", Description = "Occupancy Latent Gain Per Person", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "areaPerPerson_", NickName = "areaPerPerson_", Description = "Area Per Person", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "occupancy_", NickName = "occupancy_", Description = "Occupancy", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
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
        public SAMAnalyticalUpdateOccupancyGains()
          : base("SAMAnalytical.UpdateOccupancyGains", "SAMAnalytical.UpdateOccupancyGains",
              "Updates Occupancy Gains Properties for Spaces",
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

            double occupancySensibleGainPerPerson = double.NaN;
            index = Params.IndexOfInputParam("occupancySensibleGainPerPerson_");
            if (index != -1)
                dataAccess.GetData(index, ref occupancySensibleGainPerPerson);

            double occupancyLatentGainPerPerson = double.NaN;
            index = Params.IndexOfInputParam("occupancyLatentGainPerPerson_");
            if (index != -1)
                dataAccess.GetData(index, ref occupancyLatentGainPerPerson);

            double occupancy = double.NaN;
            index = Params.IndexOfInputParam("occupancy_");
            if (index != -1)
                dataAccess.GetData(index, ref occupancy);

            double areaPerPerson = double.NaN;
            index = Params.IndexOfInputParam("areaPerPerson_");
            if (index != -1)
                dataAccess.GetData(index, ref areaPerPerson);

            DegreeOfActivity degreeOfActivity = null;
            index = Params.IndexOfInputParam("degreeOfActivity_");
            if (index != -1)
                dataAccess.GetData(index, ref degreeOfActivity);

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
                    internalCondition.SetValue(InternalConditionParameter.OccupancyProfileName, profile.Name);

                if (double.IsNaN(occupancySensibleGainPerPerson))
                {
                    if(degreeOfActivity != null)
                        internalCondition.SetValue(InternalConditionParameter.OccupancySensibleGainPerPerson, degreeOfActivity.Sensible);
                }
                else
                {
                    internalCondition.SetValue(InternalConditionParameter.OccupancySensibleGainPerPerson, occupancySensibleGainPerPerson);
                }

                if (double.IsNaN(occupancyLatentGainPerPerson))
                {
                    if (degreeOfActivity != null)
                        internalCondition.SetValue(InternalConditionParameter.OccupancyLatentGainPerPerson, degreeOfActivity.Latent);
                }
                else
                {
                    internalCondition.SetValue(InternalConditionParameter.OccupancyLatentGain, occupancyLatentGainPerPerson);
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