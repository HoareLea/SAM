using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalModifyInternalConditionByProfile : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6c2c620a-3932-456e-84f8-b0a4c210dfa5");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalModifyInternalConditionByProfile()
          : base("SAMAnalytical.ModifyInternalConditionByProfile", "SAMAnalytical.ModifyInternalConditionByProfile",
              "Modify InternalCondition By Profile",
              "SAM", "Analytical02")
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

                GooAnalyticalModelParam gooAnalyticalModelParam = new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM Analytical Model", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(gooAnalyticalModelParam, ParamVisibility.Binding));

                GooProfileParam gooProfileParam = new GooProfileParam() { Name = "_profile", NickName = "_profile", Description = "SAM Analytical Profile", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(gooProfileParam, ParamVisibility.Binding));

                GooSpaceParam gooSpaceParam = new GooSpaceParam() { Name = "_spaces", NickName = "_spaces", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list };
                result.Add(new GH_SAMParam(gooSpaceParam, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "analyticalModel", NickName = "analyticalModel", Description = "SAM Analytical Model", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            int index = -1;

            index = Params.IndexOfInputParam("_analyticalModel");
            AnalyticalModel analyticalModel = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            index = Params.IndexOfInputParam("_profile");
            Profile profile = null;
            if (index == -1 || !dataAccess.GetData(index, ref profile) || profile == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            if(profile.ProfileType == ProfileType.Undefined)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid ProfileType");
                return;
            }

            index = Params.IndexOfInputParam("_spaces");
            List<Space> spaces = new List<Space>();
            if (index == -1 || !dataAccess.GetDataList(index, spaces) || spaces == null || spaces.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Data");
                return;
            }

            ProfileLibrary profileLibrary = analyticalModel.ProfileLibrary;
            if(profileLibrary == null)
            {
                profileLibrary = new ProfileLibrary(analyticalModel.Name);
            }

            profileLibrary.Add(profile);

            AdjacencyCluster adjacencyCluster = analyticalModel.AdjacencyCluster;
            if(adjacencyCluster != null)
            {
                adjacencyCluster = new AdjacencyCluster(adjacencyCluster);
                
                foreach (Space space in spaces)
                {
                    Space space_Temp = adjacencyCluster.GetObject<Space>(space.Guid);
                    if (space_Temp == null)
                    {
                        continue;
                    }

                    space_Temp = new Space(space_Temp);

                    InternalCondition internalCondition = space_Temp.InternalCondition;
                    if (internalCondition == null)
                    {
                        internalCondition = new InternalCondition(space_Temp.Name);
                    }

                    internalCondition.SetProfileName(profile.ProfileType, profile.Name);

                    space_Temp.InternalCondition = internalCondition;

                    adjacencyCluster.AddObject(space_Temp);
                }

                analyticalModel = new AnalyticalModel(analyticalModel, adjacencyCluster, analyticalModel.MaterialLibrary, profileLibrary);
            }

            index = Params.IndexOfOutputParam("analyticalModel");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalModel);
            }
        }
    }
}