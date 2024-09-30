using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateProfileByValues : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("50610d29-ac34-46fc-b829-632f7ce30f38");

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
                result.Add(new GH_SAMParam(new GooProfileParam() { Name = "profile_", NickName = "profile_", Description = "SAM Analytical Profile", Optional = true, Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_name", NickName = "_name", Description = "Name of Profile", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "values_", NickName = "values_", Description = "Values", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "typeOrGroup_", NickName = "typeOrGroup_", Description = "SAM Analytical ProfileType or ProfileGroup", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooProfileParam() {Name = "profile", NickName = "profile", Description = "SAM Analytical Profile", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Updates PanelTypes for AdjacencyCluster
        /// </summary>
        public SAMAnalyticalCreateProfileByValues()
          : base("SAMAnalytical.CreateProfileByValues", "SAMAnalytical.CreateProfileByValues",
              "Creates SAM Analytical Profile By Values",
              "SAM", "Analytical01")
        {
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            index = Params.IndexOfInputParam("_name");
            if (index == -1)
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

            Profile profile = null;
            index = Params.IndexOfInputParam("profile_");
            if(index != -1)
            {
                dataAccess.GetData(index, ref profile);
            }

            if(profile == null)
            {
                profile = new Profile(name, ProfileGroup.Gain);
            }
            else
            {
                profile = new Profile(Guid.NewGuid(), profile, name, profile.Category);
            }

            index = Params.IndexOfInputParam("values_");
            if(index != -1)
            {
                List<double> values = new List<double>();
                if (dataAccess.GetDataList(index, values) && values != null && values.Count != 0)
                {
                    profile = new Profile(profile, values, profile.Category);
                }
            }

            index = Params.IndexOfInputParam("typeOrGroup_");
            if (index != -1)
            {
                string category = null;
                if (dataAccess.GetData(index, ref category) && !string.IsNullOrWhiteSpace(category))
                {
                    if (Core.Query.TryConvert(category, out ProfileType profileType))
                    {
                        category = profileType.Text();
                    }

                    if(string.IsNullOrWhiteSpace( category))
                    {
                        if (Core.Query.TryConvert(category, out ProfileGroup profileGroup))
                        {
                            category = profileGroup.Text();
                        }
                    }

                    if(!string.IsNullOrWhiteSpace(category))
                    {
                        profile = new Profile(Guid.NewGuid(), profile, category);
                    }
                }
            }

            index = Params.IndexOfOutputParam("profile");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooProfile(profile));
            }
        }
    }
}