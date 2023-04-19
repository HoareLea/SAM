using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateProfileByBooleans : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("cb7b0a6e-0d45-46c6-a37e-295c3606b782");

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_name", NickName = "_name", Description = "Name of Profile", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_booleans", NickName = "_booleans", Description = "Booleans (True/False)", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooProfileParam() {Name = "profile", NickName = "profile", Description = "SAM Analytical Profile", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooProfileParam() { Name = "reversedProfile", NickName = "profile", Description = "SAM Analytical Reversed Profile", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMAnalyticalCreateProfileByBooleans()
          : base("SAMAnalytical.CreateProfileByBooleans", "SAMAnalytical.CreateProfileByBooleans",
              "Creates SAM Analytical Profile By Booleans",
              "SAM WIP", "Analytical")
        {
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            index = Params.IndexOfInputParam("_name");
            if(index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string name = null;
            if (!dataAccess.GetData(index, ref name) || string.IsNullOrWhiteSpace(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_booleans");
            if (index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<bool> booleans = new List<bool>();
            if (!dataAccess.GetDataList(index, booleans) || booleans == null || booleans.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            Profile profile = new Profile(name, ProfileType.Other, booleans.ConvertAll(x => x ? 1.0 : 0.0));
            Profile profile_Reversed = new Profile(string.Format("{0}_Reversed", name), ProfileType.Other, booleans.ConvertAll(x => x ? 0.0 : 1.0));

            index = Params.IndexOfOutputParam("profile");
            if (index != -1)
                dataAccess.SetData(index, new GooProfile(profile));

            index = Params.IndexOfOutputParam("reversedProfile");
            if (index != -1)
                dataAccess.SetData(index, new GooProfile(profile_Reversed));
        }
    }
}